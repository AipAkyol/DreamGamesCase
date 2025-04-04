using UnityEngine;

public class Tile : MonoBehaviour
{
    public string tileType; // The type of tile (r, g, b, y, etc.)
    public Sprite normalSprite;
    public Sprite secondarySprite;
    public bool hasHint = false;

    protected GridManager gridManager; // Reference to the grid

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>(); // Find the grid manager in the scene
    }

    protected virtual void OnMouseDown()
    {

        if (IsAnyPopupActive() || IsAnyRocketProjectileActive()) return;

        Debug.Log("Tile tapped: " + tileType);

        if (CanBlast())
        {
            BlastTile();
        }
    }

     bool CanBlast()
    {
        // Check if there are at least 2 adjacent tiles of the same color
        return gridManager.GetMatchingTiles(transform.position, tileType) >= 2;
    }

    void BlastTile()
    {
        gridManager.RemoveTile(transform.position, tileType);
    }

    protected bool IsAnyPopupActive()
    {
        // Check for either popup type (returns null if none exists)
        return FindFirstObjectByType<WinPopup>() != null ||
               FindFirstObjectByType<LosePopup>() != null;
    }

    protected bool IsAnyRocketProjectileActive()
    {
        return FindFirstObjectByType<RocketProjectile>() != null; 
    }
}
