using UnityEngine;
using Ez.Msg;

public class Projectile : MonoBehaviour {

	public int Damage = 10;

    // When a message or request takes a parameter, we may use a property to define the message and shorten its usage code
    private EzMsg.EventAction<IArmor> ApplyDamageMsg {
        get {return _=>_.ApplyDamage(Damage);}
    }

    // If a message or request takes no parameter, we may simply declare it as static
    public static EzMsg.EventFunc<IArmor,int> GetArmorHealth;


	public void OnTriggerEnter(Collider other) {
	    //### Request an int value from a GetHealth method from any gameObject's component implementing IArmor

//      * Inline format, no previous declaration required
//	    var health = EzMsg.Request<IArmor, int>(other.gameObject, _=>_.GetHealth());

//      * Short form with Predefined Request
//	    var health = EzMsg.Request(other.gameObject, GetArmorHealth);

//      * Extension form from gameObject
//	    var health = other.gameObject.Request(GetArmorHealth);
//
//	    Debug.Log("(Projectile) Armor Health found: " + health);

	    //### Sends an ApplyDamage(Damage) message to all gameObject's components implementing IArmor

//      * Original 'ExecuteEvents' format:
//      ExecuteEvents.Execute<IArmor>(other.gameObject, null, (x,y)=>x.ApplyDamage(Damage));

//      * Inline format, no previous declaration required
//		EzMsg.Send<IArmor>(other.gameObject, _=>_.ApplyDamage(Damage)).Run();

//      * Shorthand form with Predefined Request.
//	    Generic type <> doesn' need to be defined since ApplyDamageMsg already did it.
//	    Note that this can't be chained, but auto-runs
//	    other.gameObject.Send(ApplyDamageMsg);


//	    * Three different ways to perform a Request
//	    int h1 = EzMsg.Request<IArmor, int>(other.gameObject, _=>_.GetHealth());
//	    int h2 = other.gameObject.Request<IArmor, int>(_=>_.GetHealth());
//	    int h3 = other.gameObject.Request(GetArmorHealth);


//	    EzMsg.Send<IArmor>(other.gameObject, _=>_.ApplyDamage(Damage)).Run();
//	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage));


	    // Note: Currently, a EzMsgManager component scene is required in the scene to hold the multiple coroutines.

	    EzMsg.Send<IArmor>(other.gameObject, _=>_.ApplyDamage(Damage))
	        .Wait(2f)
	        .Send<IWeapon>(gameObject, _=>_.Reload())
	        .Run();

//      * Send's Shorthand form is non-chainable..
//	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage));

//	    * ..but Wait's shorthand form is chainable. Yes, you may start with a gameObject.Wait(0f)
//	    other.gameObject.Wait(4f).Send<IWeapon>(gameObject, _=>_.Reload())
//	        .Run();

	}

}

