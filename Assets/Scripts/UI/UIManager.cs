using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public CanvasGroup mainCanvas;
    public CanvasGroup buildCanvas;
    public CanvasGroup resourceCanvas;
    public CanvasGroup editBuildingCanvas;

    public TextMeshProUGUI stoneAmountText;
    public TextMeshProUGUI woodAmountText;
    public TextMeshProUGUI goldAmountText;

    private static UIManager _instance;
    public static UIManager Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }else
        {
            Destroy(gameObject);
        }
        DisableCanvas(buildCanvas);
        DisableCanvas(editBuildingCanvas);
        EnableCanvas(mainCanvas);
    }

    void OnEnable()
    {
        ResourceManager.OnResourcesChanged += UpdateResources;
    }

    void OnDisable()
    {
        ResourceManager.OnResourcesChanged -= UpdateResources;
    }

    public void UpdateResources(float wood, float stone, float gold)
    {
        woodAmountText.text = wood.ToString("0");
        stoneAmountText.text = stone.ToString("0");
        goldAmountText.text = gold.ToString("0");
    }

    public void EnableCanvas(CanvasGroup canvas)
    {
        canvas.alpha = 1f;
        canvas.blocksRaycasts = true;
        canvas.interactable = true;
    }

    public void DisableCanvas(CanvasGroup canvas)
    {
        canvas.alpha = 0f;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }
    public void EnableEditMode()
    {
        DisableCanvas(mainCanvas);
        DisableCanvas(buildCanvas);
        EnableCanvas(editBuildingCanvas);
    }

    public void DisableEditMode()
    {
        DisableCanvas(editBuildingCanvas);
        EnableCanvas(mainCanvas);
    }
}
