namespace vadymvlm.Legion.Collection
{
  using UnityEngine;

  [DisallowMultipleComponent]
  [AddComponentMenu("vadymvlm.Legion/EventBus/LGN Event Timer")]
  public sealed class EventTimer : MonoBehaviour
  {
    public static readonly Multicast OnFixedUpdate = new();
    public static readonly Multicast OnUpdate = new();
    public static readonly Multicast OnLateUpdate = new();

    /// <summary>
    /// Example of use: AI players, weapon.
    /// </summary>
    public static readonly Multicast OnBeforeLateUpdate = new();

    /// <summary>
    /// Example of use: camera SVfx, linked audio listener.
    /// </summary>
    public static readonly Multicast OnAfterLateUpdate = new();

#if UNITY_ASSERTIONS
    private void Awake() => AssertExistsSingleInstanceOnly(this);

    private static void AssertExistsSingleInstanceOnly(Component component)
    {
      var type = component.GetType();
      var count = FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
      Debug.Assert(count == 1, $"Too many {type.Name} are placed on the scene.");
    }
#endif

    private void FixedUpdate()
    {
      OnFixedUpdate.Invoke();
    }

    private void Update()
    {
      OnUpdate.Invoke();
    }

    private void LateUpdate()
    {
      OnBeforeLateUpdate.Invoke();
      OnLateUpdate.Invoke();
      OnAfterLateUpdate.Invoke();
    }
  }
}