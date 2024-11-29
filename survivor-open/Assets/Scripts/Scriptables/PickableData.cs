using UnityEngine;

[CreateAssetMenu(menuName = "Pickable/Pickalble Data", fileName = "PickalbleData")]
public class PickableData : ScriptableObject
{
    [SerializeField] private float hitBoxRadius = 0.4f;

    public float HitBoxRadius
    { get { return hitBoxRadius; } }
}
