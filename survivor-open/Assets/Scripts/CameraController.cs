using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerController player;
    private WaitForEndOfFrame endOfFrame;

    public void Initialize(PlayerController _player)
    {
        endOfFrame = new WaitForEndOfFrame();
        this.player = _player;

        StartCoroutine(MoveCamera());
    }

    private IEnumerator MoveCamera()
    {
        while (player != null)
        {
            Vector3 targetPosition = player.transform.position;

            transform.position = new Vector3(targetPosition.x, targetPosition.y, -10);

            yield return endOfFrame;
        }
    }
}