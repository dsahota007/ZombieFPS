using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthRagdoll : MonoBehaviour
{
    public int Health = 100;
    public GameObject ragdollRoot;
    public float ragdollForce = 3f;
    public Collider BoxRootCollider;

    private float currentHealth = 0;
    private bool isDead = false;

    private Animator animator;
    private NavMeshAgent agent;

    //--
    private PlayerMovement _player;         //THIS IS ALL FOR STEPPING ON HEJAD WE CAN DO BETTER
    private CharacterController _playerCC;  //THIS IS ALL FOR STEPPING ON HEJAD WE CAN DO BETTER
    public float HealthPercent => Mathf.Clamp01(currentHealth / Mathf.Max(1f, (float)Health)); //this is for healt bar for enemy 

    [Header("Health Bar UI")]
    public Canvas healthCanvas;       // World Space Canvas prefab
    public UnityEngine.UI.Slider healthBar; // Slider on that canvas
    public GameObject nameTagGO;
    public Vector3 healthBarOffset = new Vector3(0, 2, 0); // bar height above enemy

    private Transform lookCam; // reference to player's camera



    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        SetRagdollState(false);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerBody"), LayerMask.NameToLayer("DeadBody"));   // Ignore collisions between PlayerBody and DeadBody layers
        
        currentHealth = Health;
        _player = FindFirstObjectByType<PlayerMovement>();
        if (_player != null) _playerCC = _player.GetComponent<CharacterController>();
         
        var camScript = FindFirstObjectByType<CameraScript>();   //fetch sciprt for cam
        lookCam = camScript != null ? camScript.cam : Camera.main?.transform;

        // init slider
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f; // full health
        }

        SetHealthUIVisible(true);

    }
    private void SetHealthUIVisible(bool visible)
    {
        if (healthCanvas) healthCanvas.enabled = visible;  // hides all children under the canvas
        if (nameTagGO) nameTagGO.SetActive(visible);    // explicit toggle in case Tag is not under that canvas
    }



    void Update()
    { 
        if (!isDead && _player != null && _playerCC != null && BoxRootCollider != null)   //THIS IS ALL FOR STEPPING ON HEJAD WE CAN DO BETTER
        {
            bool airborne = !_player.IsGrounded();
            Physics.IgnoreCollision(BoxRootCollider, _playerCC, airborne);
        }

        if (!isDead && healthCanvas != null)
        {
            healthCanvas.transform.position = transform.position + healthBarOffset;

            if (lookCam != null)
            {
                Vector3 dir = healthCanvas.transform.position - lookCam.position; 
                dir.y = 0f;                 // keep upright
                if (dir.sqrMagnitude > 0.0001f)
                    healthCanvas.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

    }


    public void RegisterHit(Vector3 hitDirection)
    {
        var dm = FindFirstObjectByType<DropManager>();
        if (dm != null && dm.IsInstaKill)
        {
            Die(hitDirection);
            return;
        }


        if (isDead) return;   //leave func if already deaad

        currentHealth++;

        var cam = FindObjectOfType<CameraScript>(); //get cam script
        if (currentHealth >= Health)
        {
            if (cam) cam.ShowHitmarker(true);
            Die(hitDirection);
 
        }
        else
        {
            if (cam) cam.ShowHitmarker(false);

        }
    }
    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        var dm = FindFirstObjectByType<DropManager>();
        if (dm != null && dm.IsInstaKill)
        {
            Die(hitDirection);
            return;
        }


        if (isDead) return;   //exit func if dead

        currentHealth -= damage;    //decrement the damage from health
        var cam = FindObjectOfType<CameraScript>();  //get the cam script


        if (currentHealth <= 0f)
        {
            if (cam) cam.ShowHitmarker(true); //showhitmarkker
            Die(hitDirection);
            return;     //get outt the this part 
        }
        else
        {
            if (cam) cam.ShowHitmarker(false);  // regular hitmarker
        }

        if (healthBar != null)
            healthBar.value = Mathf.Clamp01(currentHealth / (float)Health);  //After changing HP, convert to 0..1 and assign slider value so it shrinks/grows correctly

    }

    void Die(Vector3 hitDirection)
    {
        isDead = true;
        PointManager.Instance.AddPoints(50);

        if (animator) animator.enabled = false;         //turn all that shit off animations, navmesh and the boxCollider so we dont run into it 
        if (agent) agent.enabled = false;
        if (BoxRootCollider) BoxRootCollider.enabled = false;

        var ds = FindFirstObjectByType<DropSpawner>();
        if (ds) ds.TrySpawnDrop(transform.position + Vector3.up * 0.5f);

        // Disable other attack/AI scripts if any----------------------------- idk waht this block does 
        //MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        //foreach (var script in scripts)
        //{
        //    if (script != this) // Don't disable the health/ragdoll script itself
        //        script.enabled = false;
        //}
        //--------------------------------------------------------------------- idk waht this block does 

        SetRagdollState(true);
        ApplyRagdollForce(hitDirection);

        // Change layer to DeadBody (no collision with player)
        //SetLayerRecursively(ragdollRoot, LayerMask.NameToLayer("DeadBody"));

        // Dynamically ignore collisions between this ragdoll and the Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            foreach (var ragdollCol in ragdollRoot.GetComponentsInChildren<Collider>())  //get all collider in enemy
            {
                foreach (var playerCol in player.GetComponentsInChildren<Collider>())      //get all collision in player
                {
                    Physics.IgnoreCollision(ragdollCol, playerCol, true);               //ignore it so we can walk over deadbodies
                }
            }
        }
        FindObjectOfType<ZombieSpawner>().OnZombieKilled();    //decrement amount of zombies for the spawner
        Destroy(gameObject, 30f);    //make bodies dissapear. 

        SetHealthUIVisible(false);

    }


    void SetRagdollState(bool enabled)
    {
        foreach (var rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = !enabled;   //find all components in root and turn off Kinematic bc this allows physics to move the body

        foreach (var col in ragdollRoot.GetComponentsInChildren<Collider>())
            col.enabled = enabled;     //we turn off the collider as well
    }

    void ApplyRagdollForce(Vector3 direction)
    {
        var rbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();   //get all rigidBodies in the ragdoll
        if (rbs.Length > 0)   
            rbs[0].AddForce(direction * ragdollForce, ForceMode.Impulse);   //we use this to push them in direction and its the FIRST bone 
          
    }
    public void SetHealth(int newHealth)  //this is for incrementing health every round
    {
        Health = newHealth;
        currentHealth = newHealth;
    }
    public bool IsDead()
    {
        return isDead;
    }


    //    void SetLayerRecursively(GameObject obj, int layer)
    //    {
    //        if (obj == null) return;
    //        obj.layer = layer;

    //        foreach (Transform child in obj.transform)
    //            SetLayerRecursively(child.gameObject, layer);
    //    }
}