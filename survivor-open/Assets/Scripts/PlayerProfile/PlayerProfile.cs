using UnityEngine;

[System.Serializable]
public class UnlockedCharacter: IControllable
{
    [SerializeField] private PlayerCharacterController characterController;
    [SerializeField] private bool isUnlocked;
    private int unlockCost;

    public PlayerCharacterController GameCharacter
    {
        get { return characterController; }    
    }

    public bool IsUnlocked
    {
        get { return isUnlocked; }
        set { isUnlocked = value; }
    }

    public void Initialize(IControllable[] _injectedElements)
    {
        
    }
}

public class PlayerProfile : MonoBehaviour, IControllable
{

    [SerializeField] private UnlockedCharacter[] unlockedCharacters;
    private int currentGold;

    PlayerCharacterController currentCharacterController;

    SaveLoadManager saveLoadManager;

    public UnlockedCharacter[] UnlockedCharacters
    {
        get { return unlockedCharacters; }
    }

    public PlayerCharacterController CurrentCharacterController
    {
        get { return currentCharacterController; }
        set { currentCharacterController = value; }
    }


    public void Initialize(IControllable[] _injectedElements)
    {
        saveLoadManager = _injectedElements[0] as SaveLoadManager;
        currentCharacterController = unlockedCharacters[0].GameCharacter;
    }
}
