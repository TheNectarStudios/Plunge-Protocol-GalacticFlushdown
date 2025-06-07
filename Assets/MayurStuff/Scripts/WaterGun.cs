using UnityEngine;

public class WaterGun : MonoBehaviour
{
    public GameObject waterBulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.3f;
    public float bulletSpeed = 25f;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (waterBulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missing waterBulletPrefab or firePoint!");
            return;
        }

        GameObject bullet = Instantiate(waterBulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Water bullet prefab is missing Rigidbody!");
            return;
        }

        rb.velocity = firePoint.forward * bulletSpeed;

        Destroy(bullet, 5f); // Bullet auto-cleans in 5 seconds
    }
}
