using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    public float speed = 5f;          // Movement speed
    private GridManager gridManager;
    private Vector2 direction;        // Set based on prefab type
    bool isLongerTravel = false; // Flag to check if the projectile is a long travel one
    Vector2Int lastTilePos;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();

        // Get reference to the sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null && sr.sprite != null)
        {
            // Calculate scale to match tile size
            float targetSize = gridManager.tileSize;
            float spriteSize = sr.sprite.bounds.size.x;
            float scale = targetSize / spriteSize;

            transform.localScale = new Vector3(scale, scale, 1);
        }


        SetDirectionFromPrefabName(); // Auto-detect direction
        Move();
    }

    void SetDirectionFromPrefabName()
    {
        string prefabName = gameObject.name;
        if (prefabName.Contains("Left")) direction = Vector2.left;
        else if (prefabName.Contains("Right")) direction = Vector2.right;
        else if (prefabName.Contains("Up")) direction = Vector2.up;
        else if (prefabName.Contains("Down")) direction = Vector2.down;
    }

    void Move()
    {
        // Calculate exact grid boundaries in world space
        float gridLeft = gridManager.gridOffset.x;
        float gridRight = gridLeft + (gridManager.gridWidth * gridManager.tileSize);
        float gridBottom = gridManager.gridOffset.y;
        float gridTop = gridBottom + (gridManager.gridHeight * gridManager.tileSize);

        // Calculate target position based on direction
        Vector3 targetPos = transform.position;
        Vector3 siblingTargetPos = transform.position;

        if (direction == Vector2.left)
        {
            targetPos.x = gridLeft - 0.2f;
            siblingTargetPos.x = gridRight + 0.2f;
        }
        else if (direction == Vector2.right)
        {
            targetPos.x = gridRight + 0.2f;
            siblingTargetPos.x = gridLeft - 0.2f;
        }
        else if (direction == Vector2.up)
        {
            targetPos.y = gridTop + 0.2f;
            siblingTargetPos.y = gridBottom - 0.2f;
        }
        else if (direction == Vector2.down)
        {
            targetPos.y = gridBottom - 0.2f;
            siblingTargetPos.y = gridTop + 0.2f;
        }

        // Calculate movement duration based on distance
        float distance = Vector3.Distance(transform.position, targetPos);
        float siblingDistance = Vector3.Distance(transform.position, siblingTargetPos);

        if (distance > siblingDistance)
        {
            isLongerTravel = true;
        }

        float duration = distance / speed;

        transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(CheckCollision)
            .OnComplete(CompleteTravel);
    }

    void CompleteTravel()
    {
        if (isLongerTravel)
        {
            gridManager.SpawnNewCubes();
            gridManager.CheckAndMarkRocketEligibleGroups();
        }
        Destroy(gameObject);
    }

    void CheckCollision()
    {
        Vector2 gridPos = gridManager.WorldToGridPosition(transform.position);
        int x = Mathf.FloorToInt(gridPos.x);
        int y = Mathf.FloorToInt(gridPos.y);



        if (gridManager.IsPositionInGrid(x, y)
            && !( lastTilePos.x == x
            && lastTilePos.y == y))
        {

            Debug.Log("Last tile position: " + lastTilePos.x + ", " + lastTilePos.y);
            lastTilePos = new Vector2Int(x, y);
            Debug.Log("Rocket hit tile at: " + x + ", " + y);
            Debug.Log("Prefab name: " + gameObject.name);
            gridManager.DamageTile(x, y);
        }
    }
}