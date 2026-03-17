using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour, IDamageable
{
    public BuildingData data;

    [SerializeField] private Renderer[] renderers;

    public bool isBuilded;
    [Header("UI")]
    public BuildingProgressUI progressUI;

    [Header("DEBUG Level Data")]
    [SerializeField] private float productionTime;
    [SerializeField] private int productionAmount;
    [SerializeField] private Transform visualRoot;

    [Header("Materials")]
    public Material normalMaterial;
    public Material previewMaterial;
    private MaterialPropertyBlock propBlock;

    // Contador para la generación de recursos.
    private float counter;

    // Nivel interno del edificio.
    private int currentLevel = 0;
    // Nivel de control del edificio.
    public int buildingLevel;

    private float buildTimer;
    private bool isUnderConstruction;

    public Vector2Int gridOrigin;
    public Vector2Int gridSize;

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
        if (isUnderConstruction)
        {
            buildTimer -= Time.deltaTime;

            if (progressUI != null)
                progressUI.SetProgress(CurrentLevelData.buildTime - buildTimer, CurrentLevelData.buildTime);

            if (buildTimer <= 0)
            {
                FinishConstruction();
            }

            return;
        }

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
        ResourceManager.Instance.AddResource(data.resourceProduced, productionAmount);
        Debug.Log("Generados " + productionAmount + data.resourceProduced);
    }

#region Upgrade & Build System
    public void StartConstruction()
    {
        isUnderConstruction = true;
        isBuilded = false;

        buildTimer = CurrentLevelData.buildTime;

        if(progressUI) progressUI.fillImage.gameObject.SetActive(true);
    }

    void FinishConstruction()
    {
        isUnderConstruction = false;
        isBuilded = true;

        ApplyLevelVisual();
        ApplyLevelStats();

        counter = productionTime;
        if (progressUI) progressUI.fillImage.gameObject.SetActive(false);
    }

    [ContextMenu("Upgrade")]
    public void Upgrade()
    {
        if (isUnderConstruction)
            return;

        if (currentLevel >= data.levels.Length - 1)
            return;

        BuildingLevel nextLevel = data.levels[currentLevel + 1];

        if (!HasResources(nextLevel))
            return;

        SpendResources(nextLevel);

        currentLevel++;

        buildingLevel = CurrentLevelData.level;

        StartConstruction();
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
        {
            Destroy(child.gameObject);
        }

        // instanciar nuevo modelo
        GameObject model = Instantiate(CurrentLevelData.modelPrefab, visualRoot);

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
    public void TakeDamage(float _damage)
    {
        
    }

}