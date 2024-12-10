using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceManager : MonoBehaviour, IControllable
{
    [Header("Main Menu UI")]
    [SerializeField] private Button startButtonCharacterChoice;

    [SerializeField] private Button startButtonGameplay;
    [SerializeField] private RectTransform mainMenuRoot;
    [SerializeField] private RectTransform windowsRoot;
    [SerializeField] private CharacterChoiceMain characterChoiceMain;

    [Header("Gameplay UI")]
    [SerializeField] private RectTransform gameplayRoot;

    [SerializeField] private ProgressBar healthBar;
    [SerializeField] private ProgressBar experienceBar;
    [SerializeField] private TextMeshProUGUI levelLabel;

    #region Injectables

    private PlayerProfile playerProfile;

    #endregion Injectables

    public float HPValue
    {
        set { healthBar.UpdateBar(value); }
    }

    public float ExpValue
    {
        set { experienceBar.UpdateBar(value); }
    }

    public string LevelValue
    {
        set { levelLabel.text = value; }
    }

    private void OnEnable()
    {
        if (startButtonCharacterChoice != null)
        {
            startButtonCharacterChoice.onClick.AddListener(Handle_StartGameShowCharacterChoice);
        }

        if (startButtonGameplay != null)
        {
            startButtonGameplay.onClick.AddListener(Handle_StartGameplay);
        }
    }

    private void OnDisable()
    {
        if (startButtonCharacterChoice != null)
        {
            startButtonCharacterChoice.onClick.RemoveListener(Handle_StartGameShowCharacterChoice);
        }

        if (startButtonGameplay != null)
        {
            startButtonGameplay.onClick.RemoveListener(Handle_StartGameplay);
        }
    }

    private void Handle_StartGameShowCharacterChoice()
    {
        startButtonCharacterChoice.gameObject.SetActive(false);
        CharacterChoiceMain characterChoice = GameObject.Instantiate(characterChoiceMain, windowsRoot) as CharacterChoiceMain;
        characterChoice.Initialize(new IControllable[] { playerProfile, this });
        startButtonGameplay.gameObject.SetActive(true);
    }

    private void Handle_StartGameplay()
    {
        GameEvents.DoStartGame();
        if (mainMenuRoot != null)
        {
            mainMenuRoot.gameObject.SetActive(false);
        }

        if (gameplayRoot != null)
        {
            gameplayRoot.gameObject.SetActive(true);
        }
    }

    public void Initialize(IControllable[] _injectedElements)
    {
        playerProfile = _injectedElements[0] as PlayerProfile;
    }
}