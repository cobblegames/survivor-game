using UnityEngine;

[System.Serializable]
public class UnlockedCharacters
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

}

public class PlayerProfile : MonoBehaviour, IControllable
{
    [SerializeField] private UnlockedCharacters[] unlockedCharacters;
    private int currentGold;

    PlayerCharacterController currentCharacterController;

    public PlayerCharacterController CurrentCharacterController
    {
        get { return currentCharacterController; }
        set { currentCharacterController = value; }
    }


    public void Initialize(IControllable[] _injectedElements)
    {
        currentCharacterController = unlockedCharacters[0].GameCharacter;
    }
}
