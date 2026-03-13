using System;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceManager : MonoBehaviour
{
    public float currentWood;
    public float currentStone;
    public float currentGold;

    [SerializeField] private UIManager resourceUI;

    public static event Action<float, float, float> OnResourcesChanged;

    private static ResourceManager _instance;
    public static ResourceManager Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // Indicate that the GameObject won't be destroyed between scenes.
            DontDestroyOnLoad(gameObject);
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        OnResourcesChanged?.Invoke(currentWood, currentStone, currentGold);
    }

    public void AddResource(ResourceType type, float amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                currentWood += amount;
                break;

            case ResourceType.Stone:
                currentStone += amount;
                break;

            case ResourceType.Gold:
                currentGold += amount;
                break;
        }
        OnResourcesChanged?.Invoke(currentWood, currentStone, currentGold);
    }

    public void SpendResources(float woodCost, float stoneCost, float goldCost)
    {
        currentWood -= woodCost;
        currentStone -= stoneCost;
        currentGold -= goldCost;
        OnResourcesChanged?.Invoke(currentWood, currentStone, currentGold);
    }
}