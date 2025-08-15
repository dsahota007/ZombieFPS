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

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        SetRagdollState(false);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerBody"), LayerMask.NameToLayer("DeadBody"));   // Ignore collisions between PlayerBody and DeadBody layers
        
        currentHealth = Health;

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