
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
   

    public static List<Enemy> GetAllEnemiesInSpatialGroups(List<int> spatialGroups)
    {
        List<Enemy> enemies = new List<Enemy>();

        foreach (int spatialGroup in spatialGroups)
        {
            enemies.AddRange(GameController.instance.enemySpatialGroups[spatialGroup]);
        }

        return enemies;
    }

    public static Vector3 V2toV3(Vector2 v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    
    public static List<int> GetExpandedSpatialGroups(int spatialGroup, int numberOfPartitions = -1)
    {
        List<int> expandedSpatialGroups = new List<int>() { spatialGroup };

        int widthRange = GameController.instance.SpatialGroupWidth;  // ex. 100
        int heightRange = GameController.instance.SpatialGroupHeight; // ex. 100
        if (numberOfPartitions == -1)
            numberOfPartitions = GameController.instance.NumberOfPartitions; // ex. 10000 -or- 25

        int sqrtOfPartitions = (int)Mathf.Sqrt(numberOfPartitions); // Square root of partitions
        int partitionsPerRow = sqrtOfPartitions;  // Number of columns in the grid
        int numberOfRows = sqrtOfPartitions;      // Number of rows in the grid

        // Add side and diagonal neighbors if they are within bounds
        AddSideNeighbors(expandedSpatialGroups, spatialGroup, partitionsPerRow, numberOfRows);
        AddDiagonalNeighbors(expandedSpatialGroups, spatialGroup, partitionsPerRow, numberOfRows);

        return expandedSpatialGroups;
    }

    // Helper method to check boundaries and add side neighbors (top, bottom, left, right)
    private static void AddSideNeighbors(List<int> groups, int spatialGroup, int partitionsPerRow, int numberOfRows)
    {
        bool isLeft = IsLeftEdge(spatialGroup, partitionsPerRow);
        bool isRight = IsRightEdge(spatialGroup, partitionsPerRow);
        bool isTop = IsTopEdge(spatialGroup, partitionsPerRow, numberOfRows);
        bool isBottom = IsBottomEdge(spatialGroup, partitionsPerRow);

        if (!isTop) groups.Add(spatialGroup + partitionsPerRow);     // Top neighbor
        if (!isBottom) groups.Add(spatialGroup - partitionsPerRow);  // Bottom neighbor
        if (!isLeft) groups.Add(spatialGroup - 1);                   // Left neighbor
        if (!isRight) groups.Add(spatialGroup + 1);                  // Right neighbor
    }

    // Helper method to check boundaries and add diagonal neighbors
    private static void AddDiagonalNeighbors(List<int> groups, int spatialGroup, int partitionsPerRow, int numberOfRows)
    {
        bool isLeft = IsLeftEdge(spatialGroup, partitionsPerRow);
        bool isRight = IsRightEdge(spatialGroup, partitionsPerRow);
        bool isTop = IsTopEdge(spatialGroup, partitionsPerRow, numberOfRows);
        bool isBottom = IsBottomEdge(spatialGroup, partitionsPerRow);

        if (!isTop && !isRight) groups.Add(spatialGroup + partitionsPerRow + 1);     // Top-right neighbor
        if (!isTop && !isLeft) groups.Add(spatialGroup + partitionsPerRow - 1);      // Top-left neighbor
        if (!isBottom && !isRight) groups.Add(spatialGroup - partitionsPerRow + 1);  // Bottom-right neighbor
        if (!isBottom && !isLeft) groups.Add(spatialGroup - partitionsPerRow - 1);   // Bottom-left neighbor
    }

    // Edge-checking methods to encapsulate boundary logic
    private static bool IsLeftEdge(int spatialGroup, int partitionsPerRow)
    {
        return spatialGroup % partitionsPerRow == 0;
    }

    private static bool IsRightEdge(int spatialGroup, int partitionsPerRow)
    {
        return spatialGroup % partitionsPerRow == partitionsPerRow - 1;
    }

    private static bool IsTopEdge(int spatialGroup, int partitionsPerRow, int numberOfRows)
    {
        return spatialGroup / partitionsPerRow >= numberOfRows - 1;
    }

    private static bool IsBottomEdge(int spatialGroup, int partitionsPerRow)
    {
        return spatialGroup / partitionsPerRow == 0;
    }

}