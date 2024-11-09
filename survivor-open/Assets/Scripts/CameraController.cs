using UnityEngine;

public class CameraController : MonoBehaviour, IControllable
{
    PlayerController player;
 
    public void Initialize(IControllable[] _argTable)
    {
        this.player = _argTable[0] as PlayerController;
    }


    private void LateUpdate()
    {
        if (player != null)
        {
            
            Vector3 targetPosition = player.transform.position;
            transform.position = targetPosition;
        }
    }



}
