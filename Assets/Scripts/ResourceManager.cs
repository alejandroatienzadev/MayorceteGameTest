using Mono.Cecil;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourceManager : MonoBehaviour
{
    public float currentWood;
    public float currentStone;

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
}
