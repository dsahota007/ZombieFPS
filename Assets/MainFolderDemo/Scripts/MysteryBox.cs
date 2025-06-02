using UnityEngine;
using System.Collections;

public class MysteryBox : MonoBehaviour
{
    public Transform showcasePoint;
    public float floatHeight = 1.0f;
    public float floatSpeed = 1.5f;
    public float spinSpeed = 60f;
    public float displayTime = 10f; // Time before the weapon disappears if not taken

    private GameObject currentPreview;
    private WeaponManager weaponManager;
    private bool isBoxOpen = false;
    private float closeTimer = 0f;

    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>();
    }

    void Update()
    {
        if (!isBoxOpen && Input.GetKeyDown(KeyCode.E))
        {
            ShowRandomWeapon();
        }

        // Only allow "take" and timer logic when box is open
        if (isBoxOpen)
        {
            closeTimer -= Time.deltaTime;

            // Float & spin the preview if there is one
            if (currentPreview != null)
            {
                Vector3 floatPos = showcasePoint.position + Vector3.up * (Mathf.Sin(Time.time * floatSpeed) * 0.2f + floatHeight);
                currentPreview.transform.position = floatPos;
                currentPreview.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

                // Take the weapon
                if (Input.GetKeyDown(KeyCode.F))
                {
                    GiveWeaponToPlayer();
                }
            }

            // If timer runs out, close the box/reset
            if (closeTimer <= 0f)
            {
                ResetBox();
            }
        }
    }

    void ShowRandomWeapon()
    {
        if (weaponManager == null || weaponManager.weaponPrefabs == null || weaponManager.weaponPrefabs.Length == 0)
            return;

        // Remove last preview if any (shouldn't be possible)
        if (currentPreview != null)
            Destroy(currentPreview);

        // Pick a random weapon prefab
        GameObject prefab = weaponManager.weaponPrefabs[Random.Range(0, weaponManager.weaponPrefabs.Length)];
        currentPreview = Instantiate(prefab, showcasePoint.position + Vector3.up * floatHeight, Quaternion.identity);

        // Remove scripts/colliders to make it a display prop
        foreach (var comp in currentPreview.GetComponentsInChildren<MonoBehaviour>())
            Destroy(comp);
        foreach (var col in currentPreview.GetComponentsInChildren<Collider>())
            Destroy(col);

        isBoxOpen = true;
        closeTimer = displayTime;
    }

    void GiveWeaponToPlayer()
    {
        if (weaponManager == null || currentPreview == null) return;

        // Find which prefab this preview matches
        GameObject prefabToGive = null;
        foreach (var prefab in weaponManager.weaponPrefabs)
        {
            if (prefab.name == currentPreview.name.Replace("(Clone)", "").Trim())
            {
                prefabToGive = prefab;
                break;
            }
        }

        if (prefabToGive == null)
        {
            Debug.LogWarning("Could not find weapon prefab to give!");
            ResetBox();
            return;
        }

        weaponManager.GiveWeapon(prefabToGive);

        // Remove preview and close box
        ResetBox();
    }

    void ResetBox()
    {
        if (currentPreview != null)
            Destroy(currentPreview);
        currentPreview = null;
        isBoxOpen = false;
        closeTimer = 0f;
    }
}
