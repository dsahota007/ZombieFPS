using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.LightTransport;

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

    //------------------------------------------------------------------------

    private PlayerMovement _player;         //THIS IS ALL FOR STEPPING ON HEAD WE CAN DO BETTER
    private CharacterController _playerCC;  //THIS IS ALL FOR STEPPING ON HEAD WE CAN DO BETTER
    public float HealthPercent => Mathf.Clamp01(currentHealth / Mathf.Max(1f, (float)Health)); //this is for healt bar for enemy 

    [Header("Health Bar UI")]
    public Canvas healthCanvas;       // World Space Canvas prefab
    public UnityEngine.UI.Slider healthBar; // Slider on that canvas
    public GameObject nameTagGO;
    public Vector3 healthBarOffset = new Vector3(0, 2, 0); // bar height above enemy

    private Transform lookCam; // reference to player's camera

    //------------------------------------------------------------------------

    [Header("Health Bar Visibility")]
    public float showMaxDistance = 25f;           // only within this distance
    [Range(0f, 1f)] public float centerDotThreshold = 0.85f; // ~31° cone (cos θ)
    public bool requireLineOfSight = true;        // raycast check
    public LayerMask occlusionMask = ~0;          // what can block view (walls, props)
    public float fadeSpeed = 10f;                 // canvas alpha lerp
    public Vector3 lookAtOffset = new Vector3(0, 1.6f, 0); // aim at head height

    private CanvasGroup _cg; //alpha fade
    private Renderer _nameTagRenderer;   // <— add

    // ---- FIRE INFUSION DOT (damage over time) -------------------------------------

    private bool fireDotActive = false;
    private float fireDotEndTime = 0f;
    private float fireNextTickTime = 0f;
    private float fireDotPctPerSec = 0f;          // e.g., 0.03f == 3%/sec
    private GameObject activeFireVFX;

    // --- VENOM INFUSION state -------------------------------------
    private bool venomDotActive = false;
    private float venomDotPctPerSec = 0f;
    private float venomDotEndTime = 0f;
    private float venomNextTickTime = 0f;
    private GameObject activeVenomVFX = null;




    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        SetRagdollState(false);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerBody"), LayerMask.NameToLayer("DeadBody"));   // Ignore collisions between PlayerBody and DeadBody layers
        
        currentHealth = Health;
        _player = FindFirstObjectByType<PlayerMovement>();
        if (_player != null) _playerCC = _player.GetComponent<CharacterController>();

        var camScript = FindFirstObjectByType<CameraScript>();
        lookCam = camScript != null ? camScript.cam : Camera.main?.transform;

        // init slider
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 1f; // full health
        }

        // GET/ADD CanvasGroup ON THE HEALTH CANVAS ROOT
        _nameTagRenderer = nameTagGO ? nameTagGO.GetComponent<Renderer>() : null;

        // CanvasGroup init should NOT be inside the healthBar check:
        if (healthCanvas != null)
        {
            _cg = healthCanvas.GetComponent<CanvasGroup>();  //we get entire canvas
            if (_cg == null) _cg = healthCanvas.gameObject.AddComponent<CanvasGroup>();  //we get if we dont find
            _cg.alpha = 0f;              // start hidden
            _cg.blocksRaycasts = false;        
            //_cg.interactable = false;
            healthCanvas.enabled = true;
        }

        // IMPORTANT: remove the force-show
        // SetHealthUIVisible(true);  <-- delete this line


        //SetHealthUIVisible(true);
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
        if (!isDead && _cg != null)
        {
            float target = ShouldShowBar() ? 1f : 0f;    //we want fully shown or hidden
            _cg.alpha = Mathf.MoveTowards(_cg.alpha, target, fadeSpeed * Time.deltaTime);    //MoveTowards(current, target, max change)  -- fade logic 

            bool visibleNow = _cg.alpha > 0.01f;   //If the alpha is greater than ~0.01 (almost invisible), treat it as visible
            //_cg.blocksRaycasts = visibleNow;
            //_cg.interactable = visibleNow;

            // If the tag is 3D TMP (has a MeshRenderer), toggle it here:
            if (_nameTagRenderer != null)
                _nameTagRenderer.enabled = visibleNow;
        }

        // ---- FIRE infusion DOT tick ----
        if (fireDotActive)
        {
            if (Time.time >= fireDotEndTime || isDead)  //Check if the burn effect should stop
            {
                fireDotActive = false;
                if (activeFireVFX != null) 
                { 
                    Destroy(activeFireVFX); //disable it all 
                    activeFireVFX = null;     
                }
            }
            else if (Time.time >= fireNextTickTime)  //Otherwise, if it’s time to deal the next tick of damage.
            {
                fireNextTickTime += 1f; // next second
                float tickDamage = Mathf.Max(1f, Health * fireDotPctPerSec);    //Example: Enemy has 100 HP, fireDotPctPerSec = 0.03f (3%) → 3 dmg/sec    ---->   Ensures at least 1 damage per tick. Without it, a tiny - health enemy(like 10 HP) with 3 % DOT → 0.3 dmg / sec, which Unity would round down to 0 → no damage.
                TakeDamage(tickDamage, Vector3.zero); // direction not important for DOT
            }
        }

        // --- VENOM infusion DOT tick ---
        if (venomDotActive)
        {
            if (Time.time >= venomDotEndTime || isDead)
            {
                venomDotActive = false;
                if (activeVenomVFX != null) { Destroy(activeVenomVFX); activeVenomVFX = null; }
            }
            else if (Time.time >= venomNextTickTime)
            {
                venomNextTickTime += 1f; // next second

                // % of MAX health per second (minimum 1 damage so it always hurts)
                float tickDamage = Mathf.Max(1f, Health * venomDotPctPerSec);
                TakeDamage(tickDamage, Vector3.zero);
            }
        }

    }

    //-------------------------------INFUSION ATTACK BULLET ------------

    public void ApplyFireInfusionEffect(float durationSeconds, float percentPerSec, GameObject onEnemyVFXPrefab, Vector3 vfxLocalPos, Vector3 vfxLocalEuler, Vector3 vfxLocalScale)

    {
        // Start/refresh DOT
        fireDotActive = true;
        fireDotPctPerSec = Mathf.Max(0f, percentPerSec);                //This stores how much damage per second
        fireDotEndTime = Time.time + Mathf.Max(0f, durationSeconds);    //calculates when the burning effect should stop
        fireNextTickTime = Time.time + 1f; // tick every 1s

        // Attach/refresh VFX
        if (onEnemyVFXPrefab != null)
        {
            if (activeFireVFX == null)
            {
                activeFireVFX = Instantiate(onEnemyVFXPrefab, transform);
            }
            // enforce uniform transform every time we apply/refresh
            activeFireVFX.transform.localPosition = vfxLocalPos;
            activeFireVFX.transform.localRotation = Quaternion.Euler(vfxLocalEuler);
            activeFireVFX.transform.localScale = vfxLocalScale;
        //else
        //{
        //    // if already exists, you can refresh lifetime or leave it
        //}
        }
    }
    public void ApplyVenomInfusionEffect(float durationSeconds, float percentPerSec, GameObject onEnemyVFXPrefab, Vector3 vfxLocalPos, Vector3 vfxLocalEuler, Vector3 vfxLocalScale) 
    { 
        venomDotActive = true;
        venomDotPctPerSec = Mathf.Max(0f, percentPerSec);
        venomDotEndTime = Time.time + Mathf.Max(0f, durationSeconds);
        venomNextTickTime = Time.time + 1f; // tick every 1s

        if (onEnemyVFXPrefab != null)
        {
            if (activeVenomVFX == null)
            {
                activeVenomVFX = Instantiate(onEnemyVFXPrefab, transform);
            }
            // enforce a uniform transform every (re)apply
            activeVenomVFX.transform.localPosition = vfxLocalPos;
            activeVenomVFX.transform.localRotation = Quaternion.Euler(vfxLocalEuler);
            activeVenomVFX.transform.localScale = vfxLocalScale;
        }
    }



    // ---------------------------- HEALTH BAR ----------------------

    private void SetHealthUIVisible(bool visible)
    {
        if (_cg != null)
        {
            _cg.alpha = visible ? 1f : 0f;      // instantly show/hide
            _cg.blocksRaycasts = visible;       // optional
            _cg.interactable = visible;         // optional
        }
        else
        {
            if (healthCanvas) healthCanvas.enabled = visible;
            if (nameTagGO) nameTagGO.SetActive(visible);
        }
    }

    bool ShouldShowBar()
    {
        if (lookCam == null) return false;   //gtfo we dont have cam

        Vector3 focusPos = transform.position + lookAtOffset;   //focus point which is going to be head of the enemy 
        float dist = Vector3.Distance(lookCam.position, focusPos);   //get dist of camera to focus point
        if (dist > showMaxDistance) return false;           //if were outta range Gtfo this code

        // how centered? (1 = dead center, -1 = behind)
        Vector3 toEnemy = (focusPos - lookCam.position).normalized;  //direction 
        float dot = Vector3.Dot(lookCam.forward, toEnemy);            //Uses the dot product to check if you’re looking at the enemy.
        if (dot < centerDotThreshold) return false;

        if (requireLineOfSight)
        {
            // if something blocks the view, hide
            if (Physics.Raycast(lookCam.position, toEnemy, out RaycastHit hit, dist, occlusionMask))
            {
                // allow the enemy itself
                if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                    return false;
            }
        }
        return true;
    }

    //----------------------------- HIT DIE DAMAGE


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

    //-------------------------- RAGDOLL
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