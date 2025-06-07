using UnityEngine;

public class PlungerAttack : MonoBehaviour
{
    public Transform topPlunger;
    public Transform rightPlunger;
    public Transform leftPlunger;

    public GameObject plungerProjectilePrefab;
    public float plungerSpeed = 20f;

    private int plungerIndex = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // LMB
        {
            FireNextPlunger();
        }
    }

    void FireNextPlunger()
    {
        Transform firePoint = GetCurrentPlungerTransform();

        if (firePoint == null || plungerProjectilePrefab == null)
        {
            Debug.LogWarning("Missing plunger firePoint or prefab!");
            return;
        }

        GameObject projectile = Instantiate(plungerProjectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = firePoint.forward * plungerSpeed;
        }

        Destroy(projectile, 5f); // Cleanup

        // Move to next plunger
        plungerIndex = (plungerIndex + 1) % 3;
    }

    Transform GetCurrentPlungerTransform()
    {
        switch (plungerIndex)
        {
            case 0: return topPlunger;
            case 1: return rightPlunger;
            case 2: return leftPlunger;
            default: return topPlunger;
        }
    }
}
