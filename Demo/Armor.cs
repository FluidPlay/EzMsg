namespace Ez.Msg.Demos { 

using UnityEngine;
using System.Collections;

public class Armor : MonoBehaviour, IArmor {
    
	public int Health = 100;

    public bool Destructable = true;

	public IEnumerable ApplyDamage(int Damage) {
		Health -= Damage;
		Debug.Log("(Armor) New Health: "+Health+" ; Time: "+Time.time);
	    yield break;
	}

    public int? GetHealth() {
        return Health;
    }

    public bool? IsDestructible() {
        return Destructable;
    }

    public bool IsDestructibleNonNullable()
    {
        return Destructable;
    }

    public IEnumerable DecreaseArmor(float Percentage) { yield break; }

    public IEnumerable IncreaseArmor(float Percentage) { yield break; }
}

}
