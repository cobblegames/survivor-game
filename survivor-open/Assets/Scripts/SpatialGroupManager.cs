using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! MIN HEAP FOR BATCH //
public class BatchScore : System.IComparable<BatchScore>
{
    public int BatchId { get; }
    public int Score { get; private set; }

    public BatchScore(int batchId, int score)
    {
        BatchId = batchId;
        Score = score;
    }

    public void UpdateScore(int delta)
    {
        Score += delta;
    }

    public int CompareTo(BatchScore other)
    {
        int scoreComparison = Score.CompareTo(other.Score);
        if (scoreComparison == 0)
        {
            // When scores are equal, further compare based on BatchId
            return BatchId.CompareTo(other.BatchId);
        }
        return scoreComparison;
    }
}

public class SpatialGroupManager : MonoBehaviour, IControllable
{
    public SpatialGroupsData spatialData;

    // Enemy logic
    private Dictionary<int, List<Enemy>> enemyBatches = new Dictionary<int, List<Enemy>>();

    public Dictionary<int, HashSet<Enemy>> enemySpatialGroups = new Dictionary<int, HashSet<Enemy>>();
    public Dictionary<int, HashSet<BaseBullet>> bulletSpatialGroups = new Dictionary<int, HashSet<BaseBullet>>();
    private SortedSet<BatchScore> batchQueue_Enemy = new SortedSet<BatchScore>();

    // Keeps track of the current score of each batch
    private Dictionary<int, BatchScore> batchScoreMap_Enemy = new Dictionary<int, BatchScore>();

    private float enemySpawnTimer = 0f;
    private float enemySpawnTimerCD = 0f;

    private float runLogicTimer = 0f;
    private float runLogicTimerCD = 1f;

    private int mapWidthMin = -1;
    private int mapWidthMax = -1;
    private int mapHeightMin = -1;
    private int mapHeightMax = -1;

    // For get spatial group STATIC (more efficient) calculations
    private int CELLS_PER_ROW_STATIC;

    private int CELLS_PER_COLUMN_STATIC; // Square grid assumption
    private float CELL_WIDTH_STATIC;
    private float CELL_HEIGHT_STATIC;
    private int HALF_WIDTH_STATIC;
    private int HALF_HEIGHT_STATIC;

    private PlayerController playerController;
    private PoolManager poolManager;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    private bool isInitialized;

    public bool IsInitialized
    { get { return isInitialized; } }

    private void OnEnable()
    {
        GameEvents.OnStartGame += InitializeBatches;
    }

    private void OnDisable()
    {
        GameEvents.OnStartGame -= InitializeBatches;
    }

    public void Initialize(IControllable[] _injectedElements)
    {
        this.playerController = _injectedElements[0] as PlayerController;
        this.poolManager = _injectedElements[1] as PoolManager;
    }

    private void InitializeBatches()
    {
        // Set map bounds
        mapWidthMin = -spatialData.SpatialGroupWidth / 2;
        mapWidthMax = spatialData.SpatialGroupWidth / 2;
        mapHeightMin = -spatialData.SpatialGroupHeight / 2;
        mapHeightMax = spatialData.SpatialGroupHeight / 2;

        // STATIC GET SPATIAL GROUP ONCE CALCULATIONS

        CELLS_PER_ROW_STATIC = (int)Mathf.Sqrt(spatialData.NumberOfPartitions);
        CELLS_PER_COLUMN_STATIC = CELLS_PER_ROW_STATIC; // Square grid assumption
        CELL_WIDTH_STATIC = spatialData.SpatialGroupWidth / CELLS_PER_ROW_STATIC;
        CELL_HEIGHT_STATIC = spatialData.SpatialGroupHeight / CELLS_PER_COLUMN_STATIC;
        HALF_WIDTH_STATIC = spatialData.SpatialGroupWidth / 2;
        HALF_HEIGHT_STATIC = spatialData.SpatialGroupHeight / 2;

        for (int i = 0; i < spatialData.BatchCount; i++)
        {
            BatchScore batchScore = new BatchScore(i, 0);

            // Enemies
            enemyBatches.Add(i, new List<Enemy>()); // batches
            batchScoreMap_Enemy.Add(i, batchScore); // batch scores
            batchQueue_Enemy.Add(batchScore); // batch queue
        }

        // Create 400 -> 10000 spatial groups
        for (int i = 0; i < spatialData.NumberOfPartitions; i++)
        {
            enemySpatialGroups.Add(i, new HashSet<Enemy>());

            if (!bulletSpatialGroups.ContainsKey(i))
            {
                bulletSpatialGroups.Add(i, new HashSet<BaseBullet>());
            }
        }

        isInitialized = true;

        StartCoroutine(SpatialManagerMainCoroutiune());
    }

