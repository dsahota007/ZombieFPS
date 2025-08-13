//using Unity.Mathematics;
//using Unity.VisualScripting;
using UnityEngine;
//using static UnityEngine.Rendering.DebugUI;

public class PlayerAttributes : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxStartingHealth = 40f;
    private float currentHealth;

    [Header("Regen Settings")]
    public float regenDelay = 5f;  //how long till i can regen
    public float regenRatePerSecond = 5f; // health per second

    private float lastDamageTime;
    private bool isRegenerating = false;

    void Start()
    {
        currentHealth = maxStartingHealth;
        lastDamageTime = -regenDelay;  // Allows regen to start immediately if untouched  --  so if we we have not taken damage in 5 seconds this we are able to than regen i think.?
                                            //        Time.time >= lastDamageTime + regenDelay
                                            //→ 0 >= -5 + 5
                                            //→ 0 >= 0 true
    }

    void Update()
    {
        //-------------------- Health Regen 
        if (currentHealth < maxStartingHealth && currentHealth > 0)   //only do regen if: currentHealth < maxStartingHealth → you're missing some health -- currentHealth > 0 → you're not dead
        {
            if (!isRegenerating && Time.time >= lastDamageTime + regenDelay)   //-- not already regening and it's been at least 5 seconds (or whatever your regen delay is) since the last hit
            {
                isRegenerating = true;  //now WE NEED TO REGEN
            }

            if (isRegenerating)
            {
                currentHealth += regenRatePerSecond * Time.deltaTime;
                currentHealth = Mathf.Clamp(currentHealth, 0, maxStartingHealth);  //math.clamp (value, min, max)

                UI ui = FindObjectOfType<UI>();
                if (ui != null)
                    ui.UpdateHealthBar(currentHealth / maxStartingHealth);    // -->  gives a value between 0 and 1  (100 / 100 = 1 → full bar)  (50 / 100 = 0.5 → half bar) (0 / 100 = 0 → empty bar)
            }
        }


    }


    public void TakeDamagefromEnemy(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxStartingHealth); // safety net to stay within range -- Make sure currentHealth never goes below 0 or above maxStartingHealth
        //Debug.Log($"Player took {amount} damage. Health: {currentHealth}");

        lastDamageTime = Time.time;         // Reset timer
        isRegenerating = false;             // Cancel regen if it was happening bc we just got hit

        // NEW: tell camera about the hit
        var cam = FindObjectOfType<CameraScript>(); 
        if (cam != null)
            cam.OnPlayerHit(amount, currentHealth / maxStartingHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player is DEAD!");
        }

        UI ui = FindObjectOfType<UI>();
        if (ui != null)
            ui.UpdateHealthBar(currentHealth / maxStartingHealth);      // -->  gives a value between 0 and 1  (100 / 100 = 1 → full bar)  (50 / 100 = 0.5 → half bar) (0 / 100 = 0 → empty bar)
    }

    public void IncreaseHealthFromMoreHealthPerk(float amount)  //MORE HEALTH PERK
    {
        maxStartingHealth = amount;

    }

    // PlayerAttributes.cs
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxStartingHealth);

        UI ui = FindObjectOfType<UI>();
        if (ui != null)
            ui.UpdateHealthBar(currentHealth / maxStartingHealth);
        // Note: we DON'T touch lastDamageTime/isRegenerating here.
    }

    public float GetCurrentHealth01()           //getter for blood splatter
    {
        return currentHealth / maxStartingHealth;
    }


}



