using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Buildings/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;

    public int woodCost;
    public int stoneCost;

    public Vector2Int size;

    public float productionTime;
    //public ResourceType resourceProduced;
    public int productionAmount;
    public GameObject prefab;
    public GameObject previewPrefab;
}