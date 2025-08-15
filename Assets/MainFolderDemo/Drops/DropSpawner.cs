using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [Range(0f, 1f)] public float dropChance = 0.15f;
    public GameObject[] pickupPrefabs; // each has DropPickup + trigger collider

    public void TrySpawnDrop(Vector3 where)
    {
        if (pickupPrefabs == null || pickupPrefabs.Length == 0) return;
        if (Random.value > dropChance) return;

        var prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
        Instantiate(prefab, where + Vector3.up * 0.5f, Quaternion.identity);
    }
}
