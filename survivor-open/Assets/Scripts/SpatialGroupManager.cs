using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class SpatialGroupManager : MonoBehaviour
{
    public SpatialGroupsData spatialData;

    // Enemy logic
    private Dictionary<int, List<Enemy>> enemyBatches = new Dictionary<int, List<Enemy>>();

    // For enemies
    public Dictionary<int, HashSet<Enemy>> enemySpatialGroups = new Dictionary<int, HashSet<Enemy>>();

    private SortedSet<BatchScore> batchQueue_Enemy = new SortedSet<BatchScore>();

    // Keeps track of the current score of each batch
    private Dictionary<int, BatchScore> batchScoreMap_Enemy = new Dictionary<int, BatchScore>();

    // Spawning enemies
    public GameObject enemyPF;

    public Transform enemyHolder;

    private float enemySpawnTimer = 0f;
    private float enemySpawnTimerCD = 0f;
    private int maxEnemyCount = 10000;

    private float runLogicTimer = 0f;
    private float runLogicTimerCD = 1f;

    private int mapWidthMin = -1;
    private int mapWidthMax = -1;
    private int mapHeightMin = -1;
    private int mapHeightMax = -1;

    private PlayerController playerControllerReference;

    private void OnEnable()
    {
        GameEvents.OnStartGame += InitializeBatches;
    }

    private void OnDisable()
    {
        GameEvents.OnStartGame -= InitializeBatches;
    }

    public void Initialize(PlayerController _player)
    {
        this.playerControllerReference = _player;
    }

    private void InitializeBatches()
    {
        for (int i = 0; i < 50; i++)
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
        }

        // Spawn 10,000 enemies
        int initEnemyCount = 10000;
        maxEnemyCount = 10000;
        for (int i = 0; i < initEnemyCount; i++)
        {
            SpawnEnemy();
        }

        // Set map bounds
        mapWidthMin = -spatialData.SpatialGroupWidth / 2;
        mapWidthMax = spatialData.SpatialGroupWidth / 2;
        mapHeightMin = -spatialData.SpatialGroupHeight / 2;
        mapHeightMax = spatialData.SpatialGroupHeight / 2;

        StartCoroutine(SpatialManagerMainCoroutiune());
    }

    private IEnumerator SpatialManagerMainCoroutiune()// 50 frames per second
    {
        while (playerControllerReference != null)
        {
            runLogicTimer += Time.deltaTime;

            if (runLogicTimer >= runLogicTimerCD)
            {
                runLogicTimer = 0f;
            }

            SpawnEnemies();
            RunBatchLogic((int)(runLogicTimer * 50)); // runLogicTimer is the batchID, for that set of enemies

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Player is absent - probably dead or quit");
    }

    private void RunBatchLogic(int batchID)
    {
        // Run logic for all enemies in batch
        foreach (Enemy enemy in enemyBatches[batchID])
        {
            if (enemy) RunEnemyLogic(enemy);
        }

        // TODO: Clean out previous batch?
    }

    private void RunEnemyLogic(Enemy _enemy)
    {
        // Calculate direction
        _enemy.currentMovementDirection = playerControllerReference.transform.position - _enemy.transform.position;
        _enemy.currentMovementDirection.Normalize();

        // Move towards the player //TODO: Maybe have this run every frame? (no, but can definetely be more)
        _enemy.transform.position += _enemy.currentMovementDirection * Time.deltaTime * _enemy.movementSpeed;

        // Push other nearby enemies away
        PushNearbyEnemies(_enemy);

        // Update spatial group
        int newSpatialGroup = GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
        if (newSpatialGroup != _enemy.spatialGroup)
        {
            enemySpatialGroups[_enemy.spatialGroup].Remove(_enemy); // REMOVE from old spatial group

            _enemy.spatialGroup = newSpatialGroup; // UPDATE current spatial group
            enemySpatialGroups[_enemy.spatialGroup].Add(_enemy); // ADD to new spatial group
        }
    }

    private void PushNearbyEnemies(Enemy _enemy)
    {
        List<Enemy> currAreaEnemies = enemySpatialGroups[_enemy.spatialGroup].ToList(); // ONLY enemies in the same spatial group

        // Check each enemy, if the distance between them is less than 0.2, push it away
        foreach (Enemy enemy in currAreaEnemies) // ONLY enemies in the same spatial group
        {
            if (enemy == null) continue;
            if (enemy == this) continue;

            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y);
            if (distance < 0.2f)
            {
                // Push this enemy away
                Vector3 direction = transform.position - enemy.transform.position;
                direction.Normalize();
                enemy.transform.position -= direction * Time.deltaTime * _enemy.movementSpeed * 5;
            }
        }
    }

    public List<Enemy> GetAllEnemiesInSpatialGroups(List<int> spatialGroups)
    {
        List<Enemy> enemies = new List<Enemy>();

        foreach (int spatialGroup in spatialGroups)
        {
            enemies.AddRange(enemySpatialGroups[spatialGroup]);
        }

        return enemies;
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

        if (enemySpawnTimer > enemySpawnTimerCD && enemyHolder.childCount < maxEnemyCount)
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnEnemy();
            }

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

    private void SpawnEnemy()
    {
        //! Which batch should it be added?
        int batchToBeAdded = GetBestBatch("enemy");

        // Get the QUADRANT of the player (25 quadrants in the map)
        int playerQuadrant = GetSpatialGroupDynamic(playerControllerReference.transform.position.x, playerControllerReference.transform.position.y, spatialData.SpatialGroupWidth, spatialData.SpatialGroupHeight, 25);
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

        GameObject enemyGO = Instantiate(enemyPF, enemyHolder);
        enemyGO.transform.position = new Vector3(xVal, yVal, 0);
        enemyGO.transform.parent = enemyHolder;

        Enemy enemyScript = enemyGO.GetComponent<Enemy>();

        // Spatial group
        int spatialGroup = GetSpatialGroup(enemyGO.transform.position.x, enemyGO.transform.position.y);
        enemyScript.spatialGroup = spatialGroup;
        AddToSpatialGroup(spatialGroup, enemyScript);

        // Batch for update logic
        enemyScript.BatchID = batchToBeAdded;
        enemyBatches[batchToBeAdded].Add(enemyScript);

        enemyScript.Initialize(this); //Inject dependency
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
}