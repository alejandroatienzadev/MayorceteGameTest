using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance;
    public static GridManager Instance => _instance;


    public int gridWidth = 50;
    public int gridHeight = 50;

    public float cellSize = 1f;

    private GridCell[,] grid;

    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                GridCell cell = grid[x, z];

                if (cell.occupied)
                    Gizmos.color = Color.red;
                else if (cell.buildable)
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.gray;

                Gizmos.DrawWireCube(
                    cell.worldPosition + new Vector3(cellSize / 2f, 0, cellSize / 2f),
                    new Vector3(cellSize, 0.05f, cellSize)
                );
            }
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new GridCell[gridWidth, gridHeight];

        float offsetX = gridWidth * cellSize / 2f;
        float offsetZ = gridHeight * cellSize / 2f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = new Vector3(
                    x * cellSize - offsetX,
                    0,
                    z * cellSize - offsetZ
                ) + transform.position;

                grid[x, z] = new GridCell(worldPos)
                {
                    buildable = false
                };
            }
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float offsetX = gridWidth * cellSize / 2f;
        float offsetZ = gridHeight * cellSize / 2f;

        Vector3 local = worldPos - transform.position;

        int x = Mathf.FloorToInt((local.x + offsetX) / cellSize);
        int z = Mathf.FloorToInt((local.z + offsetZ) / cellSize);

        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float offsetX = gridWidth * cellSize / 2f;
        float offsetZ = gridHeight * cellSize / 2f;

        return new Vector3(
            gridPos.x * cellSize - offsetX,
            0,
            gridPos.y * cellSize - offsetZ
        ) + transform.position;
    }

    public Vector3 GetCellCenter(int x, int z)
    {
        if (x < 0 || x >= gridWidth ||
            z < 0 || z >= gridHeight)
            return Vector3.zero;

        return grid[x, z].worldPosition + new Vector3(
            cellSize / 2f,
            0.01f,
            cellSize / 2f
        );
    }

    public Vector2Int GetBuildingOrigin(Vector2Int center, Vector2Int size)
    {
        int originX = center.x - size.x / 2;
        int originZ = center.y - size.y / 2;

        return new Vector2Int(originX, originZ);
    }

    public bool CanBuild(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                int checkX = origin.x + x;
                int checkZ = origin.y + z;

                if (checkX < 0 || checkX >= gridWidth ||
                    checkZ < 0 || checkZ >= gridHeight)
                    return false;

                GridCell cell = grid[checkX, checkZ];

                if (!cell.buildable || cell.occupied)
                    return false;
            }
        }

        return true;
    }

    public void PlaceBuilding(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                int cellX = origin.x + x;
                int cellZ = origin.y + z;

                grid[cellX, cellZ].occupied = true;
            }
        }
    }

    public void SetBuildable(int x, int z, bool value)
    {
        if (x < 0 || x >= gridWidth ||
            z < 0 || z >= gridHeight)
            return;

        grid[x, z].buildable = value;
    }

    public GridCell GetCell(int x, int z)
    {
        if (x < 0 || x >= gridWidth ||
            z < 0 || z >= gridHeight)
            return null;

        return grid[x, z];
    }

    public bool IsCellBuildable(int x, int z)
    {
        if (x < 0 || x >= gridWidth ||
            z < 0 || z >= gridHeight)
            return false;

        GridCell cell = grid[x, z];

        return cell.buildable && !cell.occupied;
    }

    public void DrawBuildPreview(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                int cellX = origin.x + x;
                int cellZ = origin.y + z;

                Vector3 pos = GetCellCenter(cellX, cellZ);

                bool valid = IsCellBuildable(cellX, cellZ);

                Color color = valid ? Color.green : Color.red;

                Debug.DrawLine(pos, pos + Vector3.up * 0.5f, color, 0f);
            }
        }
    }

    public void RemoveBuilding(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                grid[origin.x + x, origin.y + z].occupied = false;
            }
        }
    }
}