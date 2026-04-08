using System;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceManager : MonoBehaviour
{
    public float currentWood;
    public float currentStone;
    public float currentGold;
    public float timeToUpdateResources = 5;
    float timeToUpdateResourcesCounter;
    public float currentWoodToAdd = 0;
    public float currentStoneToAdd = 0;
    public float currentGoldToAdd = 0;

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
        timeToUpdateResourcesCounter = timeToUpdateResources;
        OnResourcesChanged?.Invoke(currentWood, currentStone, currentGold);
    }

    void Update()
    {
        if (timeToUpdateResourcesCounter > 0)
        {
            timeToUpdateResourcesCounter -= Time.deltaTime;
        }

        if (timeToUpdateResourcesCounter < 0)
        {
            AddResources();
        }
    }

    public void UpdateResources(ResourceType type, float amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                currentWoodToAdd += amount;
                break;

            case ResourceType.Stone:
                currentStoneToAdd += amount;
                break;

            case ResourceType.Gold:
                currentGoldToAdd += amount;
                break;
        }
    }

    public void AddResources()
    {
        currentWood += currentWoodToAdd;
        currentStone += currentStoneToAdd;
        currentGold += currentGoldToAdd;
        OnResourcesChanged?.Invoke(currentWood, currentStone, currentGold);
        timeToUpdateResourcesCounter = timeToUpdateResources;
    }

    public void SpendResources(float woodCost, float stoneCost, float goldCost)
    {
        currentWood -= woodCost;
        currentStone -= stoneCost;
        currentGold -= goldCost;
        OnResourcesChanged?.Invoke(currentWood, currentStone, currentGold);
    }
}