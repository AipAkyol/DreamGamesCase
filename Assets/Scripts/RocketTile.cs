using UnityEngine;

public class RocketTile : Tile
{

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    protected override void OnMouseDown()
    {

        if (IsAnyPopupActive() || IsAnyRocketProjectileActive()) return;

        ActivateRocket();
    }


    public void ActivateRocket()
    {
        gridManager.BlastRocket(transform.position, tileType);
    }
}