    private IEnumerator SpatialManagerMainCoroutiune()
    {
        while (playerController != null)
        {
            runLogicTimer += Time.deltaTime;

            if (runLogicTimer >= runLogicTimerCD)
            {
                for (int i = 0; i < enemyBatches.Count; i++)
                {
                    RunIntervalLogic(i);
                }
                runLogicTimer = 0f;
            }

            SpawnEnemies();
            for (int i = 0; i < enemyBatches.Count; i++)
            {
                RunEveryFrameLogic(i);
            }

            yield return waitForEndOfFrame;
        }

        Debug.Log("Player is absent - probably dead or quit");
    }

    private void RunIntervalLogic(int batchID)
    {
        // Run logic for all enemies in batch
        foreach (Enemy enemy in enemyBatches[batchID])
        {
            if (enemy) enemy.IntervalLogic();
        }
    }

    private void RunEveryFrameLogic(int batchID)
    {
        // Run logic for all enemies in batch
        foreach (Enemy enemy in enemyBatches[batchID])
        {
            if (enemy) enemy.EveryFrameLogic();
            else Debug.Log("Enemy is null");
        }
    }

    public List<Enemy> GetAllEnemiesInSpatialGroups(List<int> spatialGroups)
    {
        List<Enemy> enemies = new List<Enemy>();

        foreach (int spatialGroup in spatialGroups)
        {
            //Enemy enemy;
            enemySpatialGroups.TryGetValue(spatialGroup, out var enemy);
            if (enemy != null)
            {
                enemies.AddRange(enemy);
            }
        }

        return enemies;
    }

    public List<int> GetExpandedSpatialGroups(int spatialGroup, Vector2 direction)
    {
        List<int> expandedSpatialGroups = new List<int>() { spatialGroup };

        bool goingRight = direction.x > 0;
        bool goingTop = direction.y > 0;

        int widthRange = spatialData.SpatialGroupWidth;  // ex. 100
        int heightRange = spatialData.SpatialGroupHeight; // ex. 100

        bool isLeft = spatialGroup % widthRange == 0;
        bool isRight = spatialGroup % widthRange == widthRange - 1;
        bool isTop = spatialGroup / widthRange == heightRange - 1;
        bool isBottom = spatialGroup / widthRange == 0;

        // Sides
        if (!isTop && goingTop) expandedSpatialGroups.Add(spatialGroup + widthRange);
        if (!isBottom && !goingTop) expandedSpatialGroups.Add(spatialGroup - widthRange);
        if (!isLeft && !goingRight) expandedSpatialGroups.Add(spatialGroup - 1);
        if (!isRight && goingRight) expandedSpatialGroups.Add(spatialGroup + 1);

        // Diagonals
        if (!isTop && !isRight && (goingTop || goingRight)) expandedSpatialGroups.Add(spatialGroup + widthRange + 1); // top right
        if (!isTop && !isLeft && (goingTop || !goingRight)) expandedSpatialGroups.Add(spatialGroup + widthRange - 1); // top left
        if (!isBottom && !isRight && (!goingTop || goingRight)) expandedSpatialGroups.Add(spatialGroup - widthRange + 1); // bottom right
        if (!isBottom && !isLeft && (!goingTop || !goingRight)) expandedSpatialGroups.Add(spatialGroup - widthRange - 1); // bottom left

        return expandedSpatialGroups;
    }

    public List<int> GetExpandedSpatialGroups(int spatialGroup, int numberOfPartitions = -1)
    {
        List<int> expandedSpatialGroups = new List<int>() { spatialGroup };

        int widthRange = spatialData.SpatialGroupWidth;  // ex. 100
        int heightRange = spatialData.SpatialGroupHeight; // ex. 100
        if (numberOfPartitions == -1)
            numberOfPartitions = spatialData.NumberOfPartitions; // ex. 10000 -or- 25

        int sqrtOfPartitions = (int)Mathf.Sqrt(numberOfPartitions); // Square root of partitions
        int partitionsPerRow = sqrtOfPartitions;  // Number of columns in the grid
        int numberOfRows = sqrtOfPartitions;      // Number of rows in the grid

        // Add side and diagonal neighbors if they are within bounds
        AddSideNeighbors(expandedSpatialGroups, spatialGroup, partitionsPerRow, numberOfRows);
        AddDiagonalNeighbors(expandedSpatialGroups, spatialGroup, partitionsPerRow, numberOfRows);

        return expandedSpatialGroups;
    }

