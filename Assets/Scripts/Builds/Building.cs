using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    public BuildingData data;

    [SerializeField] private Renderer[] renderers;

    public int currentLevel = 0;

    public bool isBuilded;

    public Material normalMaterial;
    public Material previewMaterial;
    public float counter;

    public void Awake()
    {
        GetRenderers();
        counter = data.productionTime;
    }

    void Update()
    {
        if (isBuilded)
        {
            if (counter >= 0)
            {
                counter -= Time.deltaTime;
            }else
            {
                GenerateResources();
            }
        }
    }

    void GenerateResources()
    {
        ResourceManager.Instance.AddResource(data.resourceProduced, data.productionAmount);
        counter = data.productionTime;
    }

    [ContextMenu("GetRenderers")]
    private void GetRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

#region Visual Methods
    public void SetPreviewMode(bool preview) 
    { 
        Material mat = preview ? previewMaterial : normalMaterial; 
    
        foreach (Renderer r in renderers) 
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }
            r.materials = mats; 
        } 
    }

    public void SetBuildValid(bool valid) 
    { 
        Color color = valid ? data.canBuildColor : data.cantBuildColor; 
        foreach (Renderer r in renderers) 
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color); 
            r.SetPropertyBlock(propBlock);
        }
    }
#endregion
}