using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UserInterfaceManager : MonoBehaviour, IControllable
{
    [Header("Main Menu UI")]

    [SerializeField] private Button startButton;
    [SerializeField] private Transform mainMenuRoot;
    
    
    
    [Header ("Gameplay UI")]

    [SerializeField] private Transform gameplayRoot;
    [SerializeField] private ProgressBar healthBar;
    [SerializeField] private ProgressBar experienceBar;
    [SerializeField] private TextMeshProUGUI levelLabel;



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
        set { levelLabel.text =  value; }
    }

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(Handle_StartButton);
        }
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(Handle_StartButton);
        }
    }

    private void Handle_StartButton()
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
       
    }
}