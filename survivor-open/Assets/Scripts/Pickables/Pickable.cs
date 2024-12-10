using UnityEngine;

public class Pickable : MonoBehaviour, IControllable, IMovable
{
    [SerializeField] protected PickableData _data;
    protected SpatialGroupManager spatialGroupManager;
    protected PlayerCharacterController playerController;

    [SerializeField] protected bool magnetIsOn = false;
    [SerializeField] protected float currentSpeed = 0.2f;

    public virtual void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        playerController = _injectedElements[1] as PlayerCharacterController;
    }

    public void IntervalLogic()
    {
    }

    public void EveryFrameLogic()
    {
        if (magnetIsOn)
        {
            Vector3 currentMovementDirection = playerController.transform.position - transform.position;
            currentMovementDirection.Normalize();
            Debug.Log("Moving pickable");
            transform.position += currentMovementDirection * Time.deltaTime * currentSpeed;
        }
    }

    public virtual void Handle_GivePickupBonus()
    {
    }
}