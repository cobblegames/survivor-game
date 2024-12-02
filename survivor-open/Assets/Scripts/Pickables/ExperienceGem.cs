using UnityEngine;

public class ExperienceGem : Pickable
{
    private float currentPoints;
    private float currentCollection;

    public float CurrentPoints
    { get { return currentPoints; } }


    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);
    }


}
