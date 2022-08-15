using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLibrary : MonoBehaviour
{
    public Weapon[] allWeapons;
    public static Weapon[] weapons;

    private void Awake()
    {
        weapons = allWeapons;
    }

    public static Weapon FindWeapon(string name)
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon.GetWeaponData().weaponName.Equals(name)) return weapon;
        }

        return weapons[0];
    }
}
