using System.Text;
using QuikGraph;

namespace DinClassLibrary;

public class DinDependencyGraph
{
    private readonly BidirectionalGraph<Type, Edge<Type>> _graph = new();

    public void Clear()
    {
        _graph.Clear();
    }

    public void AddVertex(Type vertex)
    {
        _graph.AddVertex(vertex);
    }

    public void AddEdge(Type source, Type target)
    {
        // Ensure vertices exist before adding the edge
        _graph.AddVertex(source);
        _graph.AddVertex(target);
        _graph.AddEdge(new Edge<Type>(source, target));
    }

    public void CheckForCycles()
    {
        // Simple unoptimized DFS from every node to detect cycles
        foreach (var vertex in _graph.Vertices)
        {
            CheckVertexForCycle(vertex, new List<Type>());
        }
    }

    private void CheckVertexForCycle(Type current, List<Type> path)
    {
        // If the current node is already in the recursion path, we found a cycle
        if (path.Contains(current))
        {
            var cycleStartIndex = path.IndexOf(current);
            var cyclePath = path.Skip(cycleStartIndex).ToList();
            cyclePath.Add(current); // Close the loop visually

            var cycleString = string.Join(" -> ", cyclePath.Select(t => t.Name));
            throw new InvalidOperationException($"Circular dependency detected: {cycleString}");
        }

        path.Add(current);

        foreach (var edge in _graph.OutEdges(current))
        {
            CheckVertexForCycle(edge.Target, path);
        }

        // Backtrack
        path.RemoveAt(path.Count - 1);
    }

    public void PrintGraph(Type startNode)
    {
        DinLogger.LogInfo("Dependency graph:");

        var sb = new StringBuilder();
        var printedVerticesCount = 0;

        PrintVertex(startNode, 0, sb, ref printedVerticesCount);

        DinLogger.LogInfo(
            $"""
             
             Total vertices: {printedVerticesCount}
             Unique vertices: {_graph.VertexCount}
             Total edges: {_graph.EdgeCount}
             {sb}
             """);
    }

    private void PrintVertex(Type vertex, int level, StringBuilder sb, ref int count)
    {
        count++;
        sb.AppendLine($"{new string(' ', level * 2)}├─ {vertex.Name}");

        foreach (var edge in _graph.OutEdges(vertex))
        {
            PrintVertex(edge.Target, level + 1, sb, ref count);
        }
    }
}