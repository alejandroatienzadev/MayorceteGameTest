using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private BuildingData currentBuilding;
    private GameObject previewObject;

    public bool isBuildingMode = false;

    private static BuildingManager _instance;
    public static BuildingManager Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }else
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
    }

    void MovePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            previewObject.transform.position = hit.point;
        }
    }

    public void Build()
    {
        if (!CanBuild()) return;

        Vector3 buildPosition = previewObject.transform.position;

        SpendResources();

        Instantiate(currentBuilding.prefab, buildPosition, Quaternion.identity);

        Destroy(previewObject);

        isBuildingMode = false;
    }

    bool CanBuild()
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