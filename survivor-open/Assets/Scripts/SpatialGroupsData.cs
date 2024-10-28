using UnityEngine;

[CreateAssetMenu(menuName = "SpatialGroups/Setup Data", fileName = "SpatialGroupsSetupData")]
public class SpatialGroupsData : ScriptableObject
{
    [SerializeField] private int spatialGroupWidth = 100;
    [SerializeField] private int spatialGroupHeight = 100;
    [SerializeField] private int numberOfPartitions = 10000;

    public int SpatialGroupWidth
    { get { return spatialGroupWidth; } private set { spatialGroupWidth = value; } }

    public int SpatialGroupHeight
    { get { return spatialGroupHeight; } private set { spatialGroupHeight = value; } }

    public int NumberOfPartitions { get { return numberOfPartitions; } private set { numberOfPartitions = value; } }
}