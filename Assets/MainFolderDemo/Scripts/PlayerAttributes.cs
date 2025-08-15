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

    //---- perk reset setting
    private float _baseMaxHealth;
    private float _baseRegenDelay;
    private float _baseRegenRate;

    void Start()
    {
        currentHealth = maxStartingHealth;
        lastDamageTime = -regenDelay;  // Allows regen to start immediately if untouched  --  so if we we have not taken damage in 5 seconds this we are able to than regen i think.?
                                       //        Time.time >= lastDamageTime + regenDelay
                                       //→ 0 >= -5 + 5
                                       //→ 0 >= 0 true

        _baseMaxHealth = maxStartingHealth;
        _baseRegenDelay = regenDelay;
        _baseRegenRate = regenRatePerSecond;

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
            ResetAllPerksOnDeath();
        }

        UI ui = FindObjectOfType<UI>();
        if (ui != null)
            ui.UpdateHealthBar(currentHealth / maxStartingHealth);      // -->  gives a value between 0 and 1  (100 / 100 = 1 → full bar)  (50 / 100 = 0.5 → half bar) (0 / 100 = 0 → empty bar)
    }

    public void IncreaseHealthFromMoreHealthPerk(float amount)  //MORE HEALTH PERK
    {
        maxStartingHealth = amount;
    }

    public void IncreaseRegenFromMoreRevivePerk(float timeToRegen, float regenRatePerSecondIncrease)  //MORE REVIVE PERK
    {
        regenDelay = timeToRegen;  //how long till i can regen
        regenRatePerSecond = regenRatePerSecondIncrease; // health per second
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

    // wipe all perks + UI + stats (call this when you die)
    public void ResetAllPerksOnDeath()
    {
        // 1) global weapon modifiers back to normal
        Weapon.GlobalFireRateMult = 1f;
        Weapon.GlobalReloadSpeedMult = 1f;
        MagicManager.GlobalCooldownMult = 1f;     // back to normal cooldown


        // 2) player stats back to base
        maxStartingHealth = _baseMaxHealth;
        regenDelay = _baseRegenDelay;
        regenRatePerSecond = _baseRegenRate;
        currentHealth = Mathf.Min(currentHealth, maxStartingHealth);

        // 3) speeds back to base (if speed perk changed them)
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.ResetSpeedsToBase();   // add tiny method below in PlayerMovement

        // 4) clear perk “owned” flags so you can buy again
        var fire = FindFirstObjectByType<MoreFireRatePerk>(); if (fire) fire.hasMoreFireRatePerk = false;
        var speed = FindFirstObjectByType<MoreSpeedPerk>(); if (speed) speed.hasMoreSpeedPerk = false;
        var health = FindFirstObjectByType<MoreHealthPerk>(); if (health) health.hasMoreHealthPerk = false;
        var revive = FindFirstObjectByType<MoreRevivePerk>(); if (revive) revive.hasMoreRevivePerk = false;
        var fasthands = FindFirstObjectByType<FastHandsPerk>(); if (fasthands) fasthands.hasFastHandsPerk = false;
        var magicCooldonw = FindFirstObjectByType<MagicCooldownPerk>(); if (magicCooldonw) magicCooldonw.hasMagicCooldownPerk = false;

        // 5) remove perk icons from the bar
        var ui = FindFirstObjectByType<UI>();
        if (ui != null)
        {
            ui.RemovePerkIcon(PerkType.FireRate);
            ui.RemovePerkIcon(PerkType.Speed);
            ui.RemovePerkIcon(PerkType.Health);
            ui.RemovePerkIcon(PerkType.Revive);
            ui.RemovePerkIcon(PerkType.FastHands);
            ui.RemovePerkIcon(PerkType.MagicCooldown);
        }
    }



}



