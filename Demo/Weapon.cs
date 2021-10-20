namespace Ez.Msg.Demos
{
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon {
    public IEnumerable Reload()
    {
        Debug.Log("(Weapon) Reload! ; Time: "+Time.time);
        yield return null;
    }

    public IEnumerable Fire()
    {
        Debug.Log("Fire!");
        yield return null;
    }
}

}