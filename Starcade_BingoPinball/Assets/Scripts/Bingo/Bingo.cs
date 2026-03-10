using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public enum Combinations
{
    Nothing,
    FiveInRow,
    FourCorneres,
    Y,
    T,
    L,
    X,
    Z,
    Jackpot,
    FreeBalls
}

[System.Serializable]
public class Bingo
{
    public const int NUMBERS_TO_DRAW = 45;

    private static Dictionary<Combinations, List<int[]>> combinations;
    private static Dictionary<Combinations, float> paytable = new Dictionary<Combinations, float>()
    {
        { Combinations.Nothing, 0 },
        { Combinations.FiveInRow, 0.25f },
        { Combinations.FourCorneres, 1f },
        { Combinations.Y, 0.5f },
        { Combinations.T, 3 },
        { Combinations.L, 10 },
        { Combinations.X, 15 },
        { Combinations.Z, 25 },
        { Combinations.Jackpot, 0 },
        { Combinations.FreeBalls, 0 },
    };
    private static Dictionary<Combinations, float> freqs89 = new Dictionary<Combinations, float>()
    {
        { Combinations.Nothing, 0 },
        { Combinations.FiveInRow, 0.508f },
        { Combinations.FourCorneres, 0.11f },
        { Combinations.Y, 0.27f },
        { Combinations.T, 0.04f },
        { Combinations.L, 0.01f },
        { Combinations.X, 0.007f },
        { Combinations.Z, 0.005f },
        { Combinations.FreeBalls, 0.05f },
    };
    private static Dictionary<Combinations, float> freqs91 = new Dictionary<Combinations, float>()
    {
        { Combinations.Nothing, 0 },
        { Combinations.FiveInRow, 0.511f },
        { Combinations.FourCorneres, 0.105f },
        { Combinations.Y, 0.27f },
        { Combinations.T, 0.04f },
        { Combinations.L, 0.011f },
        { Combinations.X, 0.008f },
        { Combinations.Z, 0.005f },
        { Combinations.FreeBalls, 0.05f },
    };
    private static Dictionary<Combinations, float> freqs93 = new Dictionary<Combinations, float>()
    {
        { Combinations.Nothing, 0 },
        { Combinations.FiveInRow, 0.505f },
        { Combinations.FourCorneres, 0.11f },
        { Combinations.Y, 0.27f },
        { Combinations.T, 0.04f },
        { Combinations.L, 0.011f },
        { Combinations.X, 0.009f },
        { Combinations.Z, 0.005f },
        { Combinations.FreeBalls, 0.05f },
    };
    private BingoCard card = new BingoCard();
    private float win = 0;
    private float maxWin = 0;
    private int[] combinationNumbers;

    static Bingo()
    {
        InitializeCombinations();
    }

    public static Dictionary<Combinations, List<int[]>> Patterns
    {
        get
        {
            return combinations;
        }
    }

    public static Dictionary<Combinations, float> Paytable
    {
        get
        {
            return paytable;
        }
    }

    public static Dictionary<Combinations, float> Freqs
    {
        get
        {
            int ratio;
            if (Game.State == null)
                ratio = 93;
            else
                ratio = Game.State.Ratio;

            switch (ratio)
            {
                case 89: return freqs89; break;
                case 91: return freqs91; break;
                default: return freqs93; break;
            }
        }
    }

    public BingoCard Card
    {
        get
        {
            return card;
        }
    }

    public void NewGame(bool generateJpCombination)
    {
        card.ClearMarkedNumbers();
        card.GenerateRandomCard(generateJpCombination);

        // Count max possible win
        foreach (var n in card.NumbersToHit)
        {
            TrySetNumber(n);
        }
        Combinations combination = Bingo.CheckCombinations(card).Key;
        maxWin = paytable[combination];

        win = 0;
        card.ClearMarkedNumbers();
    }

    public void SetNumberByIndex(int index)
    {
        card.SetNumberByIndex(index);
        var combo = CheckCombinations(card);
        combinationNumbers = combo.Value;
        Combinations winningCombination = combo.Key;
        win = paytable[winningCombination];
    }

