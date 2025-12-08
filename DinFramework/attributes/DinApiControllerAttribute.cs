using System;

namespace DinClassLibrary.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DinApiControllerAttribute : Attribute
{
    public string Segment { get; }

    public DinApiControllerAttribute(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Segment must be a non-empty string", nameof(segment));
        Segment = segment.Trim('/');
    }
}
