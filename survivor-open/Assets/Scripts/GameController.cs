using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Drag Elements - Cant be Blank")]
    [SerializeField] private SaveLoadManager saveLoadManager;

    [SerializeField] private PlayerProfile playerProfile;
    [SerializeField] private SpatialGroupManager spatialGroupManager;
    [SerializeField] private UserInterfaceManager userInterfaceManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private CameraController cameraController;

    private PlayerCharacterController playerScript;

    private void OnEnable()
    {
        GameEvents.OnDoStartGame += Handle_StartGame;
    }

    private void OnDisable()
    {
        GameEvents.OnDoStartGame -= Handle_StartGame;
    }

    private void Start()
    {
        if (!saveLoadManager)
        {
            Debug.LogError("No saveLoadManager object");
            return;
        }

        if (!playerProfile)
        {
            Debug.LogError("No player profile object");
            return;
        }

        if (!spatialGroupManager)
        {
            Debug.LogError("No spatial group manager object");
            return;
        }

        if (!poolManager)
        {
            Debug.LogError("No pool manager object");
            return;
        }

        if (!cameraController)
        {
            Debug.LogError("No camera controller object");
            return;
        }

        //Injecting dependencies to critical components

        if (saveLoadManager.IsProfileLoaded) /// currenly is always true - will change after save / load is implemented
        {
            playerProfile.Initialize(new IControllable[] { saveLoadManager });
            userInterfaceManager.Initialize(new IControllable[] { playerProfile });
        }
        else
        {
            Debug.LogError("Unable to Load Profile - Panic!");
            // later - start waiting coroutine
        }
    }

    private void Handle_StartGame()
    {
        playerScript = GameObject.Instantiate(playerProfile.CurrentCharacterController) as PlayerCharacterController;
        playerScript.Initialize(new IControllable[] { spatialGroupManager, userInterfaceManager });
        spatialGroupManager.Initialize(new IControllable[] { playerScript, poolManager });
        poolManager.Initialize(new IControllable[] { spatialGroupManager });
        cameraController.Initialize(new IControllable[] { playerScript });
    }
}