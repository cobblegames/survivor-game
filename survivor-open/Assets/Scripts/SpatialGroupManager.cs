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
    // For bullets
    public Dictionary<int, HashSet<Bullet>> bulletSpatialGroups = new Dictionary<int, HashSet<Bullet>>();


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


    // For get spatial group STATIC (more efficient) calculations
    int CELLS_PER_ROW_STATIC;
    int CELLS_PER_COLUMN_STATIC; // Square grid assumption
    float CELL_WIDTH_STATIC;
    float CELL_HEIGHT_STATIC;
    int HALF_WIDTH_STATIC;
    int HALF_HEIGHT_STATIC;

    private PlayerController playerControllerReference;

    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

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
        }

        for (int i = 0; i < spatialData.InitEnemyCount; i++)
        {
            SpawnEnemy();
        }

        // Set map bounds
        mapWidthMin = -spatialData.SpatialGroupWidth / 2;
        mapWidthMax = spatialData.SpatialGroupWidth / 2;
        mapHeightMin = -spatialData.SpatialGroupHeight / 2;
        mapHeightMax = spatialData.SpatialGroupHeight / 2;

        //Init Static Data Once

        CELLS_PER_ROW_STATIC = (int)Mathf.Sqrt(spatialData.NumberOfPartitions);
        CELLS_PER_COLUMN_STATIC = CELLS_PER_ROW_STATIC; // Square grid assumption
        CELL_WIDTH_STATIC = spatialData.SpatialGroupWidth / CELLS_PER_ROW_STATIC;
        CELL_HEIGHT_STATIC = spatialData.SpatialGroupHeight / CELLS_PER_COLUMN_STATIC;
        HALF_WIDTH_STATIC = spatialData.SpatialGroupWidth / 2;
        HALF_HEIGHT_STATIC = spatialData.SpatialGroupHeight / 2;

        StartCoroutine(SpatialManagerMainCoroutiune());
    }

    private IEnumerator SpatialManagerMainCoroutiune()// 50 frames per second
    {
        while (playerControllerReference != null)
        {
            runLogicTimer += Time.deltaTime;

            if (runLogicTimer >= runLogicTimerCD)
            {
                RunOnceASecondLogicForAllBullets();
                runLogicTimer = 0f;
            }

            SpawnEnemies();

            RunEnemyLogic((int)(runLogicTimer)); // runLogicTimer is the batchID, for that set of enemies

            yield return waitForEndOfFrame;
        }

        Debug.Log("Player is absent - probably dead or quit");
    }


    private void RunBulletLogic(int batchID)
    {
        // Run logic for all enemies in batch
        foreach (Bullet bullet in bulletSpatialGroups.SelectMany(x => x.Value).ToList())
        {
            if (bullet) bullet.EveryFrameLogic();
        }
    }

    private void RunEnemyLogic(int batchID)
    {
        // Run logic for all enemies in batch
        foreach (Enemy enemy in enemyBatches[batchID])
        {
            if (enemy) enemy.EveryFrameLogic();
        }    
    }

    private void RunOnceASecondLogicForAllBullets()
    {
        
        foreach (Bullet bullet in bulletSpatialGroups.SelectMany(x => x.Value).ToList())
        {
            bullet.OnceASecondLogic();
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
    public List<int> GetExpandedSpatialGroups(int spatialGroup, Vector2? direction = null, int numberOfPartitions = -1)
    {
        List<int> expandedSpatialGroups = new List<int> { spatialGroup };

        int partitionsPerRow = (int)Mathf.Sqrt(numberOfPartitions == -1 ? spatialData.NumberOfPartitions : numberOfPartitions);
        int totalRows = partitionsPerRow;

        bool goingRight = direction.HasValue && direction.Value.x > 0;
        bool goingTop = direction.HasValue && direction.Value.y > 0;

        // Boundary checks
        bool isLeft = IsLeftEdge(spatialGroup, partitionsPerRow);
        bool isRight = IsRightEdge(spatialGroup, partitionsPerRow);
        bool isTop = IsTopEdge(spatialGroup, totalRows);
        bool isBottom = IsBottomEdge(spatialGroup, partitionsPerRow);

        // Add neighbors based on direction if provided
        AddNeighbors(expandedSpatialGroups, spatialGroup, partitionsPerRow, isLeft, isRight, isTop, isBottom, goingRight, goingTop);

        return expandedSpatialGroups;
    }

    private void AddNeighbors(
        List<int> groups, int spatialGroup, int partitionsPerRow,
        bool isLeft, bool isRight, bool isTop, bool isBottom,
        bool goingRight, bool goingTop)
    {
        // Sides
        if (!isTop && goingTop) groups.Add(spatialGroup + partitionsPerRow);
        if (!isBottom && !goingTop) groups.Add(spatialGroup - partitionsPerRow);
        if (!isLeft && !goingRight) groups.Add(spatialGroup - 1);
        if (!isRight && goingRight) groups.Add(spatialGroup + 1);

        // Diagonals
        if (!isTop && !isRight && (goingTop || goingRight)) groups.Add(spatialGroup + partitionsPerRow + 1); // top right
        if (!isTop && !isLeft && (goingTop || !goingRight)) groups.Add(spatialGroup + partitionsPerRow - 1); // top left
        if (!isBottom && !isRight && (!goingTop || goingRight)) groups.Add(spatialGroup - partitionsPerRow + 1); // bottom right
        if (!isBottom && !isLeft && (!goingTop || !goingRight)) groups.Add(spatialGroup - partitionsPerRow - 1); // bottom left
    }

    // Edge-check methods
    private bool IsLeftEdge(int spatialGroup, int partitionsPerRow) => spatialGroup % partitionsPerRow == 0;

    private bool IsRightEdge(int spatialGroup, int partitionsPerRow) => spatialGroup % partitionsPerRow == partitionsPerRow - 1;

    private bool IsTopEdge(int spatialGroup, int totalRows) => spatialGroup / totalRows >= totalRows - 1;

    private bool IsBottomEdge(int spatialGroup, int partitionsPerRow) => spatialGroup / partitionsPerRow == 0;


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
        List<int> expandedSpatialGroups = GetExpandedSpatialGroups(playerQuadrant,null, 25);

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
        enemyScript.SpatialGroup = spatialGroup;
        AddToSpatialGroup(spatialGroup, enemyScript);

        // Batch for update logic
        enemyScript.BatchID = batchToBeAdded;
        enemyBatches[batchToBeAdded].Add(enemyScript);

        enemyScript.Initialize(this, playerControllerReference); //Inject dependency
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

    public bool IsOutOfBounds(Vector2 _position)
    {
        if( _position.x < mapWidthMin || _position.x > mapWidthMax|| _position.y < mapHeightMin|| _position.y > mapHeightMax ||
           Vector2.Distance(_position, playerControllerReference.transform.position) > 20f)
            return true;
        else return false;
    }

    public int GetSpatialGroupStatic(float xPos, float yPos)
    {
        // Adjust positions to map's coordinate system
        float adjustedX = xPos + spatialData.SpatialGroupWidth/2;
        float adjustedY = yPos + spatialData.SpatialGroupHeight / 2; ;

        // Calculate the indices
        int xIndex = (int)(adjustedX / CELL_WIDTH_STATIC);
        int yIndex = (int)(adjustedY / CELL_HEIGHT_STATIC);

        // Calculate the final index
        return xIndex + yIndex * CELLS_PER_ROW_STATIC;
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