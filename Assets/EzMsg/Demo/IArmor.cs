using System.Collections;
using UnityEngine.EventSystems;

public interface IArmor : IEventSystemHandler
{
	// Methods that will be called via the messaging system should return IEnumerable
	IEnumerable ApplyDamage(int Damage);
    IEnumerable DecreaseArmor(float Percentage);
    IEnumerable IncreaseArmor(float Percentage);

    int GetHealth();
}
