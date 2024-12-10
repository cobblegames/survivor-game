using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterChoiceBox : PopUpWindow
{
    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private GameObject highlightElement;

    #region Injectables

    private UnlockedCharacter unlockedCharacter;
    private CharacterChoiceMain mainChoiceMain;

    #endregion Injectables

    public GameObject HighlightElement
    {
        get { return highlightElement; }
    }

    public UnlockedCharacter UnlockedCharacter
    {
        get { return unlockedCharacter; }
    }

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);

        unlockedCharacter = _injectedElements[0] as UnlockedCharacter;
        mainChoiceMain = _injectedElements[1] as CharacterChoiceMain;
        characterIcon.sprite = unlockedCharacter.GameCharacter.PlayerData.PlayerIcon;
        characterName.text = unlockedCharacter.GameCharacter.PlayerData.CharacterName;

        highlightElement.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        highlightElement.SetActive(true);
        mainChoiceMain.MakeChoice(unlockedCharacter);
    }
}