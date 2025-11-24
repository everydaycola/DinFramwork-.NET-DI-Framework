using QuikGraph;
using QuikGraph.Algorithms;

namespace DinClassLibrary;

public class DinDependencyGraph
{
    private readonly BidirectionalGraph<Type, Edge<Type>> _graph = new();
    
    public void AddVertex(Type vertex)
    {
        _graph.AddVertex(vertex);
    }
    
    public void AddEdge(Type source, Type target)
    {
        _graph.AddEdge(new Edge<Type>(source, target));
    }
    
    public void PrintGraph(Type startNode)
    {
        Console.WriteLine("Dependency graph:\n============");
        
        var sb = new System.Text.StringBuilder();
        var printedVerticesCount = 0;
        
        PrintVertex(startNode, 0, sb, ref printedVerticesCount);
        
        Console.WriteLine($"Total vertices: {printedVerticesCount}");
        Console.WriteLine($"Unique vertices: {_graph.VertexCount}");
        Console.WriteLine($"Total edges: {_graph.EdgeCount}");
        Console.Write(sb.ToString());
    }
    
    private void PrintVertex(Type vertex, int level, System.Text.StringBuilder sb, ref int count)
    {
        count++;
        sb.AppendLine($"{new string(' ', level * 2)}├─ {vertex.Name}");
        
        foreach (var edge in _graph.OutEdges(vertex))
        {
            PrintVertex(edge.Target, level + 1, sb, ref count);
        }
    }
}