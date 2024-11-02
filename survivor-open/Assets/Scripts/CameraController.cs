using UnityEngine;

public class CameraController : MonoBehaviour
{
    PlayerController player;
 
    public void Initialize(PlayerController _player)
    {
        this.player = _player;
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
