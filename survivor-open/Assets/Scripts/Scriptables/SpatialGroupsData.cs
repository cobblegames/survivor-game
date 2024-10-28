using UnityEngine;

[CreateAssetMenu(menuName = "SpatialGroups/Setup Data", fileName = "SpatialGroupsSetupData")]
public class SpatialGroupsData : ScriptableObject
{
    [SerializeField] private int spatialGroupWidth = 100;
    [SerializeField] private int spatialGroupHeight = 100;
    [SerializeField] private int numberOfPartitions = 10000;

    [SerializeField] private int initEnemyCount = 10000;
    [SerializeField] private int maxEnemyCount = 10000;

    public int SpatialGroupWidth
    { get { return spatialGroupWidth; } private set { spatialGroupWidth = value; } }

    public int SpatialGroupHeight
    { get { return spatialGroupHeight; } private set { spatialGroupHeight = value; } }

    public int NumberOfPartitions 
    { get { return numberOfPartitions; } private set { numberOfPartitions = value; } }

    public int InitEnemyCount
    { get { return initEnemyCount; } private set { initEnemyCount = value; } }

    public int MaxEnemyCount
    { get { return maxEnemyCount; } private set { maxEnemyCount = value; } }
}