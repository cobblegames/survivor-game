using UnityEngine;

public class ExperienceGem : Pickable
{
   [SerializeField] private int currentPoints;

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);
    }


    public override void Handle_GivePickupBonus()
    {
        playerController.GetExp(currentPoints);
    }

}
