using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;   // normalized 0–1
            slider.value = 1f;      // start full
        }
    }

    public void UpdateHealthBar(float current, float max)
    {
        if (slider == null) return;

        float percent = Mathf.Clamp01(current / Mathf.Max(1f, max));
        slider.value = percent;

        // Hide if dead
        if (percent <= 0f)
            gameObject.SetActive(false);
    }
}
