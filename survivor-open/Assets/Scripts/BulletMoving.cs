using UnityEngine;

public class BulletMoving : BaseBullet, IMovable
{
    protected float currentBulletSpeed;

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);

    }
    public void EveryFrameLogic()
    {

    }

    public void IntervalLogic()
    {
       
    }
}
