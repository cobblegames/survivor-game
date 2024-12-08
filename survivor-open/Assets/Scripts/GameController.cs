using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField] private PlayerProfile playerProfile;
    [SerializeField] private PlayerCharacterController playerScript;
    [SerializeField] private SpatialGroupManager spatialGroupManager;
    [SerializeField] private UserInterfaceManager userInterfaceManager;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private CameraController cameraController;


    private void Start()
    {
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

        playerScript = playerProfile.CurrentCharacterController;

        playerScript.Initialize(new IControllable[] { spatialGroupManager, userInterfaceManager });
        spatialGroupManager.Initialize(new IControllable[] { playerScript, poolManager });
        poolManager.Initialize(new IControllable[] { spatialGroupManager });
        cameraController.Initialize(new IControllable[] { playerScript });
    }
}