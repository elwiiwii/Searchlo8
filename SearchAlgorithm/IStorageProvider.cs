namespace SearchAlgorithm;

public interface IStorageProvider
{
    public long Count();
    public void Add(int dist, ActionsStruct actions, GameState state);
    public void Remove(int dist, ActionsStruct actions);
    public void CullTo(long count);
    public List<(int dist, ActionsStruct actions, GameState state)> GetOrderedSnapshotAsList();
}