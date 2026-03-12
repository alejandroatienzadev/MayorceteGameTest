using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public GridManager gridManager;

    public Material lineMaterial;

    public float lineWidth = 0.03f;

    public bool gridVisible = false;

    void Start()
    {
        DrawGrid();
    }

    void DrawGrid()
    {
        float cellSize = gridManager.cellSize;
        int width = gridManager.gridWidth;
        int height = gridManager.gridHeight;

        float offsetX = width * cellSize / 2f;
        float offsetZ = height * cellSize / 2f;

        Vector3 origin = gridManager.transform.position;

        // líneas verticales
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(
                x * cellSize - offsetX,
                0.02f,
                -offsetZ
            ) + origin;

            Vector3 end = new Vector3(
                x * cellSize - offsetX,
                0.02f,
                height * cellSize - offsetZ
            ) + origin;

            CreateLine(start, end);
        }

        // líneas horizontales
        for (int z = 0; z <= height; z++)
        {
            Vector3 start = new Vector3(
                -offsetX,
                0.02f,
                z * cellSize - offsetZ
            ) + origin;

            Vector3 end = new Vector3(
                width * cellSize - offsetX,
                0.02f,
                z * cellSize - offsetZ
            ) + origin;

            CreateLine(start, end);
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");

        line.transform.parent = transform;

        LineRenderer lr = line.AddComponent<LineRenderer>();

        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.positionCount = 2;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.useWorldSpace = true;
    }

    public void SetGridVisible(bool visible)
    {
        gridVisible = visible;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(visible);
        }
    }
}