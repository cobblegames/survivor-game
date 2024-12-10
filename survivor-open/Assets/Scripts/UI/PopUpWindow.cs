using UnityEngine;
using UnityEngine.EventSystems;

public class PopUpWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IControllable
{
    public virtual void Initialize(IControllable[] _injectedElements)
    {
    }

    // WIP - class which should be a base for all game pop up windows
    public virtual void OnPointerDown(PointerEventData eventData)
    {
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
    }
}