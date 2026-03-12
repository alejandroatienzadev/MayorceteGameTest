using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    private BuildingData currentBuilding;

    [SerializeField] private GameObject previewObject;

    [SerializeField] private Transform buildingsContainer;

    private Vector2Int currentGridPos;

    public PlayerController controller;
    public LayerMask groundLayer;

    public bool isBuildingMode = false;

    private int rotation = 0;
    private Vector2Int rotatedSize;

    public GridCellHighlight gridHighlight;

    private List<Building> placedBuildings = new List<Building>();

    private static BuildingManager _instance;
    public static BuildingManager Instance => _instance;

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
    }

    void Update()
    {
        if (!isBuildingMode) return;

        MovePreview();
    }

    public void StartBuilding(BuildingData building)
    {
        currentBuilding = building;
        isBuildingMode = true;

        gridHighlight.SetVisible(true);

        // edificios existentes → modo preview
        foreach (var b in placedBuildings)
        {
            if (b != null)
                b.SetPreviewMode(true);
        }

        previewObject = Instantiate(building.prefab, buildingsContainer);

        // activar preview en el edificio nuevo
        Building previewBuilding = previewObject.GetComponent<Building>();

        if (previewBuilding != null)
            previewBuilding.SetPreviewMode(true);

        rotation = 0;
        rotatedSize = building.size;
    }

    void MovePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector2Int center = GridManager.Instance.WorldToGrid(hit.point);

            currentGridPos = GridManager.Instance.GetBuildingOrigin(
                center,
                rotatedSize
            );

            Vector3 worldPos = GridManager.Instance.GridToWorld(currentGridPos);

            Vector3 offset = new Vector3(
                rotatedSize.x * GridManager.Instance.cellSize / 2f,
                0,
                rotatedSize.y * GridManager.Instance.cellSize / 2f
            );

            previewObject.transform.position = worldPos + offset;

            bool canBuild = GridManager.Instance.CanBuild(
                currentGridPos,
                rotatedSize
            );

            Building previewBuilding = previewObject.GetComponent<Building>();

            if (previewBuilding != null)
            {
                previewBuilding.SetBuildValid(canBuild);
            }

            GridManager.Instance.DrawBuildPreview(
                currentGridPos,
                rotatedSize
            );
        }
    }

    public void Build()
    {
        if (!GridManager.Instance.CanBuild(currentGridPos, rotatedSize))
            return;

        if (!HasResources())
            return;

        SpendResources();

        Vector3 worldPos = GridManager.Instance.GridToWorld(currentGridPos);

        Vector3 offset = new Vector3(
            rotatedSize.x * GridManager.Instance.cellSize / 2f,
            0,
            rotatedSize.y * GridManager.Instance.cellSize / 2f
        );

        Vector3 buildPosition = worldPos + offset;

        previewObject.transform.position = buildPosition;

        GridManager.Instance.PlaceBuilding(currentGridPos, rotatedSize);

        Building building = previewObject.GetComponent<Building>();

        if (building != null)
        {
            building.SetPreviewMode(false);
            placedBuildings.Add(building);
        }

        previewObject = null;

        ExitBuildMode();
    }

    public void CancelBuild()
    {
        if (previewObject != null)
            Destroy(previewObject);

        ExitBuildMode();
    }

    void ExitBuildMode()
    {
        isBuildingMode = false;

        gridHighlight.SetVisible(false);

        foreach (var b in placedBuildings)
        {
            if (b != null)
                b.SetPreviewMode(false);
        }
    }

    bool HasResources()
    {
        if (ResourceManager.Instance.currentWood < currentBuilding.woodCost)
            return false;

        if (ResourceManager.Instance.currentStone < currentBuilding.stoneCost)
            return false;

        return true;
    }

    void SpendResources()
    {
        ResourceManager.Instance.currentWood -= currentBuilding.woodCost;
        ResourceManager.Instance.currentStone -= currentBuilding.stoneCost;
    }

    public void RotateBuilding()
    {
        rotation += 90;

        if (rotation >= 360)
            rotation = 0;

        if (previewObject != null)
            previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);

        rotatedSize = new Vector2Int(
            rotatedSize.y,
            rotatedSize.x
        );
    }
}