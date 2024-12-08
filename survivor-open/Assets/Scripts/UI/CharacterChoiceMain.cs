
using UnityEngine;

public class CharacterChoiceMain : PopUpWindow
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private CharacterChoiceBox characterChoiceBox;

    #region Injectables
    private PlayerProfile playerProfile;
    private UserInterfaceManager userInterfaceManager;
    #endregion 

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);

        playerProfile = _injectedElements[0] as PlayerProfile;
        userInterfaceManager = _injectedElements[1] as UserInterfaceManager;

        for (int i = 0; i < playerProfile.UnlockedCharacters.Length; i++)
        {
            CharacterChoiceBox charBox = GameObject.Instantiate(characterChoiceBox, contentRoot) as CharacterChoiceBox;
           
            charBox.Initialize(new IControllable[] { playerProfile.UnlockedCharacters[i] });
        }

    }


}
