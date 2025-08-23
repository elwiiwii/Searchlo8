using System.Collections.Immutable;

namespace SearchAlgorithm;

public readonly struct ActionsStruct : IEquatable<ActionsStruct>
{
    public readonly ImmutableArray<int> Path;
    private readonly int _hashCode;

    public ActionsStruct(IEnumerable<int> path)
    {
        Path = path.ToImmutableArray();
        _hashCode = CalculateHashCode(Path);
    }

    public bool Equals(ActionsStruct other) => Path.SequenceEqual(other.Path);
    public override bool Equals(object obj) => obj is ActionsStruct other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private static int CalculateHashCode(ImmutableArray<int> path)
    {
        unchecked
        {
            int hash = 19;
            foreach (int item in path)
                hash = hash * 31 + item;
            return hash;
        }
    }
}