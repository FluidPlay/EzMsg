namespace Ez.Msg.Demos
{
using UnityEngine;
using Ez;
public class Projectile : MonoBehaviour {

	public int Damage = 10;

    // When a message or request takes a parameter, we may use a property to define the message and shorten its usage code
    private EzMsg.EventAction<IArmor> ApplyDamageMsg {
        get {return _=>_.ApplyDamage(Damage);}
    }

    // If a message or request takes no parameter, we may simply declare it as static
    public static EzMsg.EventFunc<IArmor,int?> GetArmorHealth = _=>_.GetHealth();


	public void OnTriggerEnter(Collider other) {
	    //### Request an int value from a GetHealth method from any gameObject's component implementing IArmor

//      * Inline format, no previous declaration required
//	    var health = EzMsg.Request<IArmor, int>(other.gameObject, _=>_.GetHealth());

//      * Shorthand form with Predefined Request
//	    var health = EzMsg.Request(other.gameObject, GetArmorHealth);

//      * Shorthand/Extension form from gameObject
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

//	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage));


//	    * Different ways to perform a Request. Use nullable primitive types (bool?, int?, etc) to check
//        if Response was obtained (result != null).
//	    int? h1 = EzMsg.Request<IArmor, int?>(other.gameObject, _=>_.GetHealth());

//	    * Please notice the shorthand form (extension to GameObject) doesn't allow chaining
//	    var h2 = other.gameObject.Request<IArmor, int?>(_=>_.GetHealth());

//      * Pre-initialized Request. In this case, we will not send the request to the children of target
//	    var h3 = other.gameObject.Request(GetArmorHealth, false);
//	    Debug.Log("Health found: "+h3+" Time: "+Time.time);

//	    bool? validTarget = EzMsg.Request<IArmor, bool?>(other.gameObject, _=>_.IsDestructible(), true);
//	    Debug.Log(validTarget == null ? "No valid target found." : "Is target valid ? "+validTarget );

//	    bool validTarget = EzMsg.Request<IArmor, bool>(other.gameObject, _=>_.IsDestructibleNonNullable(), true, true);
//	    Debug.Log("Is target valid ? "+validTarget );

//	    EzMsg.Send<IArmor>(other.gameObject, _=>_.ApplyDamage(Damage)).Run();
//	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage));


	    // Note: Currently, a EzMsgManager component is required in the scene for any wait-chained commands.

	    EzMsg.Send<IArmor>(other.gameObject, _ => _.ApplyDamage(Damage))
	        .Wait(2f)
	        .Send<IWeapon>(gameObject, _=>_.Reload())
	        .Run();

//      * Send's Shorthand form is non-chainable, but executes immediately..
//	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage));

//	    * ..yet Wait's shorthand form is chainable. Yes, you may start with a gameObject.Wait(0f)
//	    other.gameObject.Wait(4f).Send<IWeapon>(gameObject, _=>_.Reload())
//	        .Run();

	}

}

}

