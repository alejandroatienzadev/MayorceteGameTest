using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    [Header("LayerMasks")]
    public LayerMask groundLayer;
    public LayerMask buildingLayer;
    public LayerMask towerLayer;

    private BuildingData currentBuilding;

    [SerializeField] private GameObject selectedBuild;
    [SerializeField] private Transform buildingsContainer;
    [SerializeField] private GameObject buildingContainerPrefab;

    private Vector2Int currentGridPos;
    public PlayerController controller;
    
    private bool _buildingMode = false;
    public bool BuildinMode => _buildingMode;

    private bool _isBuilding = false;
    public bool IsBuilding => _isBuilding;

    private int rotation = 0;
    private Vector2Int rotatedSize;
    
    public GridCellHighlight gridHighlight;
    private List<Building> placedBuildings = new List<Building>();

    [SerializeField] private Building selectedBuilding;

    private List<GameObject> wallPreviews = new List<GameObject>();
    private Vector2Int lastMouseGridPos;

    private float lastBuildTime; 
    private const float buildCooldown = 0.25f; 

    private static BuildingManager _instance;
    public static BuildingManager Instance => _instance;

    public enum BuildMode { Normal, Wall }
    private BuildMode currentMode = BuildMode.Normal;

    [Header("Wall System")]
    [SerializeField] private BuildingData wallTowerData;
    [SerializeField] private BuildingData wallSegmentData;
    private Vector2Int? wallStartPos = null;

#region Unity Methods
    void Awake()
    {
        if (_instance == null) { _instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (!_isBuilding) return;
        if (currentMode == BuildMode.Normal) MovePreview();
        else if (currentMode == BuildMode.Wall) HandleWallPreview();
    }
#endregion

#region Building Methods

    public void StartBuilding(BuildingData building)
    {
        CancelBuild();
        currentMode = BuildMode.Normal;
        currentBuilding = building;
        _isBuilding = true;
        gridHighlight.SetVisible(true);
        foreach (var b in placedBuildings) if (b != null) b.SetPreviewMode(true);
        selectedBuild = Instantiate(buildingContainerPrefab, buildingsContainer);
        Building previewBuilding = selectedBuild.GetComponent<Building>();
        previewBuilding.Initialize(building);
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
            currentGridPos = GridManager.Instance.GetBuildingOrigin(center, rotatedSize);
            Vector3 worldPos = GridManager.Instance.GridToWorld(currentGridPos);
            Vector3 offset = new Vector3(rotatedSize.x * GridManager.Instance.cellSize / 2f, 0, rotatedSize.y * GridManager.Instance.cellSize / 2f);
            selectedBuild.transform.position = worldPos + offset;
            bool canBuild = GridManager.Instance.CanBuild(currentGridPos, rotatedSize);
            selectedBuild.SetActive(true); 
            selectedBuild.GetComponent<Building>().SetBuildValid(canBuild);
            gridHighlight.ShowBuildArea(currentGridPos, rotatedSize);
        }
    }
    public void Build()
    {
        if (Time.time - lastBuildTime < buildCooldown) return;
        lastBuildTime = Time.time;

        if (currentMode == BuildMode.Wall) { HandleWallClick(); return; }

        if (selectedBuild == null || currentBuilding == null) return;
        if (!GridManager.Instance.CanBuild(currentGridPos, rotatedSize)) { CancelBuild(); return; }

        GridManager.Instance.PlaceBuilding(currentGridPos, rotatedSize);
        Building building = selectedBuild.GetComponent<Building>();
        building.gridOrigin = currentGridPos;
        building.gridSize = rotatedSize;
        building.StartConstruction();
        
        // Al construir, nos aseguramos de que el objeto final sea seleccionable
        SetupBuildingCollider(selectedBuild);

        placedBuildings.Add(building);
        selectedBuild = null;
        ExitBuildMode();
    }

    public void CancelBuild()
    {
        ClearWallPreviews();
        if (selectedBuild != null) Destroy(selectedBuild);
        wallStartPos = null;
        ExitBuildMode();
    }
    void ExitBuildMode()
    {
        _isBuilding = false;
        gridHighlight.SetVisible(false);
        ClearWallPreviews(); 
        foreach (var b in placedBuildings) if (b != null) b.SetPreviewMode(false);
    }

    public void RotateBuilding()
    {
        if (currentMode != BuildMode.Normal || selectedBuild == null) return;
        rotation = (rotation + 90) % 360;
        selectedBuild.transform.rotation = Quaternion.Euler(0, rotation, 0);
        rotatedSize = new Vector2Int(rotatedSize.y, rotatedSize.x);
    }

