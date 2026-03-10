using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void Task();

public class TaskExecutor : MonoBehaviour
{

    private Queue<Task> TaskQueue = new Queue<Task>();
    private object _queueLock = new object();
    
    void Update()
    {
        lock (_queueLock)
        {
            while (TaskQueue.Count > 0)
            {
                TaskQueue.Dequeue()();
            }
        }
    }

    public void ScheduleTask(Task newTask)
    {
        lock (_queueLock)
        {
            TaskQueue.Enqueue(newTask);
        }
    }
}