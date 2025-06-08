using UnityEngine;

public class PoopProjectile : MonoBehaviour
{
    [Header("VFX")]
    public GameObject impactEffect; // Assign a particle prefab in Inspector

    private bool hasExploded = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        // Spawn the effect at the contact point
        if (impactEffect != null)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.LookRotation(contact.normal);
            Vector3 pos = contact.point;

            Instantiate(impactEffect, pos, rot);
        }

        Destroy(gameObject);
    }
}
