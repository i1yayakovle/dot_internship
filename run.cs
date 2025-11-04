using System;
using System.Collections.Generic;

class Program
{
    static int Solve(List<string> lines)
    {
        if (lines.Count != 5 && lines.Count != 7)
            return -1;

        char[] hall = new char[11];
        for (int i = 0; i < 11; i++)
        {
            hall[i] = lines[1][i + 1];
        }

        int depth = lines.Count == 5 ? 2 : 4;
        char[][] rooms = new char[4][];

        if (depth == 2)
        {
            rooms[0] = new char[] { lines[2][3], lines[3][3] };
            rooms[1] = new char[] { lines[2][5], lines[3][5] };
            rooms[2] = new char[] { lines[2][7], lines[3][7] };
            rooms[3] = new char[] { lines[2][9], lines[3][9] };
        }
        else
        {
            rooms[0] = new char[] { lines[2][3], lines[3][3], lines[4][3], lines[5][3] };
            rooms[1] = new char[] { lines[2][5], lines[3][5], lines[4][5], lines[5][5] };
            rooms[2] = new char[] { lines[2][7], lines[3][7], lines[4][7], lines[5][7] };
            rooms[3] = new char[] { lines[2][9], lines[3][9], lines[4][9], lines[5][9] };
        }

        char[][] target = new char[4][];
        string types = "ABCD";
        for (int i = 0; i < 4; i++)
        {
            target[i] = new char[depth];
            for (int j = 0; j < depth; j++)
            {
                target[i][j] = types[i];
            }
        }

        var pq = new PriorityQueue<(char[], char[][]), int>();
        var dist = new Dictionary<string, int>();

        string KeyState(char[] h, char[][] r)
        {
            string roomPart = "";
            for (int i = 0; i < 4; i++)
            {
                if (i > 0) roomPart += "|";
                roomPart += new string(r[i]);
            }
            return new string(h) + "|" + roomPart;
        }

        string startKey = KeyState(hall, rooms);
        dist[startKey] = 0;
        pq.Enqueue((hall, rooms), 0);

        int[] roomEntry = { 2, 4, 6, 8 };
        int[] costs = { 1, 10, 100, 1000 };

        while (pq.Count > 0)
        {
            var state = pq.Dequeue();
            char[] hl = state.Item1;
            char[][] rms = state.Item2;

            string currentKey = KeyState(hl, rms);
            int currentEnergy = dist[currentKey];

            bool isTarget = true;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    if (rms[i][j] != target[i][j])
                    {
                        isTarget = false;
                        break;
                    }
                }
                if (!isTarget) break;
            }
            if (isTarget)
                return currentEnergy;

            for (int roomIdx = 0; roomIdx < 4; roomIdx++)
            {
                int level = 0;
                while (level < depth && rms[roomIdx][level] == '.')
                    level++;
                if (level == depth) continue;

                char letter = rms[roomIdx][level];
                int letterType = types.IndexOf(letter);
                if (letterType == -1) continue;

                if (letterType == roomIdx)
                {
                    bool allCorrect = true;
                    for (int k = level; k < depth; k++)
                    {
                        if (rms[roomIdx][k] != types[roomIdx])
                        {
                            allCorrect = false;
                            break;
                        }
                    }
                    if (allCorrect) continue;
                }

                int stepsOut = level + 1;
                int entry = roomEntry[roomIdx];

                for (int pos = 0; pos < 11; pos++)
                {
                    if (pos == 2 || pos == 4 || pos == 6 || pos == 8)
                        continue;
                    if (hl[pos] != '.')
                        continue;
                    if (!IsPathClear(hl, entry, pos))
                        continue;

                    int totalSteps = stepsOut + Math.Abs(pos - entry);
                    int newEnergy = currentEnergy + totalSteps * costs[letterType];

                    char[] newHall = new char[11];
                    Array.Copy(hl, newHall, 11);
                    newHall[pos] = letter;

                    char[][] newRooms = new char[4][];
                    for (int i = 0; i < 4; i++)
                    {
                        newRooms[i] = new char[depth];
                        Array.Copy(rms[i], newRooms[i], depth);
                    }
                    newRooms[roomIdx][level] = '.';

                    string newKey = KeyState(newHall, newRooms);
                    if (!dist.ContainsKey(newKey) || newEnergy < dist[newKey])
                    {
                        dist[newKey] = newEnergy;
                        pq.Enqueue((newHall, newRooms), newEnergy);
                    }
                }
            }

            for (int pos = 0; pos < 11; pos++)
            {
                if (hl[pos] == '.') continue;
                char letter = hl[pos];
                int letterType = types.IndexOf(letter);
                if (letterType == -1) continue;

                int targetRoom = letterType;
                int entry = roomEntry[targetRoom];

                bool canEnter = true;
                for (int k = 0; k < depth; k++)
                {
                    if (rms[targetRoom][k] != '.' && rms[targetRoom][k] != types[targetRoom])
                    {
                        canEnter = false;
                        break;
                    }
                }
                if (!canEnter) continue;
                if (!IsPathClear(hl, pos, entry)) continue;

                int level = depth - 1;
                while (level >= 0 && rms[targetRoom][level] != '.')
                    level--;
                if (level < 0) continue;

                int totalSteps = (level + 1) + Math.Abs(pos - entry);
                int newEnergy = currentEnergy + totalSteps * costs[letterType];

                char[] newHall = new char[11];
                Array.Copy(hl, newHall, 11);
                newHall[pos] = '.';

                char[][] newRooms = new char[4][];
                for (int i = 0; i < 4; i++)
                {
                    newRooms[i] = new char[depth];
                    Array.Copy(rms[i], newRooms[i], depth);
                }
                newRooms[targetRoom][level] = letter;

                string newKey = KeyState(newHall, newRooms);
                if (!dist.ContainsKey(newKey) || newEnergy < dist[newKey])
                {
                    dist[newKey] = newEnergy;
                    pq.Enqueue((newHall, newRooms), newEnergy);
                }
            }
        }

        return -1;
    }

    static bool IsPathClear(char[] hall, int from, int to)
    {
        int step = from < to ? 1 : -1;
        for (int i = from; i != to; i += step)
        {
            if (i != from && hall[i] != '.')
                return false;
        }
        return hall[to] == '.';
    }

    static void Main()
    {
        var lines = new List<string>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        int result = Solve(lines);
        Console.WriteLine(result);
    }
}