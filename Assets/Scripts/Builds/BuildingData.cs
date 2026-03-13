using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Buildings/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;
    public int woodCost;
    public int stoneCost;
    public int golCost;
    public Vector2Int size;
    public ResourceType resourceProduced;
    public float productionTime;
    public int productionAmount;
    public GameObject prefab;
    public Color canBuildColor = Color.white;
    public Color cantBuildColor = Color.red;
}