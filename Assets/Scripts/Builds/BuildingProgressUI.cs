using UnityEngine;
using UnityEngine.UI;

public class BuildingProgressUI : MonoBehaviour
{
    public Image fillImage;

    void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }

    public void SetProgress(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
