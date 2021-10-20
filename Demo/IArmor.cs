namespace Ez.Msg.Demos {
	
using System.Collections;
using UnityEngine.EventSystems;

public interface IArmor : IEventSystemHandler
{
	// Methods that will be called via Send should return IEnumerable
	IEnumerable ApplyDamage(int Damage);
    IEnumerable DecreaseArmor(float Percentage);
    IEnumerable IncreaseArmor(float Percentage);

    // Methods to be called by Request may return anything, but can't be chained or paused
    int? GetHealth();

    bool? IsDestructible();
    bool IsDestructibleNonNullable();

}

}
