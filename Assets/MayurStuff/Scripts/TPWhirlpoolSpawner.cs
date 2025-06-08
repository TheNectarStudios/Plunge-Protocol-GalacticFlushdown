using UnityEngine;

public class TPWhirlpoolSpawner : MonoBehaviour
{
    public GameObject tpWhirlpoolPrefab;
    public Transform firePoint;

    private GameObject currentTP;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentTP == null)
        {
            Debug.Log("LMB Clicked: Spawning TP");

            currentTP = Instantiate(tpWhirlpoolPrefab, firePoint.position, Quaternion.identity);
        }
    }

    public void ClearTP()
    {
        currentTP = null;
    }
}
