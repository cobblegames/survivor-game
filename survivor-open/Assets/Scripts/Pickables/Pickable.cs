using UnityEngine;

public class Pickable : MonoBehaviour, IControllable
{
    [SerializeField] protected PickableData _data;

    public virtual void Initialize(IControllable[] _injectedElements)
    {
       
    }
}
