namespace vadymvlm.Legion.Collection
{
  using System.Diagnostics;
  using System;

  /// <summary>
  /// Low-allocating multicast delegate.
  /// </summary>
  [DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class Multicast : BaseHashset<Action>, ICollection<Action>
  {
    public Multicast(int capacity = CAPACITY_DEFAULT)
      : base(capacity) { }

    /// <inheritdoc cref="BaseHashset{T}.Add(T, int)"/>
    public void Subscribe(Action action) => Add(action, action.ComputeHash());

    /// <inheritdoc cref="BaseHashset{T}.Remove(T, int)"/>
    public void Unsubscribe(Action action) => Remove(action, action.ComputeHash());

    public void Invoke()
    {
      using var enumerator = GetEnumerator();
      while (enumerator.MoveNext(out var invoke))
        invoke();
    }
  }
}