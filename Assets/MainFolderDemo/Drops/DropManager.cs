using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Durations (seconds)")]
    public float doublePointsDuration = 30f;
    public float instaKillDuration = 25f;

    private float doublePointsEnd = -999f;
    private float instaKillEnd = -999f;

    public bool IsDoublePoints => Time.time < doublePointsEnd;
    public bool IsInstaKill => Time.time < instaKillEnd;

    void Update()
    {
        // auto reset when DP expires
        if (!IsDoublePoints) PointManager.GlobalPointsMult = 1f;
    }

    public void Apply(DropType type)
    {
        var ui = FindFirstObjectByType<UI>();

        switch (type)
        {
            case DropType.DoublePoints:
                doublePointsEnd = Time.time + doublePointsDuration;
                PointManager.GlobalPointsMult = 2f;
                FindFirstObjectByType<UI>()?.ShowTimedPowerup("double", "DOUBLE POINTS", doublePointsDuration);
                break;

            case DropType.InstaKill:
                instaKillEnd = Time.time + instaKillDuration;
                FindFirstObjectByType<UI>()?.ShowTimedPowerup("insta", "INSTA-KILL", instaKillDuration);
                break;

            case DropType.Nuke:
                DoNuke();
                FindFirstObjectByType<UI>()?.ShowToast("NUKE!", 1.5f);
                break;

            case DropType.MaxAmmo:
                DoMaxAmmo();
                FindFirstObjectByType<UI>()?.ShowToast("MAX AMMO!", 1.5f);
                break;
        }

        void DoMaxAmmo()
        {
            // Refill every Weapon attached to the player (active or holstered)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                foreach (var w in player.GetComponentsInChildren<Weapon>(true)) // true = include inactive (holstered)
                {
                    if (w != null && !w.isWeaponBeingShowcased) // skip box preview
                        w.RefillFull();
                }
            }

            // (Optional safety) also top off the active weapon if you keep it elsewhere
            var active = WeaponManager.ActiveWeapon;
            if (active != null && !active.isWeaponBeingShowcased)
                active.RefillFull();
        }


        void DoNuke()
        {
            var all = FindObjectsByType<EnemyHealthRagdoll>(FindObjectsSortMode.None);
            foreach (var e in all)
            {
                if (e == null || e.IsDead()) continue;
                Vector3 dir = (e.transform.position - transform.position).normalized;
                e.TakeDamage(999999f, dir);
            }
        }
    }
}
