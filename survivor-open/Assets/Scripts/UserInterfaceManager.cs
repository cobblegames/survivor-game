using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Transform mainMenuRoot;
    [SerializeField] private Transform gameplayRoot;

    private void OnEnable()
    {
        if(startButton != null)
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
        GameEvents.StartGame();
        if(mainMenuRoot!=null)
        {
            mainMenuRoot.gameObject.SetActive(false);
        }

        if(gameplayRoot!=null)
        {
            gameplayRoot.gameObject.SetActive(true);    
        }
    }

}
