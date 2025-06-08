using System.Collections.Generic;
using UnityEngine;

public class TPWhirlpoolProjectile : MonoBehaviour
{
    public float spiralSpeed = 5f;
    public float radialSpeed = 1f;
    public float lifeTime = 5f;
    public float tailUpdateInterval = 0.05f;
    public int maxTailPoints = 100;

    private Vector3 origin;
    private float timeElapsed = 0f;
    private float tailTimer = 0f;
    private TPWhirlpoolSpawner spawner;
    private LineRenderer lineRenderer;

    private List<Vector3> tailPoints = new List<Vector3>();

    void Start()
    {
        origin = transform.position;
        spawner = FindObjectOfType<TPWhirlpoolSpawner>();
        lineRenderer = GetComponent<LineRenderer>();

        tailPoints.Clear();
        tailPoints.Add(transform.position);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        tailTimer += Time.deltaTime;

        // Spiral movement
        float angle = timeElapsed * spiralSpeed;
        float radius = timeElapsed * radialSpeed;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = origin + new Vector3(x, 0f, z);

        // Update tail trail
        if (tailTimer >= tailUpdateInterval)
        {
            tailTimer = 0f;
            tailPoints.Add(transform.position);

            if (tailPoints.Count > maxTailPoints)
                tailPoints.RemoveAt(0); // Keep the tail length fixed

            lineRenderer.positionCount = tailPoints.Count;
            lineRenderer.SetPositions(tailPoints.ToArray());
        }

        // Lifetime check
        if (timeElapsed > lifeTime)
        {
            if (spawner != null) spawner.ClearTP();
            Destroy(gameObject);
        }
    }
}
