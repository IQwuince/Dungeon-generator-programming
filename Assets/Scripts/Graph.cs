using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencyList;

    public Graph()
    {
        adjacencyList = new Dictionary<T, List<T>>();
    }

    public void Clear()
    {
        adjacencyList.Clear();
    }

    public void RemoveNode(T node)
    {
        if (adjacencyList.ContainsKey(node))
        {
            adjacencyList.Remove(node);
        }

        foreach (var key in adjacencyList.Keys)
        {
            adjacencyList[key].Remove(node);
        }
    }

    public List<T> GetNodes()
    {
        return new List<T>(adjacencyList.Keys);
    }

    public void AddNode(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
    }

    public void RemoveEdge(T fromNode, T toNode)
    {
        if (adjacencyList.ContainsKey(fromNode))
        {
            adjacencyList[fromNode].Remove(toNode);
        }
        if (adjacencyList.ContainsKey(toNode))
        {
            adjacencyList[toNode].Remove(fromNode);
        }
    }

    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode))
        {
            AddNode(fromNode);
        }
        if (!adjacencyList.ContainsKey(toNode))
        {
            AddNode(toNode);
        }

        adjacencyList[fromNode].Add(toNode);
        adjacencyList[toNode].Add(fromNode);
    }

    public List<T> GetNeighbors(T node)
    {
        return new List<T>(adjacencyList[node]);
    }

    public int GetNodeCount()
    {
        return adjacencyList.Count;
    }

    public void PrintGraph()
    {
        foreach (var node in adjacencyList)
        {
            Debug.Log($"{node.Key}: {string.Join(", ", node.Value)}");
        }
    }

    // Breadth-First Search (BFS)
    public void BFS(T startNode)
    {
        Queue<T> queue = new();
        List<T> visited = new();

        queue.Enqueue(startNode);
        visited.Add(startNode);
        while (queue.Count > 0)
        {
            startNode = queue.Dequeue();
            Debug.Log(startNode);

            foreach (var neighbour in adjacencyList[startNode])
            {
                if (!visited.Contains(neighbour))
                {
                    queue.Enqueue(neighbour);
                    visited.Add(neighbour);
                }

            }
        }

    }


    // Depth-First Search (DFS)
    public void DFS(T startNode)
    {
        Stack<T> stack = new();
        List<T> visited = new();

        stack.Push(startNode);
        visited.Add(startNode);
        while (stack.Count > 0)
        {
            startNode = stack.Pop();
            Debug.Log(startNode);

            foreach (var neighbour in adjacencyList[startNode])
            {
                if (!visited.Contains(neighbour))
                {
                    stack.Push(neighbour);
                    visited.Add(neighbour);
                }

            }
        }

    }
}