using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;

    [SerializeField] private Renderer[] renderers;

    public bool isBuilded;

    [Header("DEBUG Level Data")]
    [SerializeField] private float productionTime;
    [SerializeField] private int productionAmount;
    [SerializeField] private Transform visualRoot;

    [Header("Materials")]
    public Material normalMaterial;
    public Material previewMaterial;

    private MaterialPropertyBlock propBlock;

    private float counter;

    private int currentLevel = 0;
    public int buildingLevel;

    public BuildingLevel CurrentLevelData
    {
        get { return data.levels[currentLevel]; }
    }

    void Awake()
    {
        GetRenderers();

        propBlock = new MaterialPropertyBlock();

        currentLevel = 0;
        buildingLevel = CurrentLevelData.level;

        ApplyLevelStats();

        counter = productionTime;
    }

    void Update()
    {
        if (!isBuilded)
            return;

        counter -= Time.deltaTime;

        if (counter <= 0)
        {
            GenerateResources();
            counter = productionTime;
        }
    }

    void GenerateResources()
    {
        ResourceManager.Instance.AddResource(
            data.resourceProduced,
            productionAmount
        );
    }

#region Upgrade System

    [ContextMenu("Upgrade")]
    public void Upgrade()
    {
        if (currentLevel >= data.levels.Length - 1)
            return;

        BuildingLevel nextLevel = data.levels[currentLevel + 1];

        if (!HasResources(nextLevel))
            return;

        SpendResources(nextLevel);

        currentLevel++;

        ApplyLevelStats();

        ApplyLevelVisual();

        counter = productionTime;

        buildingLevel = CurrentLevelData.level;

    }

    void ApplyLevelStats()
    {
        productionAmount = CurrentLevelData.productionAmount;
        productionTime = CurrentLevelData.productionTime;
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

    void SpendResources(BuildingLevel level)
    {
        ResourceManager.Instance.currentWood -= level.woodCost;
        ResourceManager.Instance.currentStone -= level.stoneCost;
        ResourceManager.Instance.currentGold -= level.goldCost;
    }

    void ApplyLevelVisual()
    {
        if (visualRoot == null)
            return;

        // borrar modelo actual
        foreach (Transform child in visualRoot)
            Destroy(child.gameObject);

        // instanciar nuevo modelo
        GameObject model = Instantiate(
            CurrentLevelData.modelPrefab,
            visualRoot
        );

        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        renderers = null;

        GetRenderers();
    }

#endregion

#region Render Setup

    [ContextMenu("GetRenderers")]
    private void GetRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

#endregion

#region Visual Methods

    public void SetPreviewMode(bool preview)
    {
        GetRenderers();
        Material mat = preview ? previewMaterial : normalMaterial;

        foreach (Renderer r in renderers)
        {
            Material[] mats = new Material[r.sharedMaterials.Length];

            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;

            r.materials = mats;
        }
    }

    public void SetBuildValid(bool valid)
    {
        Color color = valid ? data.canBuildColor : data.cantBuildColor;

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(propBlock);

            propBlock.SetColor("_Color", color);

            r.SetPropertyBlock(propBlock);
        }
    }

#endregion
}