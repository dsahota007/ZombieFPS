using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public GameObject[] zombiePrefabs;         

    [Header("Spawn Points")]
    public Transform[] spawnPoints;            

    [Header("Round Settings")]
    public int startZombies = 6;
    public int zombiesPerRound = 3;
    private int currentRound = 1;
    private int zombiesRemaining;

    void Start()
    {
        StartCoroutine(SpawnRound()); 
    }

    IEnumerator SpawnRound()
    {
        int zombiesToSpawn = startZombies + (zombiesPerRound * (currentRound - 1));
        zombiesRemaining = zombiesToSpawn;     //how many are alive

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
            yield return new WaitForSeconds(1.5f);  // small delay between spawns
        }
    }

    void SpawnZombie()
    {
        GameObject zombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];  //random zomb
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];       //random spawn
        GameObject newZombie = Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyHealthRagdoll healthScript = newZombie.GetComponent<EnemyHealthRagdoll>();
        if (healthScript != null)
        {
            int healthBoost = 20 * (currentRound - 1);
            healthScript.SetHealth(healthScript.Health + healthBoost);
        }
    }

    public void OnZombieKilled()    //we gon put this in the die function so we can decrement zombie counter
    {
        zombiesRemaining--;

        if (zombiesRemaining <= 0)
        {
            currentRound++;
            Debug.Log("Next round: " + currentRound);
            StartCoroutine(SpawnRound());
        }
    }

    public int GetCurrentRound()   //for ui just fetching the round
    {
        return currentRound;
    }

}
