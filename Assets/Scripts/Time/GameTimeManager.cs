using UnityEngine;

public enum DayPhase { Morning, Afternoon, Night }

public class GameTimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float phaseDuration = 300f; 
    [SerializeField] private float timer = 0f;
    public int currentDay = 1;
    public DayPhase currentPhase = DayPhase.Morning;

    [Header("Visual References")]
    public Light sunLight;
    
    [Tooltip("Gradient for the full cycle (Morning -> Afternoon -> Night). Start and end with the same Night color to avoid jumps.")]
    public Gradient dayCycleGradient;
    
    [Tooltip("Intensity curve for the full cycle. Start and end at the same value.")]
    public AnimationCurve intensityCurve;

    void Update()
    {
        timer += Time.deltaTime;

        float totalProgress = CalculateTotalProgress();

        UpdateVisuals(totalProgress);

        if (timer >= phaseDuration)
        {
            AdvancePhase();
        }
    }

    float CalculateTotalProgress()
    {
        float phaseOffset = (int)currentPhase * phaseDuration;
        float totalDayDuration = phaseDuration * 3f;
        
        return (phaseOffset + timer) / totalDayDuration;
    }

    void UpdateVisuals(float progress)
    {
        if (sunLight == null) return;

        sunLight.color = dayCycleGradient.Evaluate(progress);
        sunLight.intensity = intensityCurve.Evaluate(progress);

        //Sun rotation.
        float sunAngle = Mathf.Lerp(-10f, 190f, progress);
        sunLight.transform.localRotation = Quaternion.Euler(sunAngle, -90f, 0f);
    }

    void AdvancePhase()
    {
        timer = 0f;
        if (currentPhase == DayPhase.Night)
        {
            currentPhase = DayPhase.Morning;
            currentDay++;
            Debug.Log("Starting Day: " + currentDay);
        }
        else
        {
            currentPhase++;
        }
    }
}