    public bool TrySetNumber(int number)
    {
        if (!card.TrySetNumber(number))
        {
            return false;
        }
        var combo = CheckCombinations(card);
        combinationNumbers = combo.Value;
        Combinations winningCombination = combo.Key;
        win = paytable[winningCombination];
        return true;
    }

    public int[] GetCombinationNumbers()
    {
        return combinationNumbers;
    }

    public Combinations GetCombination()
    {
        var combo = CheckCombinations(card);
        return combo.Key;
    }

    public float GetWin()
    {
        return win;
    }

    public float GetMaxWin()
    {
        return maxWin;
    }

    public string GetFieldString()
    {
        return card.GetFieldString();
    }

    public int PopIndexToHit()
    {
        return card.PopNumberToHit();
    }

    public int GetNumberToHit()
    {
        return card.GetNumberToHit();
    }

    public bool IsJackpotHit()
    {
        return CheckJackpot(card) != null;
    }

    public bool IsFreeBallsHit()
    {
        return CheckFreeBalls(card) != null;
    }

    public void SetJackpotNumbers()
    {
        foreach (var n in combinations[Combinations.Jackpot][0])
        {
            card.SetNumberByIndex(n);
        }
    }

    public void SetFreeNumbers()
    {
        foreach (var n in combinations[Combinations.FreeBalls][UnityEngine.Random.Range(0, 4)])
        {
            card.SetNumberByIndex(n);
        }
    }

    //////////////////////////////////////////////////////////////////
    public static KeyValuePair<Combinations, int[]> CheckCombinations(BingoCard card)
    {
        var markedNumbers = card.MarkedNumbersIndices;
        Combinations winningCombination = Combinations.Nothing;
        int[] numbers = null;
        foreach (var c in combinations)
        {
            foreach (var e in c.Value)
            {
                bool found = true;
                foreach (var index in e)
                {
                    if (!markedNumbers.Contains(index))
                    {
                        found = false;
                    }
                }
                if (found && paytable[c.Key] > paytable[winningCombination])
                {
                    winningCombination = c.Key;
                    numbers = e;
                }
            }
        }

        return new KeyValuePair<Combinations, int[]>(winningCombination, numbers);
    }

    public static int[] CheckJackpot(BingoCard card)
    {
        return CheckCombination(card, Combinations.Jackpot);
    }

    public static int[] CheckFreeBalls(BingoCard card)
    {
        return CheckCombination(card, Combinations.FreeBalls);
    }

    private static int[] CheckCombination(BingoCard card, Combinations combination)
    {
        var markedNumbers = card.MarkedNumbersIndices;
        int[] numbers = null;
        foreach (var e in combinations[combination])
        {
            bool found = true;
            foreach (var index in e)
            {
                if (!markedNumbers.Contains(index))
                {
                    found = false;
                }
            }
            if (found)
            {
                numbers = e;
            }
        }

        return numbers;
    }

