using UnityEngine;

public class Pickable : MonoBehaviour, IControllable, IMovable
{
    [SerializeField] protected PickableData _data;
    protected SpatialGroupManager spatialGroupManager;
    protected PlayerController playerController;

    public virtual void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        playerController = _injectedElements[1] as PlayerController;
    }

    public void IntervalLogic()
    {
       
    }

    public void EveryFrameLogic()
    {
        
    }

}
