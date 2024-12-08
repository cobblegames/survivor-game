
using UnityEngine;

public class CharacterChoiceMain : PopUpWindow
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private CharacterChoiceBox characterChoiceBox;

    private CharacterChoiceBox[] characterChoiceBoxes;

    #region Injectables
    private PlayerProfile playerProfile;
    private UserInterfaceManager userInterfaceManager;
    #endregion 

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);

        playerProfile = _injectedElements[0] as PlayerProfile;
        userInterfaceManager = _injectedElements[1] as UserInterfaceManager;

        characterChoiceBoxes = new CharacterChoiceBox[playerProfile.UnlockedCharacters.Length]; 

        for (int i = 0; i < playerProfile.UnlockedCharacters.Length; i++)
        {
            CharacterChoiceBox charBox = GameObject.Instantiate(characterChoiceBox, contentRoot) as CharacterChoiceBox;
           
            charBox.Initialize(new IControllable[] { playerProfile.UnlockedCharacters[i], this });

            characterChoiceBoxes[i] = charBox;
        }

    }

    public void MakeChoice(UnlockedCharacter chosenCharacter)
    {
       for (int i = 0; i < characterChoiceBoxes.Length;i++)
        {
            if(characterChoiceBoxes[i].UnlockedCharacter == chosenCharacter)
                continue;

            characterChoiceBoxes[i].HighlightElement.SetActive(false);
        }

        playerProfile.CurrentCharacterController = chosenCharacter.GameCharacter;
    }


}
