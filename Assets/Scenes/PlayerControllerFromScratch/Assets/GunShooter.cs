using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunShooter : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 700f;
    public float shootCooldown = 0.5f;

    [Header("Special Move Settings")]
    public GameObject plungerPrefab;
    public List<Transform> plungerFirePoints; // Assign 8 points in Inspector
    public float plungerForce = 500f;         // Separate force for plungers
    public float specialCooldown = 8f;
    public AudioClip specialSound;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("Animator")]
    public Animator animator;

    private float nextShootTime = 0f;
    private float nextSpecialTime = 0f;
    private bool isAttacking = false;

    void Update()
    {
        if (isAttacking) return;

        if (Input.GetButton("Fire1") && Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootCooldown;
        }

        if (Input.GetButtonDown("Fire2") && Time.time >= nextSpecialTime)
        {
            StartCoroutine(PerformSpecialMove());
            nextSpecialTime = Time.time + specialCooldown;
        }
    }

    void Shoot()
    {
        if (isAttacking) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * bulletForce);
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

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

    IEnumerator PerformSpecialMove()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("Alt", true);
        }

        if (audioSource != null && specialSound != null)
        {
            audioSource.PlayOneShot(specialSound);
        }

        float startTime = Time.time;
        float duration = 1.3f;
        float fireRate = 0.1f;

        List<Transform> firePoints = new List<Transform>(plungerFirePoints);

        while (Time.time - startTime < duration)
        {
            if (firePoints.Count > 0)
            {
                Transform fireFrom = firePoints[Random.Range(0, firePoints.Count)];

                for (int i = 0; i < 3; i++)
                {
                    GameObject plunge = Instantiate(plungerPrefab, fireFrom.position, fireFrom.rotation);
                    Rigidbody rb = plunge.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 randomDir = fireFrom.forward + Random.insideUnitSphere * 0.2f;
                        rb.AddForce(randomDir.normalized * plungerForce, ForceMode.Impulse);
                    }
                }
            }

            yield return new WaitForSeconds(fireRate);
        }

        if (animator != null)
        {
            animator.SetBool("Alt", false);
        }

        isAttacking = false;
    }
}