    // Helper method to check boundaries and add side neighbors (top, bottom, left, right)
    private void AddSideNeighbors(List<int> groups, int spatialGroup, int partitionsPerRow, int numberOfRows)
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
    private void AddDiagonalNeighbors(List<int> groups, int spatialGroup, int partitionsPerRow, int numberOfRows)
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
    private bool IsLeftEdge(int spatialGroup, int partitionsPerRow)
    {
        return spatialGroup % partitionsPerRow == 0;
    }

    private bool IsRightEdge(int spatialGroup, int partitionsPerRow)
    {
        return spatialGroup % partitionsPerRow == partitionsPerRow - 1;
    }

    private bool IsTopEdge(int spatialGroup, int partitionsPerRow, int numberOfRows)
    {
        return spatialGroup / partitionsPerRow >= numberOfRows - 1;
    }

    private bool IsBottomEdge(int spatialGroup, int partitionsPerRow)
    {
        return spatialGroup / partitionsPerRow == 0;
    }

    private void SpawnEnemies()
    {
        enemySpawnTimer += Time.deltaTime;

        if (enemySpawnTimer > enemySpawnTimerCD && poolManager.EnemyHolder.childCount < spatialData.MaxEnemyCount)
        {
            SpawnEnemy();

            enemySpawnTimer = 0f;
        }
    }

    private int GetBestBatchRaw(SortedSet<BatchScore> batchQueue)
    {
        BatchScore leastLoadedBatch = batchQueue.Min;

        // Debug.Log("Least loaded: " + leastLoadedBatch.BatchId + ", score: " + leastLoadedBatch.Score);

        if (leastLoadedBatch == null)
        {
            // Handle the case where there are no batches
            Debug.Log("THIS SHOULDN'T HAPPEN");
            return 0;
        }

        batchQueue.Remove(leastLoadedBatch); // Remove OLD
        leastLoadedBatch.UpdateScore(1);
        batchQueue.Add(leastLoadedBatch); // Add NEW

        return leastLoadedBatch.BatchId;
    }

    private int GetBestBatch(string option)
    {
        if (option == "enemy") return GetBestBatchRaw(batchQueue_Enemy);
        else return -1;
    }

    /*
       X X X X X    < Expanded
       X X X X X
       X X P X X
       X X X X X
       X X X X X
     */

    private void SpawnEnemy()
    {
        //! Which batch should it be added?
        int batchToBeAdded = GetBestBatch("enemy");

        // Get the QUADRANT of the player (25 quadrants in the map)
        int playerQuadrant = GetSpatialGroupDynamic(playerController.transform.position.x, playerController.transform.position.y, spatialData.SpatialGroupWidth, spatialData.SpatialGroupHeight, 25);
        List<int> expandedSpatialGroups = GetExpandedSpatialGroups(playerQuadrant, 25);

        // Remove the quadrant player is in
        expandedSpatialGroups.Remove(playerQuadrant);

        // Choose a random spatial group
        int randomSpatialGroup = expandedSpatialGroups[Random.Range(0, expandedSpatialGroups.Count)];

        // Get the center of that spatial group
        Vector2 centerOfSpatialGroup = GetPartitionCenterDynamic(randomSpatialGroup, spatialData.SpatialGroupWidth, spatialData.SpatialGroupHeight, 25);

        // Get a random position within that spatial group
        float sizeOfOneSpatialGroup = spatialData.SpatialGroupWidth / 5; // 100/5 -> 20
        float xVal = Random.Range(centerOfSpatialGroup.x - sizeOfOneSpatialGroup / 2, centerOfSpatialGroup.x + sizeOfOneSpatialGroup / 2);
        float yVal = Random.Range(centerOfSpatialGroup.y - sizeOfOneSpatialGroup / 2, centerOfSpatialGroup.y + sizeOfOneSpatialGroup / 2);

        GameObject enemyGO = Instantiate(poolManager.SpawnFromPool("Skeleton"), poolManager.EnemyHolder);
        Enemy enemyScript = enemyGO.GetComponent<Enemy>();
        enemyGO.transform.position = new Vector3(xVal, yVal, 0);
        enemyGO.SetActive(true);
        // Spatial group
        int spatialGroup = GetSpatialGroup(enemyGO.transform.position.x, enemyGO.transform.position.y);
        enemyScript.SpatialGroup = spatialGroup;
        AddToSpatialGroup(spatialGroup, enemyScript);

        // Batch for update logic
        enemyScript.BatchID = batchToBeAdded;
        enemyBatches[batchToBeAdded].Add(enemyScript);

        enemyScript.Initialize(new IControllable[] { this, playerController });
    }

