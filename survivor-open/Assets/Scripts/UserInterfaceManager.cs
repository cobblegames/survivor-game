using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] private Button startBuutton;


    private void OnEnable()
    {
        startBuutton.onClick.AddListener(Handle_StartButton);
    }

    private void OnDisable()
    {
        startBuutton.onClick.RemoveListener(Handle_StartButton);
    }


    private void Handle_StartButton()
    {
        GameEvents.StartGame();
    }

}
