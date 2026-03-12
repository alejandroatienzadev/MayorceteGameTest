using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    public BuildingData data;

    [SerializeField] private Renderer[] renderers;

    public Material normalMaterial;
    public Material previewMaterial;

    public void Awake()
    {
        GetRenderers();
    }

    [ContextMenu("GetRenderers")]
    private void GetRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void GenerateResources()
    {
        
    }

    public void SetPreviewMode(bool preview)
    {
        Material mat = preview ? previewMaterial : normalMaterial;

        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
    }

    public void SetBuildValid(bool valid)
    {
        Color color = valid ? data.canBuildColor : data.cantBuildColor;

        foreach (Renderer r in renderers)
        {
            if (r.material.HasProperty("_BaseColor"))
                r.material.SetColor("_BaseColor", color);
            else
                r.material.SetColor("_Color", color);
        }
    }
}
