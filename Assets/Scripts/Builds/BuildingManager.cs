using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    [Header("LayerMasks")]
    public LayerMask groundLayer;
    public LayerMask buildingLayer;

    private BuildingData currentBuilding;

    [SerializeField, Tooltip("Instancia actual de la previsualización.")] 
    private GameObject selectedBuild;

    [SerializeField, Tooltip("Contenedor donde se guardarán los edificios en la jerarquía.")] 
    private Transform buildingsContainer;

    [SerializeField, Tooltip("Plantilla base (Prefab con el script Building, Model vacío y UI).")]
    private GameObject buildingContainerPrefab;

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
        MovePreview();
    }

#region Building Methods

    public void StartBuilding(BuildingData building)
    {
        CancelBuild(); // Limpiamos cualquier selección previa
        currentBuilding = building;
        _isBuildingMode = true;
        gridHighlight.SetVisible(true);

        // Notificamos a los edificios ya colocados que entren en modo transparencia si es necesario
        foreach (var b in placedBuildings)
        {
            if (b != null) b.SetPreviewMode(true);
        }

        // INSTANCIAMOS LA PLANTILLA UNIVERSAL
        selectedBuild = Instantiate(buildingContainerPrefab, buildingsContainer);

        Building previewBuilding = selectedBuild.GetComponent<Building>();
        if (previewBuilding != null)
        {
            // Inicializamos con la data del edificio elegido
            previewBuilding.Initialize(building); 
            previewBuilding.SetPreviewMode(true);
        }

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
            Vector3 offset = new Vector3(
                rotatedSize.x * GridManager.Instance.cellSize / 2f,
                0,
                rotatedSize.y * GridManager.Instance.cellSize / 2f
            );

            selectedBuild.transform.position = worldPos + offset;

            bool canBuild = GridManager.Instance.CanBuild(currentGridPos, rotatedSize);
            Building previewBuilding = selectedBuild.GetComponent<Building>();

            if (previewBuilding != null)
                previewBuilding.SetBuildValid(canBuild);

            gridHighlight.ShowBuildArea(currentGridPos, rotatedSize);
        }
    }

    bool HasResources(BuildingLevel level)
    {
        if (ResourceManager.Instance.currentWood < level.woodCost)
            return false;
        if (ResourceManager.Instance.currentStone < level.stoneCost)
            return false;
        if (ResourceManager.Instance.currentGold < level.goldCost)
            return false;
        return true;
    }

    public void Build()
    {
        if (selectedBuild == null || currentBuilding == null) return;
        if (!GridManager.Instance.CanBuild(currentGridPos, rotatedSize)) return;
        
        // Comprobamos recursos usando el ResourceManager
        if (!HasResources(currentBuilding.levels[0]))
            return;

        // Gastamos recursos
        ResourceManager.Instance.SpendResources(currentBuilding.levels[0].woodCost, currentBuilding.levels[0].stoneCost, currentBuilding.levels[0].goldCost);

        // Posicionamiento final
        GridManager.Instance.PlaceBuilding(currentGridPos, rotatedSize);

        Building building = selectedBuild.GetComponent<Building>();
        if (building != null)
        {
            building.gridOrigin = currentGridPos;
            building.gridSize = rotatedSize;
            building.StartConstruction(); // Inicia el timer de construcción
            placedBuildings.Add(building);
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
        _isBuildingMode = false;
        gridHighlight.SetVisible(false);

        foreach (var b in placedBuildings)
        {
            if (b != null) b.SetPreviewMode(false);
        }
    }

    public void RotateBuilding()
    {
        if (selectedBuild == null) return;

        rotation = (rotation + 90) % 360;
        selectedBuild.transform.rotation = Quaternion.Euler(0, rotation, 0);

        // Intercambiamos X por Y para el tamaño en el grid
        rotatedSize = new Vector2Int(rotatedSize.y, rotatedSize.x);
    }

#endregion

#region Build Selection & Edit

    public void SelectBuilding()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            // Buscamos el componente Building en el objeto golpeado o sus padres
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

    public void ReRotateBuilding()
    {
        if (selectedBuilding == null) return;

        Vector2Int origin = selectedBuilding.gridOrigin;
        Vector2Int size = selectedBuilding.gridSize;

        GridManager.Instance.RemoveBuilding(origin, size);
        Vector2Int newSize = new Vector2Int(size.y, size.x);

        if (!GridManager.Instance.CanBuild(origin, newSize))
        {
            GridManager.Instance.PlaceBuilding(origin, size);
            return;
        }

        selectedBuilding.transform.Rotate(0, 90, 0);
        selectedBuilding.gridSize = newSize;

        Vector3 worldPos = GridManager.Instance.GridToWorld(origin);
        Vector3 offset = new Vector3(
            newSize.x * GridManager.Instance.cellSize / 2f,
            0,
            newSize.y * GridManager.Instance.cellSize / 2f
        );

        selectedBuilding.transform.position = worldPos + offset;
        GridManager.Instance.PlaceBuilding(origin, newSize);
    }

    public void UpgradeBuilding()
    {
        if (selectedBuilding != null)
            selectedBuilding.Upgrade();
    }

#endregion
}