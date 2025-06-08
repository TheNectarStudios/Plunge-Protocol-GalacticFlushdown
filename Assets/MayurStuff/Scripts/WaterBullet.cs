using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterBullet : MonoBehaviour
{
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime); // Always clean it up after 5 seconds
    }
}