    private static void InitializeCombinations()
    {
        List<int[]> nothing = new List<int[]>() { new int[] { } };
        List<int[]> fiveInRow = new List<int[]>();
        for (int i = 0; i < 5; i++)
        {
            // Horizontal
            fiveInRow.Add(Enumerable.Range(5 * i + 1, 5).Where(a => a != 13).ToArray());
            // Vertical
            fiveInRow.Add(Enumerable.Range(0, 5).Select(a => (a * 5) + i + 1).Where(a => a != 13).ToArray());
        }
        // Diagonal
        fiveInRow.Add(new int[] { 1, 7, 19, 25 });
        fiveInRow.Add(new int[] { 5, 9, 17, 21 });
        // 4 corners
        List<int[]> fourCorners = new List<int[]>
        {
            new int[] { 1, 5, 21, 25 }
        };
        // Y
        List<int[]> y = new List<int[]>
        {
            new int[] { 1, 7, 5, 9, 18, 23 },
            new int[] { 21, 17, 7, 1, 14, 15 },
            new int[] { 21, 17, 19, 25, 8, 3 },
            new int[] { 5, 9, 19, 25, 12, 11 }
        };
        // T
        List<int[]> t = new List<int[]>
        {
            new int[] { 1, 2, 3, 4, 5, 8, 18, 23 },
            new int[] { 21, 22, 23, 24, 25, 18, 8, 3 },
            new int[] { 21, 16, 11, 6, 1, 12, 14, 15 },
            new int[] { 5, 10, 15, 20, 25, 14, 12, 11 }
        };
        // L
        List<int[]> l = new List<int[]>
        {
            new int[] { 1, 6, 11, 16, 21, 22, 23, 24, 25 },
            new int[] { 5, 10, 15, 20, 25, 24, 23, 22, 21 },
            new int[] { 1, 2, 3, 4, 5, 10, 15, 20, 25 },
            new int[] { 5, 4, 3, 2, 1, 5, 11, 16, 21}
        };
        // X
        List<int[]> x = new List<int[]>
        {
            new int[] { 1, 7, 19, 25, 5, 9, 17, 21 }
        };
        // Z
        List<int[]> z = new List<int[]>
        {
            new int[] { 1, 2, 3, 4, 5, 9, 17, 21, 22, 23, 24, 25 },
            new int[] { 1, 6, 11, 16, 21, 7, 19, 5, 10, 15, 20, 25 }
        };
        // Jackpot
        List<int[]> jp = new List<int[]>
        {
            new int[] { 1, 2, 3, 4, 8, 16, 18, 22 }
        };
        // Free Balls
        List<int[]> freeBalls = new List<int[]>
        {
            new int[] { 1, 2, 3, 4, 6, 11, 12, 16, 21 },
            new int[] { 5, 10, 15, 20, 4, 3, 8, 2, 1 },
            new int[] { 25, 24, 23, 22, 20, 15, 14, 10, 5},
            new int[] { 21, 16, 11, 6, 22, 23, 18, 24, 25}
        };

        combinations = new Dictionary<Combinations, List<int[]>>
        {
            { Combinations.Nothing, nothing },
            { Combinations.FiveInRow, fiveInRow },
            { Combinations.FourCorneres, fourCorners },
            { Combinations.Y, y },
            { Combinations.T, t },
            { Combinations.L, l },
            { Combinations.X, x },
            { Combinations.Z, z },
            { Combinations.Jackpot, jp },
            { Combinations.FreeBalls, freeBalls }
        };
    }

    //////////////////////////////////////////////////////////////////
    public void Simulate()
    {
        const int N = 10000;
        const int freeBallsCount = 2;
        bool usePaytable = true;         // Count frequency using highest combination or all combinations on card
        Dictionary<Combinations, double> stats = new Dictionary<Combinations, double>()
        {
            { Combinations.Nothing, 0 },
            { Combinations.FiveInRow, 0 },
            { Combinations.FourCorneres, 0 },
            { Combinations.Y, 0 },
            { Combinations.T, 0 },
            { Combinations.L, 0 },
            { Combinations.X, 0 },
            { Combinations.Z, 0 },
            { Combinations.FreeBalls, 0 },
        };

        for (int i = 0; i < N; i++)
        {
            BingoCard card = new BingoCard();
            card.GenerateRandomCard();
            for (int j = 0; j < card.NumbersToHit.Count; j++)
            {
                card.TrySetNumber(card.NumbersToHit[j]);
            }
            if (usePaytable)
            {
                bool freeBalls = CheckFreeBalls(card) != null;
                Combinations win = CheckCombinations(card).Key;
                stats[win] += 1.0;
                if (freeBalls)
                {
                    // Subtract five in a row when free bals
                    //stats[win] -= 1;
                }
                if (freeBalls)
                {
                    stats[Combinations.FreeBalls] += 1.0;
                    i -= freeBallsCount;
                }
            }
            else
            {
                foreach (var c in combinations)
                {
                    if (c.Key == Combinations.Jackpot)
                    {
                        continue;
                    }
                    if (CheckCombination(card, c.Key) != null)
                    {
                        stats[c.Key] += 1.0;
                    }
                }
            }
        }

        double p = 0;
        double rtp = 0;
        foreach (var s in new Dictionary<Combinations, double>(stats))
        {
            if (s.Key != Combinations.FreeBalls)
            {
                p += s.Value / N;
                rtp += s.Value / N * paytable[s.Key];
            }
            stats[s.Key] = s.Value / N;
        }
        Debug.Log(string.Format("Nothing: {0}\nLine: {1}\nCorners: {2}\nY: {3}\nT: {4}\nL: {5}\nX: {6}\nZ: {7}\nFree: {8}",
            stats[Combinations.Nothing], stats[Combinations.FiveInRow], stats[Combinations.FourCorneres],
            stats[Combinations.Y], stats[Combinations.T], stats[Combinations.L], stats[Combinations.X], stats[Combinations.Z],
            stats[Combinations.FreeBalls]));
        Debug.Log("Hit Frequency: " + ((1 - stats[Combinations.Nothing])));
        Debug.Log("P: " + p);
        Debug.Log("RTP: " + rtp);
    }
}

