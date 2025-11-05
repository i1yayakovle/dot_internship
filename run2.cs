using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var graph = new Dictionary<string, HashSet<string>>();
        var gates = new HashSet<string>();

        foreach (var (u, v) in edges)
        {
            if (char.IsUpper(u[0])) gates.Add(u);
            if (char.IsUpper(v[0])) gates.Add(v);

            if (!graph.ContainsKey(u)) graph[u] = new HashSet<string>();
            if (!graph.ContainsKey(v)) graph[v] = new HashSet<string>();
            graph[u].Add(v);
            graph[v].Add(u);
        }

        string virusPos = "a";
        var result = new List<string>();

        while (true)
        {
            var activeGates = gates.Where(g => graph.ContainsKey(g) && graph[g].Count > 0).ToList();
            if (activeGates.Count == 0)
                break;

            var removable = new List<(string gate, string node)>();
            foreach (var gate in activeGates)
            {
                foreach (var node in graph[gate])
                {
                    removable.Add((gate, node));
                }
            }

            removable.Sort((x, y) =>
            {
                int cmp = string.Compare(x.gate, y.gate, StringComparison.Ordinal);
                if (cmp != 0) return cmp;
                return string.Compare(x.node, y.node, StringComparison.Ordinal);
            });

            string chosenAction = null;
            string newVirusPos = null;

            foreach (var (gate, node) in removable)
            {
                graph[gate].Remove(node);
                graph[node].Remove(gate);

                string nextPos = GetNextVirusPosition(virusPos, graph, gates);

                if (nextPos == null || !gates.Contains(nextPos))
                {
                    chosenAction = $"{gate}-{node}";
                    newVirusPos = nextPos;
                    break;
                }
                else
                {
                    graph[gate].Add(node);
                    graph[node].Add(gate);
                }
            }

            if (chosenAction == null)
            {
                var first = removable[0];
                chosenAction = $"{first.gate}-{first.node}";
                graph[first.gate].Remove(first.node);
                graph[first.node].Remove(first.gate);
                newVirusPos = GetNextVirusPosition(virusPos, graph, gates);
            }

            result.Add(chosenAction);
            virusPos = newVirusPos;

            if (virusPos == null)
                break;
        }

        return result;
    }

    static string GetNextVirusPosition(string startPos, Dictionary<string, HashSet<string>> graph, HashSet<string> gates)
    {
        var distFromStart = new Dictionary<string, int>();
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        queue.Enqueue(startPos);
        visited.Add(startPos);
        distFromStart[startPos] = 0;

        while (queue.Count > 0)
        {
            string u = queue.Dequeue();
            if (graph.TryGetValue(u, out var neighbors))
            {
                foreach (string v in neighbors.OrderBy(x => x, StringComparer.Ordinal))
                {
                    if (!visited.Contains(v))
                    {
                        visited.Add(v);
                        distFromStart[v] = distFromStart[u] + 1;
                        queue.Enqueue(v);
                    }
                }
            }
        }

        string targetGate = null;
        int minDist = int.MaxValue;
        foreach (string gate in gates)
        {
            if (distFromStart.ContainsKey(gate) && graph.ContainsKey(gate) && graph[gate].Count > 0)
            {
                if (distFromStart[gate] < minDist ||
                    (distFromStart[gate] == minDist && string.Compare(gate, targetGate, StringComparison.Ordinal) < 0))
                {
                    minDist = distFromStart[gate];
                    targetGate = gate;
                }
            }
        }

        if (targetGate == null)
            return null;

        var distToTarget = new Dictionary<string, int>();
        var revQueue = new Queue<string>();
        var revVisited = new HashSet<string>();
        revQueue.Enqueue(targetGate);
        revVisited.Add(targetGate);
        distToTarget[targetGate] = 0;

        while (revQueue.Count > 0)
        {
            string u = revQueue.Dequeue();
            if (graph.TryGetValue(u, out var neighbors))
            {
                foreach (string v in neighbors)
                {
                    if (!revVisited.Contains(v))
                    {
                        revVisited.Add(v);
                        distToTarget[v] = distToTarget[u] + 1;
                        revQueue.Enqueue(v);
                    }
                }
            }
        }

        var candidates = new List<string>();
        if (graph.TryGetValue(startPos, out var startNeighbors))
        {
            foreach (string n in startNeighbors)
            {
                if (distFromStart.TryGetValue(n, out int d1) && distToTarget.TryGetValue(n, out int d2))
                {
                    if (d1 + d2 == distFromStart[targetGate])
                    {
                        candidates.Add(n);
                    }
                }
            }
        }

        if (candidates.Count == 0)
            return null;

        candidates.Sort(StringComparer.Ordinal);
        return candidates[0];
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}