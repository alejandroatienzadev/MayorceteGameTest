using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(5,5);

    void Start()
    {
        RegisterZone();
    }

    void RegisterZone()
    {
        GridManager grid = GridManager.Instance;

        Vector2Int center = grid.WorldToGrid(transform.position);

        Vector2Int origin = grid.GetBuildingOrigin(center, size);

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                int gridX = origin.x + x;
                int gridZ = origin.y + z;

                grid.SetBuildable(gridX, gridZ, true);
            }
        }
    }
}