// Bingo card class. 
// Indices count from left to right and from top to bottom
[System.Serializable]
public class BingoCard
{
    private int[,] numbers;
    private List<int> markedNumbersIndices;     // Indices of marked numbers [1; 25]
    private List<int> numbersToHit = new List<int>();   // Number existing on card
    private List<int> numbersAlreadyHit = new List<int>();
    private List<int> numbersNotOnCard = new List<int>();

    public BingoCard()
    {
        numbers = new int[5, 5];
        markedNumbersIndices = new List<int>();
        GenerateRandomCard();
    }

    // Returns true if number exits on card and it's not marked already
    public bool TrySetNumber(int number)
    {
        int column = (number - 1) / 15;
        for (int i = 0; i < 5; i++)
        {
            int index = i * 5 + column + 1;
            if (numbers[i, column] == number && !markedNumbersIndices.Contains(index))
            {
                markedNumbersIndices.Add(index);
                numbersAlreadyHit.Add(number);
                return true;
            }
        }

        return false;
    }

    public void SetNumberByIndex(int index)
    {
        markedNumbersIndices.Add(index);
    }

    public void GenerateRandomCard(bool generateJpCpmbination = false)
    {
        numbersNotOnCard = Enumerable.Range(1, 75).ToList();

        for (int i = 0; i < 5; i++)
        {
            var range = Enumerable.Range(15 * i + 1, 15).ToList();
            for (int j = 0; j < 5; j++)
            {
                var index = UnityEngine.Random.Range(0, range.Count);
                numbers[j, i] = range[index];
                numbersNotOnCard.Remove(range[index]);
                range.RemoveAt(index);
            }
        }

        GenerateNumbersToHit(generateJpCpmbination);
    }

