using QuikGraph;
using QuikGraph.Algorithms;

namespace DIN_ClassLibrary;

public class DependencyGraph
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
    
    public void PrintGraph()
    {
        Console.WriteLine("Dependency graph:\n============");
        Console.WriteLine($"Total vertices: {_graph.VertexCount}");
        Console.WriteLine($"Total edges: {_graph.EdgeCount}");
        try
        {
            _graph
                .TopologicalSort()
                .Aggregate(
                    new HashSet<Type>(),
                    (current, vertex) =>
                    {
                        if (!current.Contains(vertex))
                            PrintVertex(vertex, current, 0);
                        return current;
                    });
        }
        catch (NonAcyclicGraphException e)
        {
            Console.WriteLine("Error:" + e.Message);
        }
    }
    
    private HashSet<Type> PrintVertex(Type vertex, HashSet<Type> visited, int level)
    {
        
        visited.Add(vertex);
        Console.WriteLine($"{new string(' ', level * 2)}├─ {vertex.Name}");
        
        return _graph.OutEdges(vertex)
            .Where(edge => !visited.Contains(edge.Target))
            .Aggregate(visited, (current, edge) => PrintVertex(edge.Target, current, level + 1));
    }
}