using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    [Header("LayerMasks")]
    public LayerMask groundLayer;

    public LayerMask buildingLayer;
    // Variable que se rellenará para construir un edificio
    private BuildingData currentBuilding;

    [SerializeField, Tooltip("Variable para rotar y construir el edificio.")] 
    private GameObject selectedBuild;

    [SerializeField, Tooltip("Contenedor de edificios.")] 
    private Transform buildingsContainer;
    // Posición actual del grid.
    private Vector2Int currentGridPos;
    [Tooltip("Referencia a los controles")]
    public PlayerController controller;
    // Booleana para controlar si se encuentra en modo construcción o no.
    bool _isBuildingMode = false;
    public bool IsBuildingMode => _isBuildingMode;
    // Rotacion que utilizaremos con los edificios.
    private int rotation = 0;
    // Vector de rotacion
    private Vector2Int rotatedSize;
    // Referencia al GridHighlight
    public GridCellHighlight gridHighlight;

    private List<Building> placedBuildings = new List<Building>();

    [SerializeField, Tooltip("Edificio ya construido seleccionado")]
    private Building selectedBuilding;

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
    /// <summary>
    /// Método para seleccionar la selección de edificio.
    /// </summary>
    /// <param name="building"></param>
    public void StartBuilding(BuildingData building)
    {
        CancelBuild();

        currentBuilding = building;
        _isBuildingMode = true;

        gridHighlight.SetVisible(true);

        foreach (var b in placedBuildings)
        {
            if (b != null)
                b.SetPreviewMode(true);
        }

        // usamos el prefab del nivel 0
        GameObject prefab = building.levels[0].modelPrefab;

        selectedBuild = Instantiate(prefab, buildingsContainer);

        Building previewBuilding = selectedBuild.GetComponent<Building>();

        if (previewBuilding != null)
        {
            previewBuilding.data = building;
            previewBuilding.SetPreviewMode(true);
        }

        rotation = 0;
        rotatedSize = building.size;
    }

    /// <summary>
    /// Método para gestionar el movimiento del edificio seleccionado
    /// </summary>
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

            if (previewBuilding != null)
                previewBuilding.SetBuildValid(canBuild);

            gridHighlight.ShowBuildArea(currentGridPos, rotatedSize);
        }
    }

    /// <summary>
    /// Método para gestionar la construcción del edificio.
    /// </summary>
    public void Build()
    {
        if (!GridManager.Instance.CanBuild(currentGridPos, rotatedSize))
            return;

        var level0 = currentBuilding.levels[0];

        if (!HasResources(level0))
            return;

        ResourceManager.Instance.SpendResources(
            level0.woodCost,
            level0.stoneCost,
            level0.goldCost
        );

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
            building.StartConstruction();

            placedBuildings.Add(building);

            building.gridOrigin = currentGridPos;
            building.gridSize = rotatedSize;
        }


        selectedBuild = null;

        ExitBuildMode();
    }

    /// <summary>
    /// Método para cancelar la construcción de un edificio.
    /// </summary>
    public void CancelBuild()
    {
        if (selectedBuild != null)
            Destroy(selectedBuild);

        ExitBuildMode();
    }

    /// <summary>
    /// Método para gestionar la salida del modo construcción
    /// </summary>
    void ExitBuildMode()
    {
        _isBuildingMode = false;

        gridHighlight.SetVisible(false);

        foreach (var b in placedBuildings)
        {
            if (b != null)
                b.SetPreviewMode(false);
        }
    }

    /// <summary>
    /// Bool para comprobar si hay recursos o no.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Método para la rotación de los edificios durante la construcción.
    /// </summary>
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
#endregion

#region Build Selection
    public void SelectBuilding()
    {
        // ignorar clicks en UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buildingLayer))
        {
            Building building = hit.collider.GetComponentInParent<Building>();

            if (building != null)
            {
                SetSelected(building);
            }
        }
        else
        {
            Deselect();
        }
    }

    void SetSelected(Building building)
    {
        if (selectedBuilding != null)
            Deselect();

        selectedBuilding = building;

        Debug.Log("Selected: " + building.data.buildingName);
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
        if (selectedBuilding == null)
            return;

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
        if (!selectedBuilding) return;
        selectedBuilding.Upgrade();
    }
#endregion
}