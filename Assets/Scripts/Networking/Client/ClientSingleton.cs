using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    public ClientGameManager GameManager { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<ClientSingleton>();

            if (instance == null)
                Debug.LogError("No ClientSingleton in the scene!");

            return instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }

    public async Task<bool> CreateClient()
    {
        GameManager = new();

        return await GameManager.InitAsync();
    }
}
