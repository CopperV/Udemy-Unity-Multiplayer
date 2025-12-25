using System;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [SerializeField]
    private GameObject muzzleFlash;

    [SerializeField]
    private Collider2D playerCollider;

    [SerializeField]
    private CoinWallet wallet;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject serverProjectilePrefab;

    [SerializeField]
    private GameObject clientProjectilePrefab;

    [Header("Prefabs")]
    [SerializeField]
    private float projectileSpeed = 12f;

    [SerializeField]
    private float fireRate = 1.5f;

    [SerializeField]
    private float muzzleFlashDuration = 0.5f;

    [SerializeField]
    private int costToFire = 1;

    private bool isFiring = false;
    private float muzzleFlashTimer;
    private float timer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            return;

        inputReader.PrimaryFireEvent += OnFire;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner)
            return;

        inputReader.PrimaryFireEvent -= OnFire;
    }

    private void Update()
    {
        if(muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;

            if(muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner || !isFiring)
            return;

        if (wallet.TotalCoins.Value < costToFire)
            return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);

        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        timer = 1 / fireRate;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (wallet.TotalCoins.Value < costToFire)
            return;

        wallet.SpendCoins(costToFire);

        var projectile = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);
        projectile.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

        if(projectile.TryGetComponent(out DealDamageOnContact dealDamageOnContact))
        {
            dealDamageOnContact.SetOwner(OwnerClientId);
        }

        if (projectile.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner)
            return;

        SpawnDummyProjectile(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        var projectile = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
        projectile.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

        if(projectile.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
    }

    private void OnFire(bool state) => isFiring = state;
}
