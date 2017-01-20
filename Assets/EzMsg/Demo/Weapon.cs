using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon {
    public IEnumerable Reload()
    {
        Debug.Log("(Weapon) Reload! ; Time: "+Time.time);
        yield return null;
    }

    public void Fire()
    {
        throw new System.NotImplementedException();
    }
}
