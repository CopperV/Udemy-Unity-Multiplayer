using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private Transform turretTransform;

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        var mousePos = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        turretTransform.up = new Vector2(mousePos.x - turretTransform.position.x, mousePos.y - turretTransform.position.y);
    }
}