#endregion

#region Wall Building Methods
    public void StartWallBuilding()
    {
        CancelBuild();
        currentMode = BuildMode.Wall;
        _isBuilding = true;
        gridHighlight.SetVisible(true);
        wallStartPos = null;
    }

    void HandleWallPreview()
    {
         Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer | buildingLayer))
        {
            Vector2Int hoveredGrid;
            Building existingBuilding = hit.collider.GetComponentInParent<Building>();
            
            if (existingBuilding != null && existingBuilding.data == wallTowerData)
                hoveredGrid = existingBuilding.gridOrigin;
            else
                hoveredGrid = GridManager.Instance.WorldToGrid(hit.point);

            if (hoveredGrid != lastMouseGridPos)
            {
                lastMouseGridPos = hoveredGrid;
                ClearWallPreviews();
                if (wallStartPos == null) ShowPreviewSingle(wallTowerData, hoveredGrid, Vector2Int.up);
                else ShowWallPreviewLine(wallStartPos.Value, hoveredGrid);
            }
        }
    }

    void HandleWallClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer | buildingLayer)) return;

        Vector2Int gridPos;
        Building existingBuilding = hit.collider.GetComponentInParent<Building>();

        if (existingBuilding != null && existingBuilding.data == wallTowerData)
            gridPos = existingBuilding.gridOrigin;
        else
            gridPos = GridManager.Instance.WorldToGrid(hit.point);

        if (wallStartPos == null)
        {
            wallStartPos = gridPos;
        }
        else
        {
            if (gridPos == wallStartPos.Value) return; 
            
            BuildWall(wallStartPos.Value, gridPos);
            wallStartPos = null;
            ExitBuildMode();
        }
    }

    void BuildWall(Vector2Int start, Vector2Int end)
    {
        ClearWallPreviews(); 

        TryPlaceTower(start, Vector2Int.zero);
        TryPlaceTower(end, Vector2Int.zero);

        List<Vector2Int> path = GetPath(start, end);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int currentCell = path[i];
            Vector2Int nextCell = (i < path.Count - 1) ? path[i + 1] : end;
            Vector2Int dir = nextCell - currentCell;

            PlaceSingle(wallSegmentData, currentCell, dir);
        }
    }
    
    void TryPlaceTower(Vector2Int origin, Vector2Int direction)
    {
        Vector2Int centerCell = origin + new Vector2Int(1, 1);
        if (!GridManager.Instance.GetCell(centerCell.x, centerCell.y).occupied)
        {
            PlaceSingle(wallTowerData, origin, direction);
        }
    }

    void PlaceSingle(BuildingData data, Vector2Int pos, Vector2Int direction)
    {
        bool isTower = data == wallTowerData;
        bool shouldRotate = !isTower && direction.x != 0;
        
        Vector2Int size = shouldRotate ? new Vector2Int(data.size.y, data.size.x) : data.size;
        Vector2Int origin = pos;

        if (!GridManager.Instance.CanBuild(origin, size)) return;

        GameObject obj = Instantiate(buildingContainerPrefab, buildingsContainer);
        Building b = obj.GetComponent<Building>();
        b.Initialize(data);
        b.gridOrigin = origin;
        b.gridSize = size;

        float cellSize = GridManager.Instance.cellSize;
        Vector3 worldPos = GridManager.Instance.GridToWorld(origin);
        Vector3 centerOffset = new Vector3(size.x * cellSize / 2f, 0, size.y * cellSize / 2f);
        
        obj.transform.position = worldPos + centerOffset;
        obj.transform.rotation = shouldRotate ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

        // --- NUEVO: Configurar Collider para el objeto final ---
        SetupBuildingCollider(obj);

        GridManager.Instance.PlaceBuilding(origin, size);
        b.StartConstruction();
        placedBuildings.Add(b);
    }
#endregion

