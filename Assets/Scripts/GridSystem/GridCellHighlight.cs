using UnityEngine;

public class GridCellHighlight : MonoBehaviour
{
    public GridManager gridManager;

    public GameObject cellPrefab;

    [SerializeField] private Material occupiedMat;
    [SerializeField] private Material nonOccupiedMat;

    GameObject[,] cells;

    Renderer[,] renderers;

    void Start()
    {
        CreateCells();

        // ocultar al iniciar
        SetVisible(false);
    }

    void CreateCells()
    {
        int width = gridManager.gridWidth;
        int height = gridManager.gridHeight;

        cells = new GameObject[width, height];
        renderers = new Renderer[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);

                cell.transform.position = gridManager.GetCellCenter(x, z);

                cell.transform.localScale =
                    new Vector3(gridManager.cellSize, 0.02f, gridManager.cellSize);

                cells[x, z] = cell;
                renderers[x, z] = cell.GetComponentInChildren<Renderer>();
            }
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        UpdateCellColors();
    }

    void UpdateCellColors()
    {
        int width = gridManager.gridWidth;
        int height = gridManager.gridHeight;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = gridManager.GetCell(x, z);

                if (cell == null) continue;

                Renderer r = renderers[x, z];

                if (!cell.buildable)
                {
                    r.enabled = false;
                    continue;
                }

                r.enabled = true;

                if (cell.occupied)
                    r.material = occupiedMat;
                else
                    r.material = nonOccupiedMat;
            }
        }
    }
}