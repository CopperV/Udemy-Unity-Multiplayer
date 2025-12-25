using UnityEngine;

public class LifeTime : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
