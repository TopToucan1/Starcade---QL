using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class DaemonManager : MonoBehaviour
{
    private const int PORT = 1307;
    private const int BUFFER_SIZE = 8192;

    private static TaskExecutor taskExecutor;

    private TcpClient client;
    private Stream tcpStream;
    private byte[] buffer = new byte[BUFFER_SIZE];
    private StringBuilder messageBuilder = new StringBuilder();

    private delegate void XmlRequest(XmlWriter root);

    private static DaemonManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            return;
        }

        taskExecutor = GetComponent<TaskExecutor>();

        if (Game.platform != Platform.Board)
        {
            return;
        }

        try
        {
            client = new TcpClient("localhost", PORT);
            tcpStream = client.GetStream();
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

        Receive();
    }

    public static DaemonManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Request(XmlRequest request)
    {
        if (client == null)
        {
            Debug.Log("Request. Client is null");
            return;
        }

        StringWriter sw = new StringWriter();
        XmlWriter root = XmlWriter.Create(sw);
        root.WriteStartDocument();

        root.WriteStartElement("request");

        request(root);

        root.WriteEndElement();
        root.WriteEndDocument();
        root.Close();

        string str = sw.ToString();
        str = str.Replace("\n", "").Replace("\r", "");
        byte[] bytes = Encoding.UTF8.GetBytes(str + "\n\r\n\r");
        tcpStream.Write(bytes, 0, bytes.Length);
    }

    public void RequestSaveState()
    {
        Request(delegate (XmlWriter root)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string data = "";
            MemoryStream stream = new MemoryStream();
            bf.Serialize(stream, Game.State);
            data = Convert.ToBase64String(stream.ToArray());

            root.WriteStartElement("save");
            root.WriteAttributeString("fname", Saver.STATE_FILENAME);
            root.WriteAttributeString("game", Game.NAME);
            root.WriteValue(data);
            root.WriteEndElement();
        });
    }

    public void RequestSaveBalance()
    {
        if (!Game.State.IsFirstBallPlayed)
        {
            // Hack for purchase on first ball
            return;
        }

        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("set_balance");
            Dictionary<string, int> values = new Dictionary<string, int>()
            {
                { "play_balance", Game.State.Freeways },
                { "free_play_balance", Game.State.Promo },
                { "reward_balance", Game.State.Rewards },
            };
            foreach (var kvp in values)
            {
                root.WriteStartElement(kvp.Key);
                root.WriteValue(kvp.Value);
                root.WriteEndElement();
            }
            root.WriteEndElement();
        });
    }

    public void RequestSaveProgressive()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("set_progressive");
            foreach (var fund in Game.State.Progressives)
            {
                root.WriteStartElement(fund.Key);
                root.WriteAttributeString("issuance_status", (string)fund.Value["issuance_status"]);
                root.WriteValue(fund.Value["current"]);
                root.WriteEndElement();
            }
            root.WriteEndElement();
        });
    }

    public void RequestLoadState()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("load");
            root.WriteAttributeString("game", Game.NAME);
            root.WriteValue(Saver.STATE_FILENAME);
            root.WriteEndElement();
        });
    }

    public void RequestLoadBalance()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("get_balance");
            root.WriteEndElement();
        });
    }

    public void RequestLoadSettings()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("get_settings");
            root.WriteAttributeString("game", Game.NAME);

            root.WriteStartElement("credit_cost");
            root.WriteEndElement();
            root.WriteStartElement("bet");
            root.WriteEndElement();
            root.WriteStartElement("volume");
            root.WriteEndElement();
            root.WriteStartElement("ratio");
            root.WriteEndElement();

            root.WriteEndElement();
        });
    }

    public void RequestLoadProgressive()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("get_progressive");
            root.WriteEndElement();
        });
    }

    public void RequestSaveHistory()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("set_history");
            foreach (var h in Game.State.History)
            {
                root.WriteAttributeString(h.Key, h.Value.ToString());
            }
            root.WriteEndElement();
        });
    }

    public void RequestSaveStatistics()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("set_statistics");
            root.WriteAttributeString("game", Game.NAME);
            foreach (var s in Game.State.Statistics)
            {
                root.WriteAttributeString(s.Key, s.Value.ToString());
            }
            root.WriteEndElement();
        });
    }

    public void RequestLoadStatistics()
    {
        Request(delegate (XmlWriter root)
        {
            root.WriteStartElement("get_statistics");
            root.WriteAttributeString("game", Game.NAME);
            root.WriteEndElement();
        });
    }

    public void Exit()
    {
        if (client == null)
        {
            return;
        }

        StringWriter sw = new StringWriter();
        XmlWriter root = XmlWriter.Create(sw);
        root.WriteStartDocument();
        root.WriteStartElement("exit");
        root.WriteEndElement();
        root.WriteEndDocument();
        root.Close();

        string str = sw.ToString();
        str = str.Replace("\n", "").Replace("\r", "");
        byte[] bytes = Encoding.UTF8.GetBytes(str + "\n\r\n\r");
        tcpStream.Write(bytes, 0, bytes.Length);
    }

    private void Receive()
    {
        if (client == null)
        {
            Debug.Log("Receive. Client is null");
            return;
        }
        
        tcpStream.BeginRead(buffer, 0, BUFFER_SIZE, new AsyncCallback(ReceiveCallback), null);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int bytesRead = tcpStream.EndRead(ar);
            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            for (int i = 0; i < msg.Length; i++)
            {
                messageBuilder.Append(msg[i]);
                int indexOfEnd = messageBuilder.ToString().IndexOf("\n\r\n\r");
                if (indexOfEnd >= 0)
                {
                    ProcessMessage(messageBuilder.ToString().Substring(0, indexOfEnd));
                    messageBuilder.Remove(0, indexOfEnd + 4);
                }
            }
            tcpStream.BeginRead(buffer, 0, BUFFER_SIZE, new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void ProcessMessage(string msg)
    {
        XDocument doc = XDocument.Parse(msg);
        if (doc.Root.Name == "request")
        {
            ProcessRequest(doc);
        }
        else if (doc.Root.Name == "response")
        {
            ProcessResponse(doc);
        }
    }

    private void ProcessRequest(XDocument doc)
    {
        for (int i = 0; i < doc.Root.Elements().Count(); i++)
        {
            XElement elem = doc.Root.Elements().ElementAt(i);
            switch (elem.Name.ToString())
            {
                case "key_down":
                    switch (elem.Value)
                    {
                        case "button_1":
                            InputBroker.SetButtonDown("Plunge");
                            break;
                        case "button_2":
                            InputBroker.SetButtonDown("Left Flipper");
                            break;
                        case "button_3":
                            InputBroker.SetButtonDown("Right Flipper");
                            break;
                        case "button_4":
                            InputBroker.SetButtonDown("Exit");
                            break;
                    }
                    break;
                case "key_up":
                    switch (elem.Value)
                    {
                        case "button_1":
                            InputBroker.SetButtonUp("Plunge");
                            break;
                        case "button_2":
                            InputBroker.SetButtonUp("Left Flipper");
                            break;
                        case "button_3":
                            InputBroker.SetButtonUp("Right Flipper");
                            break;
                        case "button_4":
                            InputBroker.SetButtonUp("Exit");
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void ProcessResponse(XDocument doc)
    {
        for (int i = 0; i < doc.Root.Elements().Count(); i++)
        {
            XElement elem = doc.Root.Elements().ElementAt(i);
            switch (elem.Name.ToString())
            {
                case "load":
                    ProcessLoad(elem);
                    break;
                case "get_balance":
                    ProcessGetBalance(elem);
                    break;
                case "get_settings":
                    ProcessGetSettings(elem);
                    break;
                case "get_progressive":
                    ProcessGetProgressive(elem);
                    break;
                case "get_statistics":
                    ProcessGetStatistics(elem);
                    break;
                default:
                    break;
            }
        }
    }

    private void ProcessLoad(XElement elem)
    {
        if (elem.Attribute("status").Value == "ok")
        {
            MemoryStream stream = new MemoryStream();
            byte[] buffer = Convert.FromBase64String(elem.Value);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            taskExecutor.ScheduleTask(new Task(delegate
            {
                Game.LoadState(bf.Deserialize(stream) as GameState);
            }));
        }
        else
        {
            Debug.Log("Load fail");
        }
    }

    private void ProcessGetBalance(XElement elem)
    {
        if (elem.Attribute("status").Value == "ok")
        {
            for (int j = 0; j < elem.Elements().Count(); j++)
            {
                XElement balance = elem.Elements().ElementAt(j);
                int amount = -1;
                Int32.TryParse(balance.Value, out amount);
                if (balance.Name == "play_balance")
                    taskExecutor.ScheduleTask(new Task(delegate
                    {
                        Game.State.Freeways = amount;
                    }));
                if (balance.Name == "free_play_balance")
                    taskExecutor.ScheduleTask(new Task(delegate
                    {
                        Game.State.Promo = amount;
                    }));
                if (balance.Name == "play_reward_balance")
                    taskExecutor.ScheduleTask(new Task(delegate
                    {
                        Game.State.Rewards = amount;
                    }));
            }
        }
        else
        {
            Debug.Log("Get Balance fail");
        }
    }

    private void ProcessGetSettings(XElement elem)
    {
        if (elem.Attribute("status").Value == "ok")
        {
            foreach (var setting in elem.Elements())
            {
                string name = setting.Name.ToString();
                string value = setting.Value;
                string type = setting.Attribute("setting_type").Value;
                taskExecutor.ScheduleTask(new Task(delegate
                {
                    Game.State.SetSetting(name, value, type);
                }));
            }
        }
        else
        {
            Debug.Log("Get Settings fail");
        }
    }

    private void ProcessGetProgressive(XElement elem)
    {
        if (elem.Attribute("status").Value == "ok")
        {
            foreach (var jpParams in elem.Elements())
            {
                string jpName = jpParams.Name.ToString();
                foreach (var jpParam in jpParams.Elements())
                {
                    if (jpParam.Name.ToString() == "enabled")
                    {
                        bool enabled = jpParam.Value == "true";
                        taskExecutor.ScheduleTask(new Task(delegate
                        {
                            Game.State.SetProgressiveEnabled(jpName, enabled);
                        }));
                    }
                    else if (jpParam.Name.ToString() == "issuance_status")
                    {
                        string value = jpParam.Value;
                        taskExecutor.ScheduleTask(new Task(delegate
                        {
                            Game.State.SetProgressiveStatus(jpName, value);
                        }));
                    }
                    else
                    {
                        string paramName = jpParam.Name.ToString();
                        float value = float.Parse(jpParam.Value);
                        taskExecutor.ScheduleTask(new Task(delegate
                        {
                            Game.State.SetProgressiveParameter(jpName, paramName, value);
                        }));
                    }
                }
            }
        }
        else
        {
            Debug.Log("Get Progressive fail");
        }

        taskExecutor.ScheduleTask(new Task(delegate
        {
            Game.StartGame();
        }));
    }

    private void ProcessGetStatistics(XElement elem)
    {
        if (elem.Attribute("status").Value == "ok")
        {
            taskExecutor.ScheduleTask(new Task(delegate
            {
                Game.State.IncBetStatistics(Int32.Parse(elem.Attribute("total_bet").Value));
                Game.State.IncWinStatistics(Int32.Parse(elem.Attribute("total_win").Value));
            }));
        }
        else
        {
            Debug.Log("Get Statistics fail");
        }
    }
}
