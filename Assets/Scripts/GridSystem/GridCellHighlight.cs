using UnityEngine;
using System.Collections.Generic;

public class GridCellHighlight : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject cellPrefab;

    [SerializeField] private Material occupiedMat;
    [SerializeField] private Material nonOccupiedMat;

    // Usamos un Diccionario para acceder rápido a la celda visual mediante su posición en el grid
    private Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, Renderer> cellRenderers = new Dictionary<Vector2Int, Renderer>();

    void Start()
    {
        // Ya no creamos nada aquí
        SetVisible(false);
    }

    // Este método lo llamará cada BuildZone
    public void CreateCellsForZone(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int pos = new Vector2Int(origin.x + x, origin.y + z);

                // Si ya existe la celda visual por otra zona solapada, no la creamos
                if (cellObjects.ContainsKey(pos)) continue;

                GameObject cell = Instantiate(cellPrefab, transform);
                cell.transform.position = gridManager.GetCellCenter(pos.x, pos.y);
                cell.transform.localScale = new Vector3(gridManager.cellSize, 0.02f, gridManager.cellSize);

                cellObjects.Add(pos, cell);
                cellRenderers.Add(pos, cell.GetComponentInChildren<Renderer>());
            }
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    // Limpia los colores volviendo al estado original (transparente/suave)
    void ClearColors()
    {
        foreach (var renderer in cellRenderers.Values)
        {
            if (renderer != null)
                renderer.material.color = new Color(1, 1, 1, 0.15f); // Color neutro
        }
    }

    public void ShowBuildArea(Vector2Int origin, Vector2Int size)
    {
        ClearColors();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int pos = new Vector2Int(origin.x + x, origin.y + z);

                if (cellRenderers.ContainsKey(pos))
                {
                    bool buildable = gridManager.IsCellBuildable(pos.x, pos.y);
                    cellRenderers[pos].material.color = buildable ? Color.blue : Color.red;
                }
            }
        }
    }
}