    private Vector2 GetPartitionCenterDynamic(int partition, float mapWidth, float mapHeight, int totalPartitions)
    {
        // Calculate the number of cells per row and column, assuming a square grid
        int cellsPerRow = (int)Mathf.Sqrt(totalPartitions);
        int cellsPerColumn = cellsPerRow; // Square grid assumption

        // Calculate the size of each cell
        float cellWidth = mapWidth / cellsPerRow;
        float cellHeight = mapHeight / cellsPerColumn;

        // Calculate the row and column index of the partition
        int rowIndex = partition / cellsPerRow;
        int columnIndex = partition % cellsPerRow;

        // Calculate the center coordinates of the partition
        float centerX = (columnIndex + 0.5f) * cellWidth - (mapWidth / 2);
        float centerY = (rowIndex + 0.5f) * cellHeight - (mapHeight / 2);

        return new Vector2(centerX, centerY);
    }

    public void AddToSpatialGroup(int spatialGroupID, Enemy enemy)
    {
        enemySpatialGroups[spatialGroupID].Add(enemy);
    }

    public void RemoveFromSpatialGroup(int spatialGroupID, Enemy enemy)
    {
        enemySpatialGroups[spatialGroupID].Remove(enemy);
    }

    public int GetSpatialGroup(float xPos, float yPos)
    {
        return GetSpatialGroupDynamic(xPos, yPos, spatialData.SpatialGroupWidth, spatialData.SpatialGroupHeight, spatialData.NumberOfPartitions);
    }

    private int GetSpatialGroupDynamic(float xPos, float yPos, float mapWidth, float mapHeight, int totalPartitions)
    {
        // Calculate the number of cells per row and column, assuming a square grid
        int cellsPerRow = (int)Mathf.Sqrt(totalPartitions);
        int cellsPerColumn = cellsPerRow; // Square  grid assumption

        // Calculate the size of each cell
        float cellWidth = mapWidth / cellsPerRow;
        float cellHeight = mapHeight / cellsPerColumn;

        if (cellWidth <= 0 || cellHeight <= 0)
        {
            Debug.LogError("Invalid cell dimensions in spatial group calculation."); return 0;
        }

        // Adjust positions to map's coordinate system
        float adjustedX = xPos + (mapWidth / 2);
        float adjustedY = yPos + (mapHeight / 2);

        // Calculate the indices
        int xIndex = (int)(adjustedX / cellWidth);
        int yIndex = (int)(adjustedY / cellHeight);

        // Ensure indices are within the range
        xIndex = Mathf.Clamp(xIndex, 0, cellsPerRow - 1);
        yIndex = Mathf.Clamp(yIndex, 0, cellsPerColumn - 1);

        // Calculate the final index
        return xIndex + yIndex * cellsPerRow;
    }

    public bool IsOutOfBounds(Vector2 _position)
    {
        return _position.x < mapWidthMin || _position.x > mapWidthMax ||
               _position.y < mapHeightMin || _position.y > mapHeightMax;
    }

    public int GetSpatialGroupStatic(float xPos, float yPos)
    {
        float adjustedX = xPos + HALF_WIDTH_STATIC;
        float adjustedY = yPos + HALF_HEIGHT_STATIC;

        // Calculate the indices
        int xIndex = (int)(adjustedX / CELL_WIDTH_STATIC);
        int yIndex = (int)(adjustedY / CELL_HEIGHT_STATIC);

        // Calculate the final index
        return xIndex + yIndex * CELLS_PER_ROW_STATIC;
    }
}