using UnityEngine;

[CreateAssetMenu(menuName = "Pickable/Pickable Data", fileName = "PickableData")]
public class PickableData : ScriptableObject
{
    [SerializeField] private float hitBoxRadius = 0.4f;
    [SerializeField] private float collectionRadius = 1.0f;

    public float HitBoxRadius
    { get { return hitBoxRadius; } }

    public float CollectionRadius
    { get { return collectionRadius; } }
}