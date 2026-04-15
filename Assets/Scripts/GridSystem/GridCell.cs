using UnityEngine;

public class GridCell
{
    public bool buildable = true;
    public bool occupied = false;

    public Vector3 worldPosition;
    public Building placedBuilding;

    public GridCell(Vector3 pos)
    {
        worldPosition = pos;
    }
}