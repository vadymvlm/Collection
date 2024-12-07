namespace vadymvlm.Legion.Collection
{
  using System.Runtime.CompilerServices;
  using System;

  public static class DelegateExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeHash<T>(this T invocation)
    where T : Delegate
    {
      var target = invocation.Target;
      return target != null ? target.GetHashCode() : invocation.Method.GetHashCode();
    }
  }
}