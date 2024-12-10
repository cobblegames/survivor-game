using UnityEngine;

public class SaveLoadManager : MonoBehaviour, IControllable
{
    // WIP - should run in the beginning, check if save file exists in coroutine, populate values to player profile

    [SerializeField] private bool isProfileLoaded;

    public bool IsProfileLoaded
    {
        get { return isProfileLoaded; }
    }

    public void Initialize(IControllable[] _injectedElements)
    {
    }
}