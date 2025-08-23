using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public float damage = 1f;
   
    
    public GameObject[] bloodEffects;
    public GameObject groundHitEffect;
    public LayerMask layersToIgnore;

    public Weapon sourceWeapon;  // set by Weapon.Shoot()

    Rigidbody rb;
    Collider myCol;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();
    }
    void Start()
    {
        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);

        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();  // we use this to make sure it does not hit us 

        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }
    }

    //void Update()
    //{
    //    transform.position += transform.forward * speed * Time.deltaTime;
    //}

    void OnTriggerEnter(Collider other)
    {
        {
            if ((layersToIgnore.value & (1 << other.gameObject.layer)) != 0)
                return;
        }

        // Ground hit
        if (other.CompareTag("Ground"))
        {
            if (groundHitEffect)
                Instantiate(groundHitEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
            return;
        }

        // Only do blood + damage if we actually hit an enemy
        var enemy = other.GetComponentInParent<EnemyHealthRagdoll>();  // safer than tag
        if (enemy != null)
        {
            if (PointManager.Instance != null)
                PointManager.Instance.AddPoints(5);

            if (bloodEffects != null && bloodEffects.Length > 0)
            {
                int index = Random.Range(0, bloodEffects.Length);
                var blood = Instantiate(bloodEffects[index], transform.position, Quaternion.identity);
                blood.transform.SetParent(enemy.transform, true);
            }

            Vector3 dir = transform.forward;
            enemy.TakeDamage(damage, dir);

            //Fire infusion DOT logic
            if (sourceWeapon != null && sourceWeapon.infusion == InfusionType.Fire)
            {
                enemy.ApplyFireInfusionEffect( sourceWeapon.fireDotDuration, sourceWeapon.fireDotPercentPerSec, sourceWeapon.fireOnEnemyVFXPrefab, 
                    sourceWeapon.fireOnEnemyVFXOffset, sourceWeapon.fireOnEnemyVFXEuler, sourceWeapon.fireOnEnemyVFXScale
                );
            }

            // ☠️ Venom DOT + VFX
            if (sourceWeapon != null && sourceWeapon.infusion == InfusionType.Venom)
            {
                enemy.ApplyVenomInfusionEffect( sourceWeapon.venomDotDuration, sourceWeapon.venomDotPercentPerSec, sourceWeapon.venomOnEnemyVFXPrefab,
                                        sourceWeapon.venomOnEnemyVFXOffset, sourceWeapon.venomOnEnemyVFXEuler, sourceWeapon.venomOnEnemyVFXScale
                );
            }

        }

        Destroy(gameObject);
    }
}