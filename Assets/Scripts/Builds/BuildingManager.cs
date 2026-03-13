using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class BuildingManager : MonoBehaviour
{
    private BuildingData currentBuilding;

    [SerializeField] private GameObject selectedBuild;

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
        CancelBuild();
        currentBuilding = building;
        isBuildingMode = true;

        gridHighlight.SetVisible(true);

        foreach (var b in placedBuildings)
        {
            if (b != null)
                b.SetPreviewMode(true);
        }

        selectedBuild = Instantiate(building.prefab, buildingsContainer);

        // activar preview en el edificio nuevo
        Building previewBuilding = selectedBuild.GetComponent<Building>();

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

            selectedBuild.transform.position = worldPos + offset;

            bool canBuild = GridManager.Instance.CanBuild(
                currentGridPos,
                rotatedSize
            );

            Building previewBuilding = selectedBuild.GetComponent<Building>();

            if (previewBuilding != null) previewBuilding.SetBuildValid(canBuild);

            gridHighlight.ShowBuildArea(currentGridPos, rotatedSize);
        }
    }

    public void Build()
    {
        if (!GridManager.Instance.CanBuild(currentGridPos, rotatedSize))
            return;

        if (!HasResources())
            return;

        ResourceManager.Instance.SpendResources(currentBuilding.woodCost, currentBuilding.stoneCost, currentBuilding.golCost);

        Vector3 worldPos = GridManager.Instance.GridToWorld(currentGridPos);

        Vector3 offset = new Vector3(
            rotatedSize.x * GridManager.Instance.cellSize / 2f,
            0,
            rotatedSize.y * GridManager.Instance.cellSize / 2f
        );

        Vector3 buildPosition = worldPos + offset;

        selectedBuild.transform.position = buildPosition;

        GridManager.Instance.PlaceBuilding(currentGridPos, rotatedSize);

        Building building = selectedBuild.GetComponent<Building>();

        if (building != null)
        {
            building.SetPreviewMode(false);
            placedBuildings.Add(building);
            building.isBuilded = true;
        }

        selectedBuild = null;

        ExitBuildMode();
    }

    public void CancelBuild()
    {
        if (selectedBuild != null)
            Destroy(selectedBuild);

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

    public void RotateBuilding()
    {
        rotation += 90;

        if (rotation >= 360)
            rotation = 0;

        if (selectedBuild != null)
            selectedBuild.transform.rotation = Quaternion.Euler(0, rotation, 0);

        rotatedSize = new Vector2Int(
            rotatedSize.y,
            rotatedSize.x
        );
    }
}