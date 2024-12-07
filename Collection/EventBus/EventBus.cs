namespace vadymvlm.Legion.Collection
{
  using System.Runtime.CompilerServices;
  using System.Diagnostics;

  public delegate void EventBusEventHandler<T>(ref T e)
  where T : struct;

  /// <summary>
  /// Low-allocating event bus.
  /// </summary>
  [DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class EventBus<T> : BaseHashset<EventBusEventHandler<T>>, ICollection<EventBusEventHandler<T>>
  where T : struct, IEventBusEvent
  {
    public static EventBus<T> Instance
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _instance != null ? _instance : _instance = new();
    }

    private static EventBus<T> _instance;

    public EventBus(int capacity = CAPACITY_DEFAULT)
      : base(capacity) { }

    /// <inheritdoc cref="BaseHashset{T}.Add(T, int)"/>
    public void Subscribe(EventBusEventHandler<T> handler) => Add(handler, handler.ComputeHash());

    /// <inheritdoc cref="BaseHashset{T}.Remove(T, int)"/>
    public void Unsubscribe(EventBusEventHandler<T> handler) => Remove(handler, handler.ComputeHash());

    public void RaiseEvent(T e)
    {
      using var enumerator = GetEnumerator();
      while (enumerator.MoveNext(out var invoke))
        invoke(ref e);
    }
  }
}