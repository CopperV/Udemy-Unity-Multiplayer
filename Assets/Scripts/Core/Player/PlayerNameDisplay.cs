using System;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField]
    private TankPlayer player;

    [SerializeField]
    private TMP_Text displayNameText;

    private void Start()
    {
        player.playerName.OnValueChanged += OnNameChanged;

        UpdateDisplay(player.playerName.Value.ToString());
    }

    private void OnDestroy()
    {
        player.playerName.OnValueChanged -= OnNameChanged;
    }

    private void OnNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue) => UpdateDisplay(newValue.ToString());

    private void UpdateDisplay(string name) => displayNameText.text = name;
}
