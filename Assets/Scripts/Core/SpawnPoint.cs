using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoints = new();

    public static Vector3 GetRandomSpawnPos() => spawnPoints.Count > 1 ?
        spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position :
        Vector3.zero;

    private void OnEnable()
    {
        spawnPoints.Add(this);
    }

    private void OnDisable()
    {
        spawnPoints.Remove(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
