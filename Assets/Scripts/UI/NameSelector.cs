using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    public const string playerNameKey = "PlayerName";

    [SerializeField]
    private TMP_InputField nameField;

    [SerializeField]
    private Button connectButton;

    [SerializeField]
    private int minNameLength = 1;

    [SerializeField]
    private int maxNameLength = 12;

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            LoadNextScene();
            return;
        }

        nameField.text = PlayerPrefs.GetString(playerNameKey, string.Empty);

        HandleNameChanged();
    }

    public void HandleNameChanged()
    {
        connectButton.interactable = nameField.text.Length >= minNameLength && nameField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        PlayerPrefs.SetString(playerNameKey, nameField.text);

        LoadNextScene();
    }

    private void LoadNextScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
}
