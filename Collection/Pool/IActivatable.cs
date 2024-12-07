namespace vadymvlm.Legion
{
  public interface IActivatable
  {
    bool IsActivated { get; }

    void Activate();
    void Inactivate();
  }
}