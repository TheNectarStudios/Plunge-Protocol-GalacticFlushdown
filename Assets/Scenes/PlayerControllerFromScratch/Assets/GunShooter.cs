using UnityEngine;
using System.Collections;

public class GunShooter : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 700f;
    public float shootCooldown = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("Animator")]
    public Animator animator;

    private float nextShootTime = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootCooldown;
        }
    }

    void Shoot()
    {
        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce);
        }

        // Play shoot sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Trigger shoot animation
        if (animator != null)
        {
            animator.SetBool("Shoot", true);
            StartCoroutine(ResetShootAnimation());
        }
    }

    IEnumerator ResetShootAnimation()
    {
        yield return new WaitForSeconds(0.17f);
        if (animator != null)
        {
            animator.SetBool("Shoot", false);
        }
    }
}
