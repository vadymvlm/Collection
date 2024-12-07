namespace vadymvlm.Legion.Collection
{
  public interface IEventBusEvent
  {
    T GetContext<T>();
  }
}