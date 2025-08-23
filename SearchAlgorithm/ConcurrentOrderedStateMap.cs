namespace SearchAlgorithm;

public sealed class ConcurrentOrderedStateMap : IStorageProvider
{
    private readonly Partition[] _partitions;
    private readonly int _partitionCount;
    private readonly int _rangeSize;
    private long _globalCount;

    public long Count => Interlocked.Read(ref _globalCount);

    public ConcurrentOrderedStateMap(int minKey, int maxKey, int? concurrencyLevel = null)
    {
        _partitionCount = concurrencyLevel ?? Environment.ProcessorCount;
        _partitions = new Partition[_partitionCount];
        _rangeSize = Math.Min(10000, Math.Max(1000, (maxKey - minKey) / _partitionCount));
        _globalCount = 0;

        for (int i = 0; i < _partitionCount; i++)
        {
            int partitionMin = minKey + i * _rangeSize;
            int partitionMax = Math.Min(minKey + (i + 1) * _rangeSize - 1, maxKey);
            _partitions[i] = new Partition(this, partitionMin, partitionMax);
        }
    }

    public void Add(int dist, ActionsStruct actions, GameState state)
    {
        var partition = GetPartition(dist);
        partition.Add(dist, actions, state);
    }

    public bool Remove(int dist, ActionsStruct actions)
    {
        var partition = GetPartition(dist);
        return partition.Remove(dist, actions);
    }

    public void CullTo(long maxItems)
    {
        if (Count <= maxItems) return;

        long toRemove = Count - maxItems;
        long removedTotal = 0;

        var partitionsWithItems = _partitions
            .Where(p => p.ItemCount > 0)
            .OrderByDescending(p => p.MaxKey)
            .ToArray();

        foreach (var partition in partitionsWithItems)
        {
            if (removedTotal >= toRemove) break;

            int remainingToRemove = (int)Math.Min(toRemove - removedTotal, int.MaxValue);
            int removed = partition.RemoveFarthest(remainingToRemove);
            removedTotal += removed;
        }
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

    public void ValidateCount()
    {
        long actualCount = 0;
        foreach (var p in _partitions)
        {
            actualCount += p.ItemCount;
        }

        if (actualCount != Count)
        {
            Console.WriteLine($"Count mismatch! Global={Count} Actual={actualCount}");
        }
    }

    private Partition GetPartition(int dist)
    {
        if (dist < _partitions[0].MinKey)
        {
            return _partitions[0];
        }
        
        if (dist > _partitions[_partitionCount - 1].MaxKey)
        {
            return _partitions[_partitionCount - 1];
        }

        int partitionIndex = (dist - _partitions[0].MinKey) / _rangeSize;

        return _partitions[Math.Clamp(partitionIndex, 0, _partitionCount - 1)];
    }

    private sealed class Partition
    {
        private readonly ConcurrentOrderedStateMap _parent;
        private readonly SortedDictionary<int, Dictionary<ActionsStruct, GameState>> _storage;
        private readonly ReaderWriterLockSlim _lock = new();
        public readonly int MinKey;
        public readonly int MaxKey;

        public int ItemCount
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _storage.Sum(kvp => kvp.Value.Count);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public Partition(ConcurrentOrderedStateMap parent, int minKey, int maxKey)
        {
            _parent = parent;
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

                bool isNew = !innerDict.ContainsKey(actions);
                innerDict[actions] = state;

                if (isNew)
                {
                    Interlocked.Increment(ref _parent._globalCount);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(int dist, ActionsStruct actions)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_storage.TryGetValue(dist, out var innerDict) &&
                   innerDict.Remove(actions))
                {
                    Interlocked.Decrement(ref _parent._globalCount);

                    if (innerDict.Count == 0)
                    {
                        _storage.Remove(dist);
                    }
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int RemoveFarthest(int maxToRemove)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_storage.Count == 0) return 0;

                var removalCandidates = new List<(int dist, ActionsStruct actions)>();
                int collected = 0;

                foreach (var outer in _storage.Reverse())
                {
                    foreach (var actionKey in outer.Value.Keys)
                    {
                        removalCandidates.Add((outer.Key, actionKey));
                        if (++collected >= maxToRemove) break;
                    }
                    if (collected >= maxToRemove) break;
                }

                int actuallyRemoved = 0;
                foreach (var candidate in removalCandidates)
                {
                    if (_storage.TryGetValue(candidate.dist, out var innerDict) &&
                        innerDict.Remove(candidate.actions))
                    {
                        actuallyRemoved++;
                        Interlocked.Decrement(ref _parent._globalCount);

                        if (innerDict.Count == 0)
                        {
                            _storage.Remove(candidate.dist);
                        }
                    }
                }

                return actuallyRemoved;
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
