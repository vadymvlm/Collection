namespace vadymvlm.Legion.Collection
{
  public interface ICollection<T>
  {
    int Count { get; }
    int Capacity { get; }

    T[] ToArray();
  }
}