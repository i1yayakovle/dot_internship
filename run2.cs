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
                return cmp != 0 ? cmp : string.Compare(x.node, y.node, StringComparison.Ordinal);
            });

            string chosenAction = null;

            foreach (var (gate, node) in removable)
            {
                graph[gate].Remove(node);
                graph[node].Remove(gate);

                bool canReach = CanReachAnyActiveGate(virusPos, graph, gates);

                if (!canReach)
                {
                    chosenAction = $"{gate}-{node}";
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
            }

            result.Add(chosenAction);

            virusPos = GetNextVirusPosition(virusPos, graph, gates);
            if (virusPos == null || gates.Contains(virusPos))
                break;
        }

        return result;
    }

    static bool CanReachAnyActiveGate(string startPos, Dictionary<string, HashSet<string>> graph, HashSet<string> gates)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();

            if (gates.Contains(current))
            {
                HashSet<string> gateNeighbors;
                if (graph.TryGetValue(current, out gateNeighbors) && gateNeighbors.Count > 0)
                    return true;
            }

            HashSet<string> neighbors;
            if (graph.TryGetValue(current, out neighbors))
            {
                foreach (string neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        return false;
    }

    static string GetNextVirusPosition(string startPos, Dictionary<string, HashSet<string>> graph, HashSet<string> gates)
    {
        var dist = new Dictionary<string, int>();
        var parent = new Dictionary<string, List<string>>();
        var queue = new Queue<string>();
        var visited = new HashSet<string>();

        queue.Enqueue(startPos);
        visited.Add(startPos);
        dist[startPos] = 0;
        parent[startPos] = new List<string>();

        while (queue.Count > 0)
        {
            string u = queue.Dequeue();
            if (graph.TryGetValue(u, out var adjNodes))
            {
                foreach (string v in adjNodes)
                {
                    if (!visited.Contains(v))
                    {
                        visited.Add(v);
                        dist[v] = dist[u] + 1;
                        parent[v] = new List<string> { u };
                        queue.Enqueue(v);
                    }
                    else if (dist.ContainsKey(v) && dist[v] == dist[u] + 1)
                    {
                        parent[v].Add(u);
                    }
                }
            }
        }

        string targetGate = null;
        int minDist = int.MaxValue;
        foreach (string gate in gates)
        {
            if (dist.ContainsKey(gate) && graph.ContainsKey(gate) && graph[gate].Count > 0)
            {
                int d = dist[gate];
                if (d < minDist || (d == minDist && string.Compare(gate, targetGate, StringComparison.Ordinal) < 0))
                {
                    minDist = d;
                    targetGate = gate;
                }
            }
        }

        if (targetGate == null)
            return null;

        var onShortestPath = new HashSet<string>();
        var pathQueue = new Queue<string>();
        pathQueue.Enqueue(targetGate);
        onShortestPath.Add(targetGate);

        while (pathQueue.Count > 0)
        {
            string u = pathQueue.Dequeue();
            if (parent.TryGetValue(u, out var preds))
            {
                foreach (string p in preds)
                {
                    if (!onShortestPath.Contains(p))
                    {
                        onShortestPath.Add(p);
                        pathQueue.Enqueue(p);
                    }
                }
            }
        }

        var candidates = new List<string>();
        if (graph.TryGetValue(startPos, out var neighbors))
        {
            foreach (string n in neighbors)
            {
                if (onShortestPath.Contains(n))
                {
                    candidates.Add(n);
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