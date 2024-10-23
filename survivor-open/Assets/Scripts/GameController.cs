using System.Collections.Generic;

using UnityEngine;

public class GameController : MonoBehaviour
{

    public static GameController instance;
    public Transform player;
    
    public PlayerController PlayerScript { get { return playerScript; } }
    private PlayerController playerScript;

    // Spawning enemies
    public GameObject enemyPF;
    public Transform enemyHolder;

    private float enemySpawnTimer = 0f;
    private float enemySpawnTimerCD = 0f;
    private int maxEnemyCount = 10000;

    // Enemy logic
    private Dictionary<int, List<Enemy>> enemyBatches = new Dictionary<int, List<Enemy>>();

    private float runLogicTimer = 0f;
    private float runLogicTimerCD = 1f;

    //* SPATIAL PARTITIONING *//
    private int spatialGroupWidth = 100;

    public int SpatialGroupWidth
    { get { return spatialGroupWidth; } }

    private int spatialGroupHeight = 100;
    public int SpatialGroupHeight
    { get { return spatialGroupHeight; } }

    private int numberOfPartitions = 10000;
    public int NumberOfPartitions
    { get { return numberOfPartitions; } }

    private int mapWidthMin = -1;
    private int mapWidthMax = -1;
    private int mapHeightMin = -1;
    private int mapHeightMax = -1;


    // For enemies
    [HideInInspector] public Dictionary<int, HashSet<Enemy>> enemySpatialGroups = new Dictionary<int, HashSet<Enemy>>();

 


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

    private SortedSet<BatchScore> batchQueue_Enemy = new SortedSet<BatchScore>();

    // Keeps track of the current score of each batch
    private Dictionary<int, BatchScore> batchScoreMap_Enemy = new Dictionary<int, BatchScore>();

  
    public int GetBestBatch(string option)
    {
        if (option == "enemy") return GetBestBatchRaw(batchQueue_Enemy);
        else return -1;
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
    }


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
        playerScript = player.GetComponent<PlayerController>();

        InitializeBatches(); //! Initiate batch stuff

        // Create 400 -> 10000 spatial groups
        for (int i = 0; i < numberOfPartitions; i++)
        {
            enemySpatialGroups.Add(i, new HashSet<Enemy>());
         
        }

        // Spawn 10,000 enemies
        int initEnemyCount =  10000;
        maxEnemyCount = 10000;
        for (int i = 0; i < initEnemyCount; i++)
        {
            SpawnEnemy();
        }

        // Set map bounds
        mapWidthMin = -spatialGroupWidth / 2;
        mapWidthMax = spatialGroupWidth / 2;
        mapHeightMin = -spatialGroupHeight / 2;
        mapHeightMax = spatialGroupHeight / 2;
    }

    private void FixedUpdate() // 50 frames per second
    {
        if (instance.player == null) return;

        runLogicTimer += Time.deltaTime;

        if (runLogicTimer >= runLogicTimerCD)
        {
          
            runLogicTimer = 0f;
        }

        SpawnEnemies();
        RunBatchLogic((int)(runLogicTimer * 50)); // runLogicTimer is the batchID, for that set of enemies
    }

 

    private void RunBatchLogic(int batchID)
    {
        // Run logic for all enemies in batch
        foreach (Enemy enemy in enemyBatches[batchID])
        {
            if (enemy) enemy.RunLogic();
        }

        // TODO: Clean out previous batch?
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

    private void SpawnEnemy()
    {
        //! Which batch should it be added?
        int batchToBeAdded = GetBestBatch("enemy");

        // Get the QUADRANT of the player (25 quadrants in the map)
        int playerQuadrant = GetSpatialGroupDynamic(player.position.x, player.position.y, spatialGroupWidth, spatialGroupHeight, 25);
        List<int> expandedSpatialGroups = Utils.GetExpandedSpatialGroups(playerQuadrant, 25);

        // Remove the quadrant player is in
        expandedSpatialGroups.Remove(playerQuadrant);

        // Choose a random spatial group
        int randomSpatialGroup = expandedSpatialGroups[Random.Range(0, expandedSpatialGroups.Count)];

        // Get the center of that spatial group
        Vector2 centerOfSpatialGroup = GetPartitionCenterDynamic(randomSpatialGroup, spatialGroupWidth, spatialGroupHeight, 25);

        // Get a random position within that spatial group
        float sizeOfOneSpatialGroup = spatialGroupWidth / 5; // 100/5 -> 20
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
    }


    public int GetSpatialGroup(float xPos, float yPos)
    {
        return GetSpatialGroupDynamic(xPos, yPos, spatialGroupWidth, spatialGroupHeight, numberOfPartitions);
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


}

