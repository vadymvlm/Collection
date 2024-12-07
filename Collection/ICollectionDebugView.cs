namespace vadymvlm.Legion.Collection
{
  using System.Diagnostics;
  using System;

  public sealed class ICollectionDebugView<T>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public T[] Items => _collection.ToArray();

    private readonly ICollection<T> _collection;

    /// <exception cref="ArgumentNullException" />
    public ICollectionDebugView(ICollection<T> collection) => _collection = collection ?? throw new ArgumentNullException(nameof(collection));
  }
}