namespace vadymvlm.Legion.Collection
{
  using System.Diagnostics;
  using System;

  using UObject = UnityEngine.Object;
  using UDebug = UnityEngine.Debug;

  /// <summary>
  /// Fixed pool, based on stack.
  /// </summary>
  [DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class FixedPool<T> : IPool<T>, ICollection<T>
  where T : UObject, IActivatable
  {
    public const int CAPACITY_DEFAULT = 4;
    public const int CAPACITY_MIN = 4;
    
    public int Capacity => _entities.Length;
    public int Count => _count;

    private readonly Func<T> _onCreate;
    private readonly Action<T> _onDestroy;
    private T[] _entities;
    private int _count;

    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentOutOfRangeException" />
    public FixedPool(Func<T> onCreate, Action<T> onDestroy, int capacity = CAPACITY_DEFAULT)
    {
      _onCreate = onCreate ?? throw new ArgumentNullException(nameof(onCreate));
      _onDestroy = onDestroy ?? throw new ArgumentNullException(nameof(onDestroy));
      _entities = capacity >= CAPACITY_MIN ? new T[capacity] : throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    /// <summary>
    /// This is an O(1) operation.
    /// </summary>
    public T Get()
    {
      T item;

      if (_count > 0)
      {
        ref var entityRef = ref _entities[--_count];
        item = entityRef;
        item.Activate();
        entityRef = null;
      }
      else
      {
        item = _onCreate();
        UDebug.Assert(item.IsActivated, $"Created not active and disabled {nameof(item)}.");
      }

      return item;
    }

    /// <summary>
    /// This is an O(1) operation.
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    public void Release(T item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item));

#if UNITY_ASSERTIONS
      for (var i = 0; i < _count; i++)
        UDebug.Assert(!_entities[i] == item, $"The input {nameof(item)} has already release.");
#endif

      if (_count < _entities.Length)
      {
        _entities[_count++] = item;
        item.Inactivate();
      }
      else
      {
        _onDestroy(item);
      }
    }

    /// <summary>
    /// Removes all elements from the <see cref="FixedPool{T}" />.
    /// </summary>
    public void Clear()
    {
      while (_count > 0)
      {
        ref var entityRef = ref _entities[--_count];
        _onDestroy(entityRef);
        entityRef = null;
      }
    }

    /// <summary>
    /// Copies the elements of the <see cref="FixedPool{T}"/> to a new array.
    /// </summary>
    public T[] ToArray()
    {
      var array = new T[_count];
      Array.Copy(_entities, array, _count);

      return array;
    }
  }
}