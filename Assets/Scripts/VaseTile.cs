using UnityEngine;

public class VaseTile : Tile
{

    public bool damaged = false;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    protected override void OnMouseDown()
    {
        return;
    }


    
}
