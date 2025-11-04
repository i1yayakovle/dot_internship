using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var graph = new Dictionary<string, HashSet<string>>();
        var gates = new HashSet<string>();
        var nodes = new HashSet<string>();

        foreach (var (u, v) in edges)
        {
            nodes.Add(u);
            nodes.Add(v);
            if (char.IsUpper(u[0])) gates.Add(u);
            if (char.IsUpper(v[0])) gates.Add(v);

            if (!graph.ContainsKey(u)) graph[u] = new HashSet<string>();
            if (!graph.ContainsKey(v)) graph[v] = new HashSet<string>();
            graph[u].Add(v);
            graph[v].Add(u);
        }

        var virusPos = "a";
        var result = new List<string>();

        while (true)
        {
            var activeGates = gates.Where(g => graph.ContainsKey(g) && graph[g].Count > 0).ToList();

            if (activeGates.Count == 0)
                break;

            var distances = new Dictionary<string, int>();
            var queue = new Queue<(string node, int dist)>();
            var visited = new HashSet<string>();

            queue.Enqueue((virusPos, 0));
            visited.Add(virusPos);

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();
                distances[current] = dist;

                if (graph.ContainsKey(current))
                {
                    foreach (var neighbor in graph[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, dist + 1));
                        }
                    }
                }
            }

            var minDist = int.MaxValue;
            string targetGate = null;

            foreach (var gate in activeGates)
            {
                if (distances.ContainsKey(gate))
                {
                    if (distances[gate] < minDist || (distances[gate] == minDist && string.Compare(gate, targetGate) < 0))
                    {
                        minDist = distances[gate];
                        targetGate = gate;
                    }
                }
            }

            if (targetGate == null)
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
                int cmp = string.Compare(x.gate, y.gate);
                if (cmp != 0) return cmp;
                return string.Compare(x.node, y.node);
            });

            string chosenAction = null;
            foreach (var (gate, node) in removable)
            {
                graph[gate].Remove(node);
                graph[node].Remove(gate);

                string nextPos = null;
                if (CanReachGate(virusPos, graph, gates, out nextPos))
                {
                    graph[gate].Add(node);
                    graph[node].Add(gate);
                }
                else
                {
                    chosenAction = $"{gate}-{node}";
                    break;
                }
            }

            if (chosenAction == null)
            {
                chosenAction = $"{removable[0].gate}-{removable[0].node}";
                graph[removable[0].gate].Remove(removable[0].node);
                graph[removable[0].node].Remove(removable[0].gate);
            }

            result.Add(chosenAction);

            virusPos = GetNextVirusPosition(virusPos, graph, gates);
            if (virusPos == null)
                break;

            if (gates.Contains(virusPos))
                break;
        }

        return result;
    }

    static bool CanReachGate(string virusPos, Dictionary<string, HashSet<string>> graph, HashSet<string> gates, out string nextPos)
    {
        nextPos = null;

        var distances = new Dictionary<string, int>();
        var queue = new Queue<(string node, int dist)>();
        var visited = new HashSet<string>();

        queue.Enqueue((virusPos, 0));
        visited.Add(virusPos);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();
            distances[current] = dist;

            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, dist + 1));
                    }
                }
            }
        }

        var reachableGates = gates.Where(g => distances.ContainsKey(g)).ToList();
        if (reachableGates.Count == 0)
            return false;

        var minDist = reachableGates.Min(g => distances[g]);
        var targetGate = reachableGates.Where(g => distances[g] == minDist)
                                       .OrderBy(g => g)
                                       .First();

        var parent = new Dictionary<string, string>();
        var revQueue = new Queue<string>();
        var revVisited = new HashSet<string>();

        revQueue.Enqueue(targetGate);
        revVisited.Add(targetGate);

        while (revQueue.Count > 0)
        {
            var current = revQueue.Dequeue();
            if (current == virusPos) break;

            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (!revVisited.Contains(neighbor))
                    {
                        revVisited.Add(neighbor);
                        parent[neighbor] = current;
                        revQueue.Enqueue(neighbor);
                    }
                }
            }
        }

        var path = new List<string>();
        var cur = virusPos;
        while (cur != targetGate)
        {
            if (!parent.ContainsKey(cur)) return false;
            cur = parent[cur];
            path.Add(cur);
        }

        if (path.Count > 0)
        {
            nextPos = path[0];
        }

        return true;
    }

    static string GetNextVirusPosition(string virusPos, Dictionary<string, HashSet<string>> graph, HashSet<string> gates)
    {
        var distances = new Dictionary<string, int>();
        var queue = new Queue<(string node, int dist)>();
        var visited = new HashSet<string>();

        queue.Enqueue((virusPos, 0));
        visited.Add(virusPos);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();
            distances[current] = dist;

            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, dist + 1));
                    }
                }
            }
        }

        var reachableGates = gates.Where(g => distances.ContainsKey(g) && graph[g].Count > 0).ToList();
        if (reachableGates.Count == 0)
            return null;

        var minDist = reachableGates.Min(g => distances[g]);
        var targetGate = reachableGates.Where(g => distances[g] == minDist)
                                       .OrderBy(g => g)
                                       .First();

        var level = new Dictionary<string, int>();
        var bfsQueue = new Queue<string>();
        bfsQueue.Enqueue(virusPos);
        level[virusPos] = 0;

        while (bfsQueue.Count > 0)
        {
            var current = bfsQueue.Dequeue();
            if (current == targetGate) break;

            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (!level.ContainsKey(neighbor))
                    {
                        level[neighbor] = level[current] + 1;
                        bfsQueue.Enqueue(neighbor);
                    }
                }
            }
        }

        var candidates = new List<string>();
        if (graph.ContainsKey(virusPos))
        {
            foreach (var neighbor in graph[virusPos])
            {
                if (level.ContainsKey(neighbor) && level[neighbor] == 1 && IsOnShortestPath(neighbor, targetGate, level, graph))
                {
                    candidates.Add(neighbor);
                }
            }
        }

        if (candidates.Count == 0)
            return null;

        candidates.Sort();
        return candidates[0];
    }

    static bool IsOnShortestPath(string from, string target, Dictionary<string, int> level, Dictionary<string, HashSet<string>> graph)
    {
        if (from == target) return true;

        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target) return true;

            if (graph.ContainsKey(current))
            {
                foreach (var neighbor in graph[current])
                {
                    if (!visited.Contains(neighbor) && level.ContainsKey(neighbor) && level[neighbor] == level[current] + 1)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return false;
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