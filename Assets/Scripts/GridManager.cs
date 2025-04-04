using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

using DG.Tweening;

[System.Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;
}

public class GridManager : MonoBehaviour
{
    public string levelFileName; 
    public float tileSize = 0.5f; // Adjust based on prefab size

    public GameObject[,] tileGrid; // 2D array to store tiles
    public int gridWidth;
    public int gridHeight;
    public int remainingMoves;
    private int currentLevel;

    public Vector3 gridOffset;
    private LevelData levelData;
    public GameObject gridHolder;

    public GameObject boxPrefab;
    public GameObject greenPrefab;
    public GameObject vasePrefab;
    public GameObject stonePrefab;
    public GameObject redPrefab;
    public GameObject bluePrefab;
    public GameObject yellowPrefab;
    public GameObject verticalRocketPrefab;
    public GameObject horizontalRocketPrefab;

    public GameObject boxParticlePrefab;
    public GameObject vaseParticlePrefab;
    public GameObject stoneParticlePrefab;
    public GameObject greenParticlePrefab;
    public GameObject blueParticlePrefab;
    public GameObject yellowParticlePrefab;
    public GameObject redParticlePrefab;

    public GameObject rocketProjectileLeft;
    public GameObject rocketProjectileRight;
    public GameObject rocketProjectileUp;
    public GameObject rocketProjectileDown;

    public GameObject winParticlePrefab;
    public GameObject winPopupPrefab;
    public GameObject losePopupPrefab;




    void Start()
    {
        DOTween.Init();

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1); // Load saved level progress

        // correct one digit level numbers

