using UnityEngine;
using System.Collections;

public class MysteryBox : MonoBehaviour
{
    public Transform showcasePoint;
    //public float floatHeight = 1.0f;
    public float floatSpeed = 1.5f;
    public float spinSpeed = 60f;
    public float displayTime = 10f; 

    private GameObject currentPreview;
    private WeaponManager weaponManager;
    private bool isBoxOpen = false;
    private float closeTimer = 0f;

    public Transform player;                    
    public float minimumDistanceToOpen = 3f;


    private ArmMovementMegaScript armMovementMegaScript;


    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>();
        armMovementMegaScript = FindObjectOfType<ArmMovementMegaScript>();

    }

    void Update()
    {
        if (!isBoxOpen && Input.GetKeyDown(KeyCode.E))
        {
            float distanceToPlayer = Vector3.Distance(player.position, transform.position);   // write transform.position bc it is attached to the box the script so we know its the box
            if (distanceToPlayer <= minimumDistanceToOpen)
            {
                OpenBoxAndShowRandomWeapon();
            }
            //else
            //{
            //    Debug.Log("You are too far from the Mystery Box!");
            //}

        }

        // Only allow "take" and timer logic when box is open
        if (isBoxOpen)
        {
            closeTimer -= Time.deltaTime;     //we decrement 1 second 

            // Float & spin the preview if there is one
            if (currentPreview != null)
            {
                Vector3 WeaponfloatPosition = showcasePoint.position + Vector3.up * (Mathf.Sin(Time.time * floatSpeed) * 0.2f);   
                currentPreview.transform.position = WeaponfloatPosition;
                currentPreview.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);     //.rotate(x,y,z)    and space.world helps stay at the box. 

                if (Input.GetKeyDown(KeyCode.F) )        //take weapon
                {
                    float distanceToPlayer = Vector3.Distance(player.position, transform.position);   // do this again so we cant pick up the gun from anywhere. 
                    if (distanceToPlayer <= minimumDistanceToOpen && armMovementMegaScript != null && !armMovementMegaScript.isReloading)
                    {
                        GiveWeaponToPlayer();
                    }
                }
            }

            if (closeTimer <= 0f)       //you dont want to take the weapon. 
            {
                ResetBox();
            }
        }
    }

    void OpenBoxAndShowRandomWeapon()
    {

        //if (weaponManager == null || weaponManager.weaponPrefabs == null || weaponManager.weaponPrefabs.Length == 0)     // do nothing if there’s no weapon list set up.
        //    return;

        //if (currentPreview != null)                     // Remove last preview if any (shouldn't be possible)
        //    Destroy(currentPreview);

        // Pick a random weapon prefab
        GameObject NewWeaponPrefab = weaponManager.weaponPrefabs[Random.Range(0, weaponManager.weaponPrefabs.Length)];   //we randomize into the enter list
        currentPreview = Instantiate(NewWeaponPrefab, showcasePoint.position, Quaternion.identity);   //Instantiate(whatToSpawn, whereToSpawn, whichRotation);

        // Remove scripts/colliders to make it a display prop     turning the weapon into a harmless, floating model — like a 3D hologram — that can’t shoot, collide, or move. Just for display.
        foreach (var componentScript in currentPreview.GetComponentsInChildren<MonoBehaviour>())
            Destroy(componentScript);
        //foreach (var colliders in currentPreview.GetComponentsInChildren<Collider>())                 //for now we dont really need ill keep scriptint disabeled in case.
        //    Destroy(colliders);  

        isBoxOpen = true;
        closeTimer = displayTime;
    }

    void GiveWeaponToPlayer()
    {
        if (weaponManager == null || currentPreview == null) return;  //If there's no weapon manager or no weapon preview, stop the function to avoid errors

        // Find which prefab this preview matches
        GameObject prefabToGive = null;
        foreach (var WeaponPrefab in weaponManager.weaponPrefabs)   //loop through all weapon list
        {
            if (WeaponPrefab.name == currentPreview.name.Replace("(Clone)", "").Trim())   //gets rid of clone name, if we dont we get clone and prefabToGive stays null - kinda confusing but works 
            {
                prefabToGive = WeaponPrefab;
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

        // Remove preview and close box bc we took the wepaon so its the same logic if we dont take it.
        ResetBox();
    }

    void ResetBox()
    {
        if (currentPreview != null)
            Destroy(currentPreview);        //destroy weapon, close the box and reset the time
        currentPreview = null;
        isBoxOpen = false;
        closeTimer = 0f;
    }
}
