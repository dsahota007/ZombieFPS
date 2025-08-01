using UnityEngine;

public class SimpleWeaponAntiClip : MonoBehaviour
{
    [Header("Anti-Clipping Settings")]
    public Transform weaponHolder;              // Drag your MAIN_ARMS here
    public float weaponViewDistance = 0.3f;     // How far to push weapons back
    public LayerMask weaponLayers = 1 << 6;     // WeaponViewModel layer
    
    private Camera mainCam;
    private Vector3 originalWeaponPos;

    void Start()
    {
        mainCam = GetComponent<Camera>();
        if (weaponHolder != null)
        {
            originalWeaponPos = weaponHolder.localPosition;
        }
    }

    void Update()
    {
        // Adjust weapon position based on camera's near clip plane
        if (weaponHolder != null)
        {
            float adjustedDistance = mainCam.nearClipPlane + weaponViewDistance;
            Vector3 newPos = originalWeaponPos;
            newPos.z = Mathf.Max(adjustedDistance, originalWeaponPos.z);
            weaponHolder.localPosition = newPos;
        }
    }
}