        if (currentLevel < 10)
        {
            levelFileName = "level_0" + currentLevel + ".json";
        }
        else
        {
            levelFileName = "level_" + currentLevel + ".json";
        }

        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            LoadLevelData();
            GenerateGrid();
        }
    }

    void LoadLevelData()
    {
        string path = Path.Combine(Application.dataPath, "CaseStudyAssets2025/Levels", levelFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            levelData = JsonUtility.FromJson<LevelData>(json);
        }
        else
        {
            Debug.LogError("Level file not found: " + path);
        }
    }

    void GenerateGrid()
    {
        if (levelData == null) return;

        // Create a GridHolder if it doesn't exist
        gridHolder = new GameObject("GridHolder");

        float gridWidthPixel = levelData.grid_width * tileSize;
        float gridHeightPixel = levelData.grid_height * tileSize;

        // Center position
        Vector3 centerOffset = new Vector3(-gridWidthPixel / 2 + tileSize / 2, -gridHeightPixel / 2 - tileSize / 2, 0);
        gridOffset = centerOffset;

        // Initialize the 2D array
        tileGrid = new GameObject[levelData.grid_width, levelData.grid_height];

        gridHeight = levelData.grid_height;
        gridWidth = levelData.grid_width;
        remainingMoves = levelData.move_count; // set starting moves

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMovesText(remainingMoves);
        }

        for (int y = 0; y < levelData.grid_height; y++)
        {
            for (int x = 0; x < levelData.grid_width; x++)
            {
                int index = y * levelData.grid_width + x;
                string tileType = levelData.grid[index];

                // If tileType is "rand", pick a random color
                if (tileType == "rand")
                {
                    tileType = GetRandomColor();
                }

                GameObject tilePrefab = GetPrefab(tileType);
                if (tilePrefab != null)
                {
                    Vector3 position = new Vector3(x * tileSize, y * tileSize, 0) + centerOffset;
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                    tile.transform.localScale = tilePrefab.transform.localScale; // Keeps prefab scale
                    tile.transform.parent = gridHolder.transform; // Set parent to GridHolder

                    // Store tile in 2D array
                    tileGrid[x, y] = tile;
                }
            }
        }

        int remainingObjectives = CountRemainingObjectives();
        Debug.Log("Remaining Objectives: " + remainingObjectives);

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoalText(remainingObjectives);
        }

        CheckAndMarkRocketEligibleGroups();
    }

    public void AfterMove()
    {
        Debug.Log("AfterMove called. Remaining moves: " + remainingMoves);

        SpawnNewCubes();
        CheckAndMarkRocketEligibleGroups();

        remainingMoves--;
        Debug.Log("Remaining Moves: " + remainingMoves);

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMovesText(remainingMoves);
        }

        int remainingObjectives = CountRemainingObjectives();
        Debug.Log("Remaining Objectives: " + remainingObjectives);

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGoalText(remainingObjectives);
        }

        checkEndGame();
    }

    public void checkEndGame()
    {
        bool isWin = checkWin();
        if (!isWin)
        {
            checkLose();
        }
        
    }

    private bool checkWin()
    {
        int remainingObjectives = CountRemainingObjectives();
        if (remainingObjectives == 0)
        {
            Win();
            return true;
        }
        return false;
    }

    private bool checkLose()
    {
        if (remainingMoves <= 0)
        {

            Lose();
            return true;
        }
        return false;
    }

    private void Win()
    {
        Debug.Log("You Win! All objectives completed.");
       
        // Celebration effects
        StartCoroutine(WinCelebration());
    }

    private void Lose()
    {
        Debug.Log("Game Over!");
        StartCoroutine(LoseSequence());
    }

    private IEnumerator WinCelebration()
    {
        // Spawn celebration particles
        for (int i = 0; i < 4; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-gridWidth * tileSize / 2, gridWidth * tileSize / 2),
                Random.Range(-gridHeight * tileSize / 2, gridHeight * tileSize / 2),
                0
            );
            Instantiate(winParticlePrefab, randomPos, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }

        // Show win popup
        if (winPopupPrefab != null)
        {
            GameObject popup = Instantiate(winPopupPrefab, GameObject.Find("Canvas").transform);
            popup.GetComponent<WinPopup>().Initialize(3f, () => {
                // This callback runs when the popup closes
                ReturnToMainScene();
            });
        }
        else
        {
            // If no popup, just wait and return
            yield return new WaitForSeconds(3f);

            ReturnToMainScene();
        }
    }

    private IEnumerator LoseSequence()
    {

        // Show lose popup
        if (losePopupPrefab != null)
        {
            GameObject popup = Instantiate(losePopupPrefab, GameObject.Find("Canvas").transform);
            LosePopup losePopup = popup.GetComponent<LosePopup>();
            losePopup.Initialize(
                onRetry: () => {
                    // Retry current level
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                },
                onMainMenu: () => {
                    ReturnToMainSceneFail();
                }
            );
        }
        else
        {
            // Fallback if no popup
            yield return new WaitForSeconds(2f);
            ReturnToMainSceneFail();
        }
    }

    private void ReturnToMainScene()
    {
        Debug.Log("Returning to Main Scene");
        PlayButtonManager playButtonManager = FindFirstObjectByType<PlayButtonManager>();
        currentLevel++;
        Debug.Log("Current Level: " + currentLevel);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);  // Save progress locally
        PlayerPrefs.Save();  // Ensure it is written to storage

        if (playButtonManager != null)
        {
            
            playButtonManager.currentLevel = currentLevel;
            playButtonManager.UpdateButtonText();
        }
        SceneManager.LoadScene("MainScene");
    }

    private void ReturnToMainSceneFail()
    {
        PlayButtonManager playButtonManager = FindFirstObjectByType<PlayButtonManager>();
        if (playButtonManager != null)
        {
            playButtonManager.currentLevel = currentLevel;
            playButtonManager.UpdateButtonText();
        }
        SceneManager.LoadScene("MainScene");
    }


    public int CountRemainingObjectives()
    {
        int count = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (tileGrid[x, y] != null)
                {
                    Tile tileComponent = tileGrid[x, y].GetComponent<Tile>();
                    string tileType = tileComponent.tileType;
                    if (tileType == "v" 
                        || tileType == "bo"
                        || tileType == "s")
                    {
                        count++;
                    }
                }
            }
        }
        return count;
    }

    public int GetMatchingTiles(Vector3 position, string type)
    {
        Vector3 relativePosition = position - gridOffset;
        int x = Mathf.FloorToInt(relativePosition.x / tileSize);
        int y = Mathf.FloorToInt(relativePosition.y / tileSize);
        Debug.Log(FloodFillCount(x, y, type, new HashSet<Vector2Int>()));
        return FloodFillCount(x, y, type, new HashSet<Vector2Int>());
    }

    private int FloodFillCount(int x, int y, string type, HashSet<Vector2Int> visited)
    {
        if (x < 0 || y < 0 || x >= tileGrid.GetLength(0) || y >= tileGrid.GetLength(1)) return 0;
        if (visited.Contains(new Vector2Int(x, y))) return 0;
        if (tileGrid[x, y] == null || tileGrid[x, y].GetComponent<Tile>().tileType != type) return 0;

        visited.Add(new Vector2Int(x, y));

        return 1 +
            FloodFillCount(x + 1, y, type, visited) +
            FloodFillCount(x - 1, y, type, visited) +
            FloodFillCount(x, y + 1, type, visited) +
            FloodFillCount(x, y - 1, type, visited);
    }

    public void RemoveTile(Vector3 position, string type)
    {

        Vector3 relativePosition = position - gridOffset;
        int x = Mathf.FloorToInt(relativePosition.x / tileSize);
        int y = Mathf.FloorToInt(relativePosition.y / tileSize);

        HashSet<Vector2Int> matchingTiles = new HashSet<Vector2Int>();
        FloodFillMark(x, y, type, matchingTiles);

        Tile tileComponent = tileGrid[x, y].GetComponent<Tile>();
        bool hasRocket = tileComponent.hasHint;

        HashSet<Vector2Int> damagedVases = new HashSet<Vector2Int>();
        foreach (Vector2Int tilePos in matchingTiles)
        {
            DamageAdjacentVases(tilePos.x, tilePos.y, damagedVases);

            DestroyAdjacentBoxes(tilePos.x, tilePos.y);
            DamageTile(tilePos.x, tilePos.y);
        }

        // Check if the clicked tile should turn into a rocket
        if (hasRocket)
        {
            SpawnRocketTile(x, y);
        }


        AfterMove();
    }

    void DestroyAdjacentBoxes(int x, int y)
    {
        if (x + 1 < gridWidth)
        {
            if (tileGrid[x + 1, y] != null)
            {
                if (tileGrid[x + 1, y].GetComponent<Tile>().tileType == "bo")
                {
                    DamageTile(x + 1, y);
                }
            }
        }
        if (x - 1 >= 0)
        {
            if (tileGrid[x - 1, y] != null)
            {
                if (tileGrid[x - 1, y].GetComponent<Tile>().tileType == "bo")
                {
                    DamageTile(x - 1, y);
                }
            }
        }
        if (y + 1 < gridHeight)
        {
            if (tileGrid[x, y + 1] != null)
            {
                if (tileGrid[x, y + 1].GetComponent<Tile>().tileType == "bo")
                {
                    DamageTile(x, y + 1);
                }
            }
        }
        if (y - 1 >= 0)
        {
            if (tileGrid[x, y - 1] != null)
            {
                if (tileGrid[x, y - 1].GetComponent<Tile>().tileType == "bo")
                {
                    DamageTile(x, y - 1);
                }
            }
        }
    }

    void DamageAdjacentVases(int x, int y, HashSet<Vector2Int> damagedVases)
    {
        if (x + 1 < gridWidth)
        {
            if (tileGrid[x + 1, y] != null)
            {
                if (tileGrid[x + 1, y].GetComponent<Tile>().tileType == "v"
                    && !damagedVases.Contains(new Vector2Int(x + 1, y)))
                {
                    damagedVases.Add(new Vector2Int(x + 1, y));
                    DamageTile(x + 1, y);
                }
            }
        }
        if (x - 1 >= 0)
        {
            if (tileGrid[x - 1, y] != null)
            {
                if (tileGrid[x - 1, y].GetComponent<Tile>().tileType == "v"
                    && !damagedVases.Contains(new Vector2Int(x - 1, y)))
                {
                    damagedVases.Add(new Vector2Int(x - 1, y));
                    DamageTile(x - 1, y);
                }
            }
        }
        if (y + 1 < gridHeight)
        {
            if (tileGrid[x, y + 1] != null)
            {
                if (tileGrid[x, y + 1].GetComponent<Tile>().tileType == "v"
                    && !damagedVases.Contains(new Vector2Int(x, y + 1)))
                {
                    damagedVases.Add(new Vector2Int(x, y + 1));
                    DamageTile(x, y + 1);
                }
            }
        }
        if (y - 1 >= 0)
        {
            if (tileGrid[x, y - 1] != null)
            {
                if (tileGrid[x, y - 1].GetComponent<Tile>().tileType == "v"
                    && !damagedVases.Contains(new Vector2Int(x, y - 1)))
                {
                    damagedVases.Add(new Vector2Int(x, y - 1));
                    DamageTile(x, y - 1);
                }
            }
        }

    }

    void SpawnRocketTile(int x, int y)
    {
        string rocketType = GetRandomRocket();
        GameObject rocketPrefab = GetPrefab(rocketType);
        if (rocketPrefab != null)
        {
            Vector3 position = new Vector3(x * tileSize, y * tileSize, 0) + gridOffset;
            GameObject rocket = Instantiate(rocketPrefab, position, Quaternion.identity);
            rocket.transform.localScale = rocketPrefab.transform.localScale; // Keeps prefab scale
            rocket.transform.parent = gridHolder.transform; // Set parent to GridHolder

            // Store rocket in 2D array
            tileGrid[x, y] = rocket;
        }
    }


    private void FloodFillMark(int x, int y, string type, HashSet<Vector2Int> visited)
    {
        if (x < 0 || y < 0 || x >= tileGrid.GetLength(0) || y >= tileGrid.GetLength(1)) return;
        if (visited.Contains(new Vector2Int(x, y))) return;
        if (tileGrid[x, y] == null || tileGrid[x, y].GetComponent<Tile>().tileType != type) return;

        visited.Add(new Vector2Int(x, y));

        FloodFillMark(x + 1, y, type, visited);
        FloodFillMark(x - 1, y, type, visited);
        FloodFillMark(x, y + 1, type, visited);
        FloodFillMark(x, y - 1, type, visited);
    }

    public void SpawnNewCubes()
    {


        for (int x = 0; x < levelData.grid_width; x++)
        {
            int emptySpaces = 0;

            // Count empty spaces from bottom to top
            for (int y = 0; y < levelData.grid_height; y++)
            {
                if (tileGrid[x, y] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    if (tileGrid[x, y].GetComponent<Tile>().tileType == "bo"
                        || tileGrid[x, y].GetComponent<Tile>().tileType == "s") // box and stones cannot fall down
                    {
                        emptySpaces = 0;
                        continue;
                    }
                    // Move tile down
                    tileGrid[x, y - emptySpaces] = tileGrid[x, y];
                    tileGrid[x, y] = null;

                    // Animate falling effect
                    //StartCoroutine(AnimateTileDrop(tileGrid[x, y - emptySpaces], x, y - emptySpaces));

                    AnimateTileDropTWE(tileGrid[x, y - emptySpaces], x, y - emptySpaces);
                }
            }

            // Spawn new tiles at the top
            for (int i = 0; i < emptySpaces; i++)
            {
                int spawnY = levelData.grid_height - emptySpaces + i;
                SpawnTile(x, spawnY);
            }
        }
    }


    void SpawnTile(int x, int y)
    {
        string tileType = GetRandomColor();
        GameObject tilePrefab = GetPrefab(tileType);

        if (tilePrefab == null)
        {
            Debug.LogError("SpawnTile: Prefab not found for tile type: " + tileType);
            return;
        }

        // Calculate spawn position (above the top row)
        float spawnY = (levelData.grid_height * tileSize) + tileSize;
        Vector3 spawnPosition = new Vector3(x * tileSize, spawnY, 0) + gridOffset;

        // Instantiate tile
        GameObject newTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
        newTile.transform.localScale = tilePrefab.transform.localScale;
        newTile.transform.parent = GameObject.Find("GridHolder").transform;

        // Store in grid
        tileGrid[x, y] = newTile;

        // Animate falling effect
        //StartCoroutine(AnimateTileDrop(newTile, x, y));
        AnimateTileDropTWE(newTile, x,y);
    }

    void AnimateTileDropTWE(GameObject tile, int x, int targetY)
    {
        if (tile == null) return;

        Vector3 endPos = new Vector3(x * tileSize, targetY * tileSize, 0) + gridOffset;

        tile.transform.DOMove(endPos, 0.5f)
            .SetEase(Ease.OutBounce) // Adds nice bounce effect
            .OnComplete(() => {
                // Any completion logic
            });
    }

    public void CheckAndMarkRocketEligibleGroups()
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (tileGrid[x, y] == null || visited.Contains(new Vector2Int(x, y))) continue;

                Tile tileComponent = tileGrid[x, y].GetComponent<Tile>();
                string type = tileComponent.tileType;
                HashSet<Vector2Int> group = new HashSet<Vector2Int>();

                // Find all matching tiles in this group
                FindMatchingGroup(x, y, type, group, visited);


                if (group.Count >= 4)
                {
                    // Change to hint version
                    foreach (Vector2Int pos in group)
                    {
                        ChangeTileAppearance(pos.x, pos.y, true);
                    }
                }
                else
                {
                    // Revert back to normal
                    foreach (Vector2Int pos in group)
                    {
                        ChangeTileAppearance(pos.x, pos.y, false);
                    }
                }
            }
        }
    }


    void FindMatchingGroup(int x, int y, string type, HashSet<Vector2Int> group, HashSet<Vector2Int> visited)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return;
        if (visited.Contains(new Vector2Int(x, y))) return;
        if (tileGrid[x, y] == null || tileGrid[x, y].GetComponent<Tile>().tileType != type) return;

        visited.Add(new Vector2Int(x, y));
        group.Add(new Vector2Int(x, y));

        FindMatchingGroup(x + 1, y, type, group, visited);
        FindMatchingGroup(x - 1, y, type, group, visited);
        FindMatchingGroup(x, y + 1, type, group, visited);
        FindMatchingGroup(x, y - 1, type, group, visited);
    }


    void ChangeTileAppearance(int x, int y, bool showHint)
    {
        if (tileGrid[x, y] == null) return;

        SpriteRenderer spriteRenderer = tileGrid[x, y].GetComponent<SpriteRenderer>();
        Tile tileComponent = tileGrid[x, y].GetComponent<Tile>();

        if (tileComponent.tileType != "b" 
            && tileComponent.tileType != "g"
            && tileComponent.tileType != "r"
            && tileComponent.tileType != "y")
        {
            return; // Dont reassign sprites if its not a box
        }


        if (spriteRenderer != null && tileComponent != null)
        {
            if (showHint)
            {
                spriteRenderer.sprite = tileComponent.secondarySprite; // Assign hint sprite
                tileComponent.hasHint = true;
            }
            else
            { 
                spriteRenderer.sprite = tileComponent.normalSprite; // Revert back to normal
                tileComponent.hasHint = false;
            }
        }
    }

    public void BlastRocket(Vector3 position, string type)
    {


        Vector3 relativePosition = position - gridOffset;
        int x = Mathf.FloorToInt(relativePosition.x / tileSize);
        int y = Mathf.FloorToInt(relativePosition.y / tileSize);

        HashSet<Vector2Int> group = new HashSet<Vector2Int>();
        FindNeighborRockets(x, y, group);

        if (group.Count >= 2)
        {
            BlastCombo(x, y, group);
        }
        else if (type == "hro")
        {
            BlastLeftRight(x, y);
        }
        else if (type == "vro")
        {
            BlastUpDown(x, y);
        }
        else
        {
            Debug.Log("error in BlastRcoket function");
        }


        StartCoroutine(AfterBlastCheckEndgame());

    }

    private IEnumerator AfterBlastCheckEndgame()
    {
        
        yield return new WaitForSeconds(1f);
        Debug.Log("AfterBlastCheckEndgame called.");
        AfterMove();
        
    }


    void FindNeighborRockets(int x, int y,  HashSet<Vector2Int> group)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return;
        if (group.Contains(new Vector2Int(x, y))) return;
        if (tileGrid[x, y] == null ) return;
        if (tileGrid[x, y].GetComponent<Tile>().tileType != "hro"
            && tileGrid[x, y].GetComponent<Tile>().tileType != "vro") return;

        group.Add(new Vector2Int(x, y));

        FindNeighborRockets(x + 1, y, group);
        FindNeighborRockets(x - 1, y, group);
        FindNeighborRockets(x, y + 1, group);
        FindNeighborRockets(x, y - 1, group);
    }

    private void BlastCombo(int x, int y, HashSet<Vector2Int> group)
    {
        foreach (Vector2Int pos in group)
        {
            DamageTile(x, y);
        }
        if (x+1 <= gridWidth - 1) BlastUpDown(x + 1, y);
        BlastUpDown(x, y);
        if (x -1 >= 0) BlastUpDown(x - 1, y);
        if (y+1 <= gridHeight - 1) BlastLeftRight(x, y + 1);
        BlastLeftRight(x, y);
        if (y - 1 >= 0) BlastLeftRight(x, y - 1);
    }

    private void BlastLeftRight(int x, int y)
    {
        Destroy(tileGrid[x, y]);
        tileGrid[x, y] = null;
        Debug.Log("Tile destroyed at: " + x + ", " + y);
        SpawnProjectile(x, y, rocketProjectileLeft);   // Left
        SpawnProjectile(x, y, rocketProjectileRight);  // Right

    }

    private void BlastUpDown(int x, int y)
    {
        Destroy(tileGrid[x, y]);
        tileGrid[x, y] = null;
        Debug.Log("Tile destroyed at: " + x + ", " + y);
        SpawnProjectile(x, y, rocketProjectileUp);     // Up
        SpawnProjectile(x, y, rocketProjectileDown);   // Down

    }

    void SpawnProjectile(int x, int y, GameObject projectilePrefab)
    {
        Vector3 spawnPos = new Vector3(x * tileSize, y * tileSize, 0) + gridOffset;
        Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
    }

    public void DamageTile(int x, int y)
    {
        if (tileGrid[x, y] == null) { return; }
        SpriteRenderer spriteRenderer = tileGrid[x, y].GetComponent<SpriteRenderer>();
        Tile tileComponent = tileGrid[x, y].GetComponent<Tile>();
        string tileType = tileComponent.tileType;


        if (spriteRenderer != null && tileComponent != null)
        {
            if (tileComponent.tileType == "v")
            {
                VaseTile vaseComponent = tileComponent.GetComponent<VaseTile>();
                if (!vaseComponent.damaged) // check vase 1st hp
                {
                    SpawnParticles(tileGrid[x, y].transform.position, tileType);
                    spriteRenderer.sprite = tileComponent.secondarySprite; // Assign damaged sprite
                    vaseComponent.damaged = true;

                    return;
                }
            }
        }

        if (!(tileType == "hro" || tileType == "vro"))
        {
            SpawnParticles(tileGrid[x, y].transform.position, tileType);
        }

        if (tileType == "hro")
        {
            BlastLeftRight(x, y);
        }
        else if (tileType == "vro")
        {
            BlastUpDown(x, y);
        }

        if (tileGrid[x, y] == null) return;
        Destroy(tileGrid[x, y]);
        tileGrid[x, y] = null;
        Debug.Log("Tile destroyed at: " + x + ", " + y);
    }

    void SpawnParticles(Vector3 position, string tileType)
    {
        switch (tileType) {
            case "v":
                Instantiate(vaseParticlePrefab, position, Quaternion.identity);
                break;
            case "bo":
                Instantiate(boxParticlePrefab, position, Quaternion.identity);
                break;
            case "s":
                Instantiate(stoneParticlePrefab, position, Quaternion.identity);
                break;
            case "g":
                Instantiate(greenParticlePrefab, position, Quaternion.identity);
                break;
            case "b":
                Instantiate(blueParticlePrefab, position, Quaternion.identity);
                break;
            case "r":
                Instantiate(redParticlePrefab, position, Quaternion.identity);
                break;
            case "y":
                Instantiate(yellowParticlePrefab, position, Quaternion.identity);
                break;
            default:
                Debug.LogError("Unknown tile type: " + tileType);
                break;
        }
    }


    string GetRandomColor()
    {
        string[] colors = { "r", "g", "b", "y" };
        return colors[Random.Range(0, colors.Length)];
    }

    string GetRandomRocket()
    {
        string[] rockets = { "hro", "vro" };
        return rockets[Random.Range(0, rockets.Length)];
    }

    GameObject GetPrefab(string type)
    {
        switch (type)
        {
            case "bo": return boxPrefab;
            case "g": return greenPrefab;
            case "v": return vasePrefab;
            case "s": return stonePrefab;
            case "r": return redPrefab;
            case "b": return bluePrefab;
            case "y": return yellowPrefab;
            case "vro": return verticalRocketPrefab;
            case "hro": return horizontalRocketPrefab;
            default: return null;
        }
    }

    

    public Vector2 WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 relativePos = worldPosition - gridOffset;
        int x = Mathf.FloorToInt(relativePos.x / tileSize);
        int y = Mathf.FloorToInt(relativePos.y / tileSize);
        return new Vector2(x, y);
    }

    public bool IsPositionInGrid(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

}
