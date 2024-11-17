using System.Collections.Generic;

using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField] private PlayerController playerScript;
    [SerializeField] private SpatialGroupManager spatialGroupManager;
    [SerializeField] private CameraController cameraController;
    public PlayerController PlayerScript
    { get { return playerScript; } }

    public SpatialGroupManager SpatialGroupManager
    { get { return spatialGroupManager; } }
   
    private void Start()
    {
        if (!playerScript)
        {
            Debug.LogError("No player object");
            return;
        }

        if (!spatialGroupManager)
        {
            Debug.LogError("No spatial group manager object");
            return;
        }

        //Injecting dependencies to critical components
       
        playerScript.Initialize(new IControllable[] { spatialGroupManager });
        spatialGroupManager.Initialize(new IControllable[] { playerScript });
  
        cameraController.Initialize(new IControllable[] { playerScript });



    }
}