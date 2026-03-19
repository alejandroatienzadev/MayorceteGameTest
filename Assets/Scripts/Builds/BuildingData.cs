using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Buildings/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;

    public Vector2Int size;
    public ResourceType resourceProduced;
    public float dustRadius;

    public BuildingLevel[] levels;

    public Color canBuildColor = Color.white;
    public Color cantBuildColor = Color.red;
}