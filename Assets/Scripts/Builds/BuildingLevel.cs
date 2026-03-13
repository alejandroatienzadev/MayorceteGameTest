using UnityEngine;

[System.Serializable]
public class BuildingLevel
{
    public int level;

    public int woodCost;
    public int stoneCost;
    public int goldCost;

    public float productionTime;
    public int productionAmount;

    public GameObject prefab;
}