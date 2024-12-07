namespace vadymvlm.Legion.Collection
{
  public interface IPool<T>
  {
    T Get();
    void Release(T item);
    void Clear();
  }
}