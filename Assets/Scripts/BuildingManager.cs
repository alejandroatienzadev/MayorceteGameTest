using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private BuildingData currentBuilding;

    [SerializeField] private GameObject previewObject;
    private Renderer previewRenderer;

    private Vector2Int currentGridPos; // posición en el grid

    public PlayerController controller;

    public LayerMask groundLayer;

    public bool isBuildingMode = false;

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

        previewObject = Instantiate(building.previewPrefab);

        previewRenderer = previewObject.GetComponentInChildren<Renderer>();
    }

    void MovePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(controller.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector2Int centerCell = GridManager.Instance.WorldToGrid(hit.point);

            // convertir centro → origen del edificio
            Vector2Int origin = new Vector2Int(
                centerCell.x - currentBuilding.size.x / 2,
                centerCell.y - currentBuilding.size.y / 2
            );

            currentGridPos = origin;

            Vector3 worldPos = GridManager.Instance.GridToWorld(origin);

            Vector3 offset = new Vector3(
                currentBuilding.size.x * GridManager.Instance.cellSize / 2f,
                0,
                currentBuilding.size.y * GridManager.Instance.cellSize / 2f
            );

            previewObject.transform.position = worldPos + offset;

            bool canBuild = GridManager.Instance.CanBuild(
                currentGridPos,
                currentBuilding.size
            );

            UpdatePreviewColor(canBuild);
        }
    }

    void UpdatePreviewColor(bool canBuild)
    {
        if (previewRenderer == null) return;

        previewRenderer.material.color = canBuild ? Color.green : Color.red;
    }

    public void Build()
    {
        if (!GridManager.Instance.CanBuild(currentGridPos, currentBuilding.size))
            return;

        if (!HasResources())
            return;

        SpendResources();

        Vector3 worldPos = GridManager.Instance.GridToWorld(currentGridPos);

        Vector3 offset = new Vector3(
            currentBuilding.size.x * GridManager.Instance.cellSize / 2f,
            0,
            currentBuilding.size.y * GridManager.Instance.cellSize / 2f
        );

        Vector3 buildPosition = worldPos + offset;

        Instantiate(currentBuilding.prefab, buildPosition, Quaternion.identity);

        GridManager.Instance.PlaceBuilding(currentGridPos, currentBuilding.size);

        Destroy(previewObject);

        isBuildingMode = false;
    }

    public void CancelBuild()
    {
        if (previewObject != null)
            Destroy(previewObject);

        isBuildingMode = false;
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
}