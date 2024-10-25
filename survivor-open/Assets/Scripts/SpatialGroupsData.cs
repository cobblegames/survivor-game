using UnityEngine;

[CreateAssetMenu(menuName = "SpatialGroups/Setup Data", fileName = "SpatialGroupsSetupData")]
public class SpatialGroupsData : ScriptableObject
{
    //* SPATIAL PARTITIONING *//
    [SerializeField] private int spatialGroupWidth = 100;

    public int SpatialGroupWidth
    { get { return spatialGroupWidth; } private set { spatialGroupWidth = value; } }

    [SerializeField] private int spatialGroupHeight = 100;
    public int SpatialGroupHeight
    { get { return spatialGroupHeight; } private set { spatialGroupHeight = value; } }

    [SerializeField] private int numberOfPartitions = 10000;
    [SerializeField] public int NumberOfPartitions { get { return numberOfPartitions; } private set { numberOfPartitions = value; } }
}