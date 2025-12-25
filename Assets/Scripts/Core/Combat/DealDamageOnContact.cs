using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField]
    private int damage = 5;

    private ulong ownerClientId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null || !collision.attachedRigidbody.TryGetComponent(out Health health))
            return;

        if (collision.attachedRigidbody.TryGetComponent(out NetworkObject netObj) && netObj.OwnerClientId == ownerClientId)
            return;

        health.TakeDamage(damage);
    }

    public void SetOwner(ulong ownerClientId) => this.ownerClientId = ownerClientId;
}
