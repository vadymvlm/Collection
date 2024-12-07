namespace vadymvlm.Legion.Collection
{
  using System.Runtime.CompilerServices;
  using System;

  public abstract class BaseHashset<T>
  {
    protected ref struct Enumerator
    {
      private readonly BaseHashset<T> _hashset;
      private readonly Entry[] _entries;
      private int _index;
      private int _count;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      internal Enumerator(BaseHashset<T> hashset)
      {
        _entries = hashset._entries;
        _count = hashset._count;
        _hashset = hashset;
        _index = 0;
        _hashset._isEnumerating = true;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Dispose()
      {
        _hashset._isEnumerating = false;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool MoveNext(out T current)
      {
        var inRange = _index < _count;
        current = inRange ? _entries[_index++].Value : default;
        return inRange;
      }
    }

    private struct Entry
    {
      public T Value;
      public int Hash;
      public int PrevIndex;

      public void Set(T value, int hash = 0, int prevIndex = -1)
      {
        Value = value;
        Hash = hash;
        PrevIndex = prevIndex;
      }
    }

    public const int CAPACITY_DEFAULT = 7;
    private const int PRIME_MUL = 2;
    private const int PRIME_MIN = 2;

    #region Exception messages
    private const string ALREADY_ERR_MESSAGE = "The handler is already subscribed.";
    private const string MODIFIED_ERR_MESSAGE = "The collection was modified after the enumerator was created.";
    private const string EXCEEDED_ERR_MESSAGE = "The number of collisions has been exceeded.";
    #endregion

    public int Capacity => _capacity;
    public int Count => _count;

    private int[] _buckets;
    private Entry[] _entries;
    private int _capacity;
    private int _count;
    private bool _isEnumerating;

    public BaseHashset(int capacity = CAPACITY_DEFAULT)
    {
      _capacity = GetCapacity(capacity);
      _buckets = new int[_capacity];
      _entries = new Entry[_capacity];
    }

    /// <summary>
    /// This is an O(1) operation (or O(n) when resizing).
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentException" />
    /// <exception cref="InvalidOperationException" />
    protected void Add(T value, int hash)
    {
      if (_isEnumerating)
        throw new InvalidOperationException(MODIFIED_ERR_MESSAGE);
      if (value == null)
        throw new ArgumentNullException(nameof(value));

      if (_count >= _capacity)
        EnsureCapacity();

      ref var bucketRef = ref GetBucketRef(hash);
      var collisionCount = 0;

      var bucketValue = bucketRef - 1;
      var currIndex = bucketValue;
      while (currIndex >= 0)
      {
        ref var entryRef = ref _entries[currIndex];
        if (entryRef.Value.Equals(value))
          throw new ArgumentException(ALREADY_ERR_MESSAGE);

        currIndex = entryRef.PrevIndex;

        ++collisionCount;
        if (collisionCount > _capacity)
          throw new InvalidOperationException(EXCEEDED_ERR_MESSAGE);
      }

      _entries[_count].Set(value, hash, prevIndex: bucketValue);
      bucketRef = ++_count;
    }
    /// <summary>
    /// This is an O(1) operation.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    protected void Remove(T value, int hash)
    {
      if (_isEnumerating)
        throw new InvalidOperationException(MODIFIED_ERR_MESSAGE);
      if (value == null)
        throw new ArgumentNullException(nameof(value));

      ref var bucketRef = ref GetBucketRef(hash);
      var collisionCount = 0;

      var prevIndex = -1;
      var currIndex = bucketRef - 1;
      while (currIndex >= 0)
      {
        ref var entryRef = ref _entries[currIndex];
        if (entryRef.Value.Equals(value))
        {
          if (prevIndex < 0)
            bucketRef = entryRef.PrevIndex + 1;
          else
            _entries[prevIndex].PrevIndex = entryRef.PrevIndex;

          --_count;

          if (currIndex == _count)
          {
            entryRef.Set(default);
          }
          else
          {
            ref var lastEntryRef = ref _entries[_count];
            GetBucketRef(lastEntryRef.Hash) = currIndex;
            entryRef = lastEntryRef;
            lastEntryRef.Set(default);
          }

          return;
        }

        prevIndex = currIndex;
        currIndex = entryRef.PrevIndex;

        ++collisionCount;
        if (collisionCount > _capacity)
          throw new InvalidOperationException(EXCEEDED_ERR_MESSAGE);
      }
    }

    /// <summary>
    /// Copies the values of the <see cref="BaseHashset{T}"/> to a new array.
    /// </summary>
    public T[] ToArray()
    {
      var array = new T[_count];
      for (var i = 0; i < _count; i++)
        array[i] = _entries[i].Value;

      return array;
    }
    protected int GetCapacity(int value)
    {
      if (value < PRIME_MIN)
        return PRIME_MIN;

      while (!IsPrime(value))
        ++value;

      return value;

      bool IsPrime(int value)
      {
        for (var i = PRIME_MIN; i * i <= value; i++)
          if (value % i == 0)
            return false;

        return true;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Enumerator GetEnumerator() => new Enumerator(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucketRef(int hash) => ref _buckets[(uint)hash % (uint)_capacity];

    private void EnsureCapacity()
    {
      var source = _entries;

      _capacity = GetCapacity(_capacity * PRIME_MUL);
      _buckets = new int[_capacity];
      _entries = new Entry[_capacity];

      for (var i = 0; i < _count; i++)
      {
        ref var entryRef = ref _entries[i];
        entryRef = source[i];

        ref var bucketRef = ref GetBucketRef(entryRef.Hash);
        entryRef.PrevIndex = bucketRef - 1;
        bucketRef = i + 1;
      }
    }
  }
}