    // Generate numbers, that will be checked on card
    public void GenerateNumbersToHit(bool generateJpCpmbination)
    {
        numbersAlreadyHit.Clear();
        numbersToHit.Clear();

        Combinations combination = Combinations.Nothing;
        
        // Generate winning pattern
        if (generateJpCpmbination)
        {
            combination = Combinations.Jackpot;
        }
        else
        {
            float r = UnityEngine.Random.Range(0f, 1f);
            float sum = 0;
            foreach (var f in Bingo.Freqs)
            {
                if (r < f.Value + sum)
                {
                    combination = f.Key;
                    break;
                }
                sum += f.Value;
            }
        }
        Debug.Log("Win combo: " + combination);

        Func<bool> check = () =>
        {
            KeyValuePair<Combinations, int[]> c = Bingo.CheckCombinations(this);
            bool freeBalls = Bingo.CheckFreeBalls(this) != null;
            float targetWin = Bingo.Paytable[combination];
            if (freeBalls)
            {
                // Free Balls pattern always contains five in a row
                targetWin = Bingo.Paytable[Combinations.FiveInRow];
            }
            return freeBalls && combination != Combinations.FreeBalls || generateJpCpmbination && (Bingo.Paytable[c.Key] > 0 || freeBalls) ||
               Bingo.Paytable[c.Key] > targetWin || (!generateJpCpmbination && Bingo.CheckJackpot(this) != null);
        };

        markedNumbersIndices.Clear();

        // Add numbers of pattern
        List<int[]> availablePatterns = new List<int[]>(Bingo.Patterns[combination]);
        while (availablePatterns.Count != 0)
        {
            int index = UnityEngine.Random.Range(0, availablePatterns.Count);
            int[] indices = availablePatterns[index];
            availablePatterns.RemoveAt(index);

            foreach (var i in indices)
            {
                SetNumberByIndex(i);
            }
            if (check())
            {
                break;
            }

            foreach (var i in indices)
            {
                numbersToHit.Add(GetNumberByIndex(i));
            }
        }
        // Add random numbers
        List<int> range = Enumerable.Range(1, 25).ToList();
        while (range.Count != 0)
        {
            int index = UnityEngine.Random.Range(0, range.Count);
            int numberIndex = range[index];
            range.RemoveAt(index);
            
            SetNumberByIndex(numberIndex);
            
            if (check())
            {
                break;
            }
            
            numbersToHit.Add(GetNumberByIndex(numberIndex));
        }

        markedNumbersIndices.Clear();
    }

    public int PopNumberToHit()
    {
        if (numbersToHit.Count == 0)
        {
            Debug.Log("No numbers to hit");
            return -1;
        }
        int number = numbersToHit[0];
        numbersToHit.RemoveAt(0);
        return number;
    }

    public int GetNumberToHit()
    {
        const float notCardNumberProbability = 0.0f;
        if (numbersToHit.Count == 0 || UnityEngine.Random.Range(0, 1f) < notCardNumberProbability)
        {
            // Return random non-card number
            return GetNumberByIndex(-1);
        }
        return numbersToHit[0];
    }

    public int GetNumber(int x, int y)
    {
        return numbers[x, y];
    }

    public int GetNumberByIndex(int index)
    {
        if (index < 0)
        {
            int[] possibleNumbers = Enumerable.Concat(numbersNotOnCard, markedNumbersIndices).ToArray();
            int randomIndex = UnityEngine.Random.Range(0, possibleNumbers.Length);
            int number = possibleNumbers[randomIndex];
            return number;
        }
        return numbers[(index - 1) / 5, (index - 1) % 5];
    }

    public int[] GetIndexOfNumber(int number)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (numbers[i, j] == number)
                {
                    return new int[] { i, j };
                }
            }
        }
        return null;
    }

    public bool IsNumberOnCard(int number)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (numbers[i, j] == number)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int[,] Numbers
    {
        get
        {
            return numbers;
        }
    }

    public List<int> MarkedNumbersIndices
    {
        get
        {
            return markedNumbersIndices;
        }
    }

    public void ClearMarkedNumbers()
    {
        markedNumbersIndices.Clear();
    }

    public List<int> NumbersToHit
    {
        get
        {
            return numbersToHit;
        }
    }

    public List<int> NumbersNotOnCard
    {
        get
        {
            return numbersNotOnCard;
        }
    }

    public List<int> NumbersAlreadyHit
    {
        get
        {
            return numbersAlreadyHit;
        }
    }

    public string GetFieldString()
    {
        StringBuilder field = new StringBuilder();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (i == 2 && j == 2)
                {
                    field.Append("X ");
                    continue;
                }
                int index = i * 5 + j + 1;
                bool isMarked = false;
                if (markedNumbersIndices.Contains(index))
                {
                    isMarked = true;
                }
                field.Append((isMarked ? "(" : "") + numbers[i, j] + (isMarked ? ")" : "") + " ");
            }
            field.Append("\n");
        }
        return field.ToString();
    }
}
