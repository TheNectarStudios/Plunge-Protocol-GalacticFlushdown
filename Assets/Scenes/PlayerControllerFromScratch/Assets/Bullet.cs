using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Impact Settings")]
    public GameObject impactEffect; // Particle effect prefab
    public float lifeTime = 5f;

    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        if (impactEffect != null && collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.LookRotation(contact.normal);
            GameObject effect = Instantiate(impactEffect, contact.point, rot);
            Destroy(effect, 2f);
        }

        Destroy(gameObject, 0.1f);
    }
}
