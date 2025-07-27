namespace Searchlo8;

public sealed class ConcurrentOrderedStateMap
{
    private readonly Partition[] _partitions;
    private readonly int _partitionCount;
    private readonly int _rangeSize;

    public ConcurrentOrderedStateMap(int minKey, int maxKey, int? concurrencyLevel = null)
    {
        _partitionCount = concurrencyLevel ?? Environment.ProcessorCount;
        _partitions = new Partition[_partitionCount];
        _rangeSize = Math.Min(10000, Math.Max(1000, (maxKey - minKey) / _partitionCount));

        for (int i = 0; i < _partitionCount; i++)
        {
            int partitionMin = minKey + i * _rangeSize;
            int partitionMax = Math.Min(minKey + (i + 1) * _rangeSize - 1, maxKey);
            _partitions[i] = new Partition(partitionMin, partitionMax);
        }
    }

    public void Add(int dist, ActionsStruct actions, GameState state)
    {
        var partition = GetPartition(dist);
        partition.Add(dist, actions, state);
    }

    public bool TryGet(int dist, ActionsStruct actions, out GameState state)
    {
        var partition = GetPartition(dist);
        return partition.TryGet(dist, actions, out state);
    }

    public IEnumerable<(int dist, ActionsStruct actions, GameState state)> GetOrdered()
    {
        foreach (var partition in _partitions)
        {
            foreach (var item in partition.GetOrderedItems())
            {
                yield return item;
            }
        }
    }

    private Partition GetPartition(int dist)
    {
        int partitionIndex = (dist - _partitions[0].MinKey) / _rangeSize;
        partitionIndex = Math.Clamp(partitionIndex, 0, _partitionCount - 1);
        return _partitions[partitionIndex];
    }

    private sealed class Partition
    {
        private readonly SortedDictionary<int, Dictionary<ActionsStruct, GameState>> _storage;
        private readonly ReaderWriterLockSlim _lock = new();
        public readonly int MinKey;
        public readonly int MaxKey;

        public Partition(int minKey, int maxKey)
        {
            MinKey = minKey;
            MaxKey = maxKey;
            _storage = new SortedDictionary<int, Dictionary<ActionsStruct, GameState>>();
        }

        public void Add(int dist, ActionsStruct actions, GameState state)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_storage.TryGetValue(dist, out var innerDict))
                {
                    innerDict = new Dictionary<ActionsStruct, GameState>();
                    _storage.Add(dist, innerDict);
                }
                innerDict[actions] = state;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGet(int dist, ActionsStruct actions, out GameState state)
        {
            _lock.EnterReadLock();
            try
            {
                if (_storage.TryGetValue(dist, out var innerDict) &&
                    innerDict.TryGetValue(actions, out state))
                {
                    return true;
                }
                state = default;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<(int dist, ActionsStruct actions, GameState state)> GetOrderedItems()
        {
            List<KeyValuePair<int, Dictionary<ActionsStruct, GameState>>> snapshot;

            _lock.EnterReadLock();
            try
            {
                snapshot = new List<KeyValuePair<int, Dictionary<ActionsStruct, GameState>>>(_storage);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (var outerKvp in snapshot)
            {
                foreach (var innerKvp in outerKvp.Value)
                {
                    yield return (outerKvp.Key, innerKvp.Key, innerKvp.Value);
                }
            }
        }
    }
}
