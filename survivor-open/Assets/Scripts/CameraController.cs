using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour, IControllable
{
    private PlayerController player;
    private WaitForEndOfFrame endOfFrame;

    public void Initialize(IControllable[] _injectedElements)
    {
        endOfFrame = new WaitForEndOfFrame();
        this.player = _injectedElements[0] as PlayerController;

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