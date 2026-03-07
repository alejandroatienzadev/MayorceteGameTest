using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;

    private GridCell[,] grid;

    private static GridManager _instance;
    public static GridManager Instance => _instance;

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
            DontDestroyOnLoad(gameObject);
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

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = new Vector3(
                    x * cellSize,
                    0,
                    z * cellSize
                );

                grid[x, z] = new GridCell(worldPos);
            }
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int z = Mathf.FloorToInt(worldPos.z / cellSize);

        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize,
            0,
            gridPos.y * cellSize
        );
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

    public void DebugPreviewCells(Vector2Int origin, Vector2Int size, bool canBuild)
    {
        Color color = canBuild ? Color.green : Color.red;

        Gizmos.color = color;

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                int cellX = origin.x + x;
                int cellZ = origin.y + z;

                if (cellX < 0 || cellX >= gridWidth ||
                    cellZ < 0 || cellZ >= gridHeight)
                    continue;

                Vector3 worldPos = grid[cellX, cellZ].worldPosition;

                Gizmos.DrawCube(
                    worldPos + new Vector3(cellSize / 2f, 0.01f, cellSize / 2f),
                    new Vector3(cellSize * 0.9f, 0.02f, cellSize * 0.9f)
                );
            }
        }
    }
}