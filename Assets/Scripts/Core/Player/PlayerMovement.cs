using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private Transform bodyTransform;

    [SerializeField]
    private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField]
    private float movementSpeed = 4f;

    [SerializeField]
    private float turningRate = 30f;

    private Vector2 previousMoveInput;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            return;

        inputReader.MoveEvent += OnMove;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner)
            return;

        inputReader.MoveEvent -= OnMove;
    }

    private void Update()
    {
        if(!IsOwner)
            return;

        float zRotation = previousMoveInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        rb.linearVelocity = (Vector2) bodyTransform.up * previousMoveInput.y * movementSpeed;
    }

    private void OnMove(Vector2 moveInput) => previousMoveInput = moveInput;
}
