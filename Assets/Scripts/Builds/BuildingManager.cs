using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    [Header("LayerMasks")]
    public LayerMask groundLayer;
    public LayerMask buildingLayer;

    private BuildingData currentBuilding;

    [SerializeField] private GameObject selectedBuild;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private GameObject buildingContainerPrefab;

    private Vector2Int currentGridPos;
    public PlayerController controller;
    
    private bool _isBuildingMode = false;
    public bool IsBuildingMode => _isBuildingMode;

    private int rotation = 0;
    private Vector2Int rotatedSize;
    
    public GridCellHighlight gridHighlight;
    private List<Building> placedBuildings = new List<Building>();

    [SerializeField] private Building selectedBuilding;

    private static BuildingManager _instance;
    public static BuildingManager Instance => _instance;

    public enum BuildMode
    {
        Normal,
        Wall
    }

    private BuildMode currentMode = BuildMode.Normal;

    [Header("Wall System")]
    [SerializeField] private BuildingData wallTowerData;
    [SerializeField] private BuildingData wallSegmentData;

    private Vector2Int? wallStartPos = null;

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
        if (!_isBuildingMode) return;

        if (currentMode == BuildMode.Normal)
        {
            MovePreview();
        }
        else if (currentMode == BuildMode.Wall)
        {
            HandleWallPreview();
        }
    }

    public void StartBuilding(BuildingData building)
    {
        CancelBuild();

        currentMode = BuildMode.Normal;

        currentBuilding = building;
        _isBuildingMode = true;
        gridHighlight.SetVisible(true);

        foreach (var b in placedBuildings)
            if (b != null) b.SetPreviewMode(true);

        selectedBuild = Instantiate(buildingContainerPrefab, buildingsContainer);

        Building previewBuilding = selectedBuild.GetComponent<Building>();
        previewBuilding.Initialize(building);
        previewBuilding.SetPreviewMode(true);

        rotation = 0;
        rotatedSize = building.size;
    }

    public void StartWallBuilding()
    {
        CancelBuild();

        currentMode = BuildMode.Wall;

        _isBuildingMode = true;
        gridHighlight.SetVisible(true);

        wallStartPos = null;
    }

    void MovePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector2Int center = GridManager.Instance.WorldToGrid(hit.point);
            currentGridPos = GridManager.Instance.GetBuildingOrigin(center, rotatedSize);

            Vector3 worldPos = GridManager.Instance.GridToWorld(currentGridPos);
            Vector3 offset = new Vector3(
                rotatedSize.x * GridManager.Instance.cellSize / 2f,
                0,
                rotatedSize.y * GridManager.Instance.cellSize / 2f
            );

            selectedBuild.transform.position = worldPos + offset;

            bool canBuild = GridManager.Instance.CanBuild(currentGridPos, rotatedSize);

            selectedBuild.GetComponent<Building>().SetBuildValid(canBuild);

            gridHighlight.ShowBuildArea(currentGridPos, rotatedSize);
        }
    }

    void HandleWallPreview()
    {
        
    }

    public void Build()
    {
        if (currentMode == BuildMode.Wall)
        {
            HandleWallClick();
            return;
        }

        // ===== NORMAL =====
        if (selectedBuild == null || currentBuilding == null) return;
        if (!GridManager.Instance.CanBuild(currentGridPos, rotatedSize)) return;

        GridManager.Instance.PlaceBuilding(currentGridPos, rotatedSize);

        Building building = selectedBuild.GetComponent<Building>();

        building.gridOrigin = currentGridPos;
        building.gridSize = rotatedSize;
        building.StartConstruction();

        placedBuildings.Add(building);

        selectedBuild = null;
        ExitBuildMode();
    }

    void HandleWallClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            return;

        Vector2Int gridPos = GridManager.Instance.WorldToGrid(hit.point);

        if (wallStartPos == null)
        {
            wallStartPos = gridPos;
        }
        else
        {
            BuildWall(wallStartPos.Value, gridPos);
            wallStartPos = null;
            ExitBuildMode();
        }
    }

    void BuildWall(Vector2Int start, Vector2Int end)
    {
        Vector2Int diff = end - start;
        bool horizontal = Mathf.Abs(diff.x) > Mathf.Abs(diff.y);
        
        Vector2Int step = horizontal ? 
            new Vector2Int(System.Math.Sign(diff.x), 0) : 
            new Vector2Int(0, System.Math.Sign(diff.y));

        int length = horizontal ? Mathf.Abs(diff.x) : Mathf.Abs(diff.y);

        for (int i = 0; i <= length; i++)
        {
            Vector2Int pos = start + (step * i);
            bool isEdge = (i == 0 || i == length);
            
            PlaceSingle(isEdge ? wallTowerData : wallSegmentData, pos, step);
        }
    }

    void PlaceSingle(BuildingData data, Vector2Int pos, Vector2Int direction)
    {
        bool rotate = direction.x != 0;
        
        Vector2Int finalSize = rotate ? new Vector2Int(data.size.y, data.size.x) : data.size;

        Vector2Int origin = GridManager.Instance.GetBuildingOrigin(pos, finalSize);

        if (!GridManager.Instance.CanBuild(origin, finalSize)) return;

        GameObject obj = Instantiate(buildingContainerPrefab, buildingsContainer);
        Building b = obj.GetComponent<Building>();
        b.Initialize(data);
        b.gridOrigin = origin;
        b.gridSize = finalSize;

        Vector3 worldPos = GridManager.Instance.GridToWorld(origin);
        float cellSize = GridManager.Instance.cellSize;
        Vector3 offset = new Vector3(finalSize.x * cellSize / 2f, 0, finalSize.y * cellSize / 2f);
        
        obj.transform.position = worldPos + offset;
        obj.transform.rotation = rotate ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

        GridManager.Instance.PlaceBuilding(origin, finalSize);
        b.StartConstruction();
        placedBuildings.Add(b);
    }

    public void CancelBuild()
    {
        if (selectedBuild != null)
            Destroy(selectedBuild);

        wallStartPos = null;

        ExitBuildMode();
    }

    void ExitBuildMode()
    {
        _isBuildingMode = false;
        gridHighlight.SetVisible(false);

        foreach (var b in placedBuildings)
            if (b != null) b.SetPreviewMode(false);
    }

    public void RotateBuilding()
    {
        if (currentMode != BuildMode.Normal) return;
        if (selectedBuild == null) return;

        rotation = (rotation + 90) % 360;
        selectedBuild.transform.rotation = Quaternion.Euler(0, rotation, 0);

        rotatedSize = new Vector2Int(rotatedSize.y, rotatedSize.x);
    }

    public void SelectBuilding()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            Building building = hit.collider.GetComponentInParent<Building>();
            if (building != null) SetSelected(building);
        }
        else
        {
            Deselect();
        }
    }

    void SetSelected(Building building)
    {
        if (selectedBuilding != null) Deselect();
        selectedBuilding = building;
        UIManager.Instance.EnableEditMode();
    }

    void Deselect()
    {
        if (!selectedBuilding) return;
        selectedBuilding = null;
        UIManager.Instance.DisableEditMode();
    }
}