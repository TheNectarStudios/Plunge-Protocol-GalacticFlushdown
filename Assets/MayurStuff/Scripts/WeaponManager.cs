using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject waterGun;
    public GameObject plungerAttack;

    private int activeWeapon = 0; // 0 = none, 1 = water gun, 2 = plunger

    void Start()
    {
        DeactivateAllWeapons(); // Start with nothing active
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ToggleWeapon(2);
        }
    }

    void ToggleWeapon(int weaponID)
    {
        if (activeWeapon == weaponID)
        {
            // Pressing same key again - deactivate
            DeactivateAllWeapons();
            activeWeapon = 0;
        }
        else
        {
            // Switching to new weapon
            ActivateWeapon(weaponID);
        }
    }

    void ActivateWeapon(int weaponID)
    {
        DeactivateAllWeapons(); // Make sure only one is active

        switch (weaponID)
        {
            case 1:
                waterGun.SetActive(true);
                break;
            case 2:
                plungerAttack.SetActive(true);
                break;
        }

        activeWeapon = weaponID;
    }

    void DeactivateAllWeapons()
    {
        waterGun.SetActive(false);
        plungerAttack.SetActive(false);
    }
}