#region EditMode Methods
    public void SelectBuilding()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            Building building = hit.collider.GetComponentInParent<Building>();
            if (building != null) 
            {
                SetSelected(building);
            }
        }else if(Physics.Raycast(ray, out RaycastHit hit2, 1000f, towerLayer))
        {
            Building building = hit2.collider.GetComponentInParent<Building>();
            if (building != null) 
            {
                StartWallBuilding();
                wallStartPos = building.gridOrigin;
            }
        }
        else
        {
            Deselect();
        }
    }

    public void ReRotateBuilding()
    {
        if (selectedBuilding == null || selectedBuilding.data.name == "Wall" || selectedBuilding.data.name == "Tower") return;
        Vector2Int origin = selectedBuilding.gridOrigin;
        Vector2Int size = selectedBuilding.gridSize;
        GridManager.Instance.RemoveBuilding(origin, size);
        Vector2Int newSize = new Vector2Int(size.y, size.x);
        if (!GridManager.Instance.CanBuild(origin, newSize)) { GridManager.Instance.PlaceBuilding(origin, size); return; }

        selectedBuilding.transform.Rotate(0, 90, 0);
        selectedBuilding.gridSize = newSize;
        Vector3 worldPos = GridManager.Instance.GridToWorld(origin);
        Vector3 offset = new Vector3(newSize.x * GridManager.Instance.cellSize / 2f, 0, newSize.y * GridManager.Instance.cellSize / 2f);
        selectedBuilding.transform.position = worldPos + offset;
        GridManager.Instance.PlaceBuilding(origin, newSize);
    }

    void SetSelected(Building building)
    {
        if (selectedBuilding != null) Deselect();
        ToggleBuildingMode(false);
        selectedBuilding = building;
        UIManager.Instance.EnableEditMode();
    }

    void Deselect() 
    { 
        selectedBuilding = null; 
        if (!_buildingMode)
        {
            UIManager.Instance.DisableEditMode();
        }
    }

    public void UpgradeBuilding()
    {
        selectedBuilding.Upgrade();
    }
#endregion

#region Auxiliar Methods
    private List<Vector2Int> GetPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;

        int stepX = System.Math.Sign(end.x - start.x);
        int stepY = System.Math.Sign(end.y - start.y);

        while (current != end)
        {
            int distX = Mathf.Abs(end.x - current.x);
            int distY = Mathf.Abs(end.y - current.y);

            if (distX >= distY && distX > 0)
            {
                current.x += stepX;
            }
            else if (distY > 0)
            {
                current.y += stepY;
            }

            if (current != end)
            {
                path.Add(current);
            }
        }

        return path;
    }

    private void SetupBuildingCollider(GameObject buildingContainer)
    {
        Transform modelTransform = buildingContainer.transform.Find("Model");
        if (modelTransform != null && modelTransform.childCount > 0)
        {
            GameObject visualModel = modelTransform.GetChild(0).gameObject;
            
            // CORRECCIÓN: Convertimos el LayerMask (bitmask) al índice de capa (0-31)
            int layerIndex = (int)Mathf.Log(buildingLayer.value, 2);
            visualModel.layer = layerIndex;
            
            if (visualModel.GetComponent<Collider>() == null)
            {
                visualModel.AddComponent<BoxCollider>();
            }
        }
    }

    public void ToggleBuildingMode(bool value)
    {
        _buildingMode = value;
    }

    void ShowWallPreviewLine(Vector2Int start, Vector2Int end)
    {
        ShowPreviewSingle(wallTowerData, start, Vector2Int.up);
        ShowPreviewSingle(wallTowerData, end, Vector2Int.up);

        List<Vector2Int> path = GetPath(start, end);

        foreach (Vector2Int cell in path)
        {
            ShowPreviewSingle(wallSegmentData, cell, Vector2Int.up); 
        }
    }

    void ShowPreviewSingle(BuildingData data, Vector2Int pos, Vector2Int direction)
    {
        bool isWall = data == wallSegmentData;
        bool shouldRotate = isWall && direction.x != 0;
        Vector2Int size = shouldRotate ? new Vector2Int(data.size.y, data.size.x) : data.size;
        Vector2Int origin = pos;

        GameObject preview = Instantiate(buildingContainerPrefab, buildingsContainer);
        wallPreviews.Add(preview);
        Building b = preview.GetComponent<Building>();
        b.Initialize(data);
        b.SetPreviewMode(true);

        // Desactivamos colliders en el preview para que no interfieran con el Raycast del ratón
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        float cellSize = GridManager.Instance.cellSize;
        Vector3 worldPos = GridManager.Instance.GridToWorld(origin); 
        Vector3 centerOffset = new Vector3(size.x * cellSize / 2f, 0, size.y * cellSize / 2f);
        
        preview.transform.position = worldPos + centerOffset;
        preview.transform.rotation = shouldRotate ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        b.SetBuildValid(GridManager.Instance.CanBuild(origin, size));
    }

    void ClearWallPreviews()
    {
        for (int i = wallPreviews.Count - 1; i >= 0; i--) if (wallPreviews[i] != null) Destroy(wallPreviews[i]);
        wallPreviews.Clear();
    }
#endregion
}