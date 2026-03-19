using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Building : MonoBehaviour, IDamageable
{
    [Header("Data")]
    public BuildingData data;

    [SerializeField] private Renderer[] renderers;

    public bool isBuilded;

    [Header("UI")]
    public BuildingProgressUI progressUI;

    [Header("Visual Configuration")]
    [SerializeField] private Transform visualRoot;
    public Material normalMaterial;
    public Material previewMaterial;
    public VisualEffect dustVFX;

    [Header("DEBUG Info")]
    [SerializeField] private float productionTime;
    [SerializeField] private int productionAmount;
    [SerializeField] private int currentLevel = 0;
    public int buildingLevel;

    private MaterialPropertyBlock propBlock;
    private float productionCounter;
    private float buildTimer;
    private bool isUnderConstruction;

    public Vector2Int gridOrigin;
    public Vector2Int gridSize;

    [Header("Construction Animation")]
    [SerializeField] private float startYOffset = -2f;

    public BuildingLevel CurrentLevelData
    {
        get 
        { 
            if (data == null || data.levels == null || data.levels.Length == 0) return null;
            return data.levels[currentLevel]; 
        }
    }

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        
        if (visualRoot == null) visualRoot = transform.Find("Model");
    }

    void Start()
    {
        if (dustVFX)
        {
            dustVFX.gameObject.SetActive(false);
        }   
    }

    /// <summary>
    /// MÉTODO CLAVE: Llamado por BuildingManager justo después de instanciar.
    /// Esto evita el NullReferenceException.
    /// </summary>
    public void Initialize(BuildingData buildingData)
    {
        this.data = buildingData;
        currentLevel = 0;
        
        if (CurrentLevelData != null)
        {
            buildingLevel = CurrentLevelData.level;
            ApplyLevelStats();
            ApplyLevelVisual();
            
            this.gameObject.name = data.name;

            productionCounter = productionTime;
        }
    }

    void Update()
    {
        if (isUnderConstruction)
        {
            buildTimer -= Time.deltaTime;

            float totalTime = CurrentLevelData.buildTime;
            float elapsedTime = totalTime - buildTimer;
            float progress = Mathf.Clamp01(elapsedTime / totalTime);

            if (visualRoot != null)
            {
                float currentY = Mathf.Lerp(startYOffset, 0, progress);
                visualRoot.localPosition = new Vector3(0, currentY, 0);
            }

            if (progressUI != null)
                progressUI.SetProgress(elapsedTime, totalTime);

            if (buildTimer <= 0)
            {
                FinishConstruction();
            }

            return;
        }

        if (!isBuilded) return;

        productionCounter -= Time.deltaTime;
        if (productionCounter <= 0)
        {
            GenerateResources();
            productionCounter = productionTime;
        }
    }

    void GenerateResources()
    {
        if (data == null) return;
        ResourceManager.Instance.AddResource(data.resourceProduced, productionAmount);
        Debug.Log($"Generados {productionAmount} de {data.resourceProduced}");
    }

#region Upgrade & Build System

    public void StartConstruction()
    {
        if (CurrentLevelData == null) return;

        ApplyLevelVisual();

        if (visualRoot != null)
            visualRoot.localPosition = new Vector3(0, startYOffset, 0);

        isUnderConstruction = true;
        isBuilded = false;
        buildTimer = CurrentLevelData.buildTime;

        if (progressUI && progressUI.fillImage) progressUI.fillImage.gameObject.SetActive(true);

        if (dustVFX)
        {
            dustVFX.gameObject.SetActive(true);
            dustVFX.SetFloat("CircleRadius", data.dustRadius);
            dustVFX.Play();
        }
    }

    void FinishConstruction()
    {
        isUnderConstruction = false;
        isBuilded = true;

        if (visualRoot != null) visualRoot.localPosition = Vector3.zero;

        ApplyLevelStats();

        productionCounter = productionTime;
        
        if (progressUI && progressUI.fillImage) 
            progressUI.fillImage.gameObject.SetActive(false);

        if (dustVFX)
        {
            dustVFX.Stop();
        }
    }

    [ContextMenu("Upgrade")]
    public void Upgrade()
    {
        if (isUnderConstruction || data == null) return;
        if (currentLevel >= data.levels.Length - 1) return;

        BuildingLevel nextLevel = data.levels[currentLevel + 1];

        if (!HasResources(nextLevel)) return;

        SpendResources(nextLevel);
        currentLevel++;
        buildingLevel = CurrentLevelData.level;

        StartConstruction();
    }

    void ApplyLevelStats()
    {
        if (CurrentLevelData == null) return;
        productionAmount = CurrentLevelData.productionAmount;
        productionTime = CurrentLevelData.productionTime;
    }

    bool HasResources(BuildingLevel level)
    {
        return ResourceManager.Instance.currentWood >= level.woodCost &&
               ResourceManager.Instance.currentStone >= level.stoneCost &&
               ResourceManager.Instance.currentGold >= level.goldCost;
    }

    void SpendResources(BuildingLevel level)
    {
        ResourceManager.Instance.currentWood -= level.woodCost;
        ResourceManager.Instance.currentStone -= level.stoneCost;
        ResourceManager.Instance.currentGold -= level.goldCost;
    }

    void ApplyLevelVisual()
    {
        if (visualRoot == null || CurrentLevelData == null) return;

        foreach (Transform child in visualRoot)
        {
            Destroy(child.gameObject);
        }

        if (CurrentLevelData.modelPrefab != null)
        {
            GameObject modelInstance = Instantiate(CurrentLevelData.modelPrefab, visualRoot);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;

            renderers = modelInstance.GetComponentsInChildren<Renderer>();
        }
    }

#endregion

#region Visual Methods

    public void SetPreviewMode(bool preview)
    {
        if (renderers == null || renderers.Length == 0) 
            renderers = GetComponentsInChildren<Renderer>();

        Material mat = preview ? previewMaterial : normalMaterial;
        if (mat == null) return;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            
            r.materials = mats;
        }
    }

    public void SetBuildValid(bool valid)
    {
        if (data == null) return;
        Color color = valid ? data.canBuildColor : data.cantBuildColor;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
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