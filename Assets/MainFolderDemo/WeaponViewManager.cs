using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WeaponViewManager : MonoBehaviour
{
    [Header("Weapon Rendering")]
    public LayerMask weaponLayerMask = 1 << 6; // WeaponViewModel layer
    public float weaponFOV = 60f;              // Separate FOV for weapons
    public float weaponNearClip = 0.01f;       // Very close near clip for weapons

    private Camera mainCamera;
    private Camera weaponCamera;
    private GameObject weaponCameraObj;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        CreateWeaponCamera();
    }

    void CreateWeaponCamera()
    {
        // Create weapon camera as child of main camera
        weaponCameraObj = new GameObject("WeaponView");
        weaponCameraObj.transform.SetParent(transform);
        weaponCameraObj.transform.localPosition = Vector3.zero;
        weaponCameraObj.transform.localRotation = Quaternion.identity;

        // Add camera component
        weaponCamera = weaponCameraObj.AddComponent<Camera>();

        // Configure weapon camera
        weaponCamera.clearFlags = CameraClearFlags.Depth;    // Only depth
        weaponCamera.cullingMask = weaponLayerMask;          // Only weapons
        weaponCamera.fieldOfView = weaponFOV;                // Separate FOV
        weaponCamera.nearClipPlane = weaponNearClip;         // Very close
        weaponCamera.farClipPlane = mainCamera.farClipPlane;
        weaponCamera.depth = mainCamera.depth + 1;           // Render after main camera

        Debug.Log("Weapon camera created for anti-clipping");
    }

    void Update()
    {
        // Sync weapon camera with main camera transformations
        if (weaponCamera != null)
        {
            weaponCamera.fieldOfView = weaponFOV; // Keep weapon FOV separate
            weaponCamera.farClipPlane = mainCamera.farClipPlane;
        }
    }

    void LateUpdate()
    {
        // Ensure weapon camera follows main camera exactly
        if (weaponCameraObj != null)
        {
            weaponCameraObj.transform.localPosition = Vector3.zero;
            weaponCameraObj.transform.localRotation = Quaternion.identity;
        }
    }
}
