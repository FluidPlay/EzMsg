# EzMsg for Unity3D

Decoupled, dynamic and type-safe Messaging System for Unity3D

`v0.92Beta - May 11, 2017 - Updated some example code, added Quickstart info to the readme`
	
`v0.91Beta - Jan 20, 2017 - Added compatibility to Unity 5.0 onwards`
	
`v0.9Beta - Jan 20, 2017 - New version's public release`
	

`Disclaimer: This is a Beta release. All intended functionality is in place and basic testing was performed, but no guarantees are provided about its functionality - so use it at your own risk. At the same time, by using it and reporting bugs you'll help me shaping this up to be the best Unity messaging system.`

## E-Z What? 

EzMsg (pronounced "Easy Message") is a decoupled, dynamic, type-safe Messaging System for the Unity3D game engine (www.unity3d.com). 

It's based off the innards of Unity's native "ExecuteEvents", meaning it relies on native data types and systems as much as possible, while adding a layer of abstraction and simplification.

EzMsg is interface-based, so it allows you to send messages and requests to methods defined in any interface implementing IEventSystemHandler. It supports fluid notation and command chaining/sequencing, so you have full and easy control of the exact sequence the messages should follow, and wait until their consumers are done.

## Why shouldn't I use another Messaging system? 

There are two forms of messaging, static and dynamic. If you check UnityEvents you'll notice there's a dropdown where you can define how you want it to behave. *Static* refers to messaging targeting editor-defined elements in your scene, eg.: a GameManager sending a message to the Player or an NPC ship warning the GameManager that it was destroyed. This target exists before the game runs, so it's trivial to find its reference (more on that later). If all you want is this kind of messaging in code, you can use native C# events or an existing 'observer pattern' helper framework - I recommend Will Miller's: http://www.willrmiller.com/?p=87

Now, what should you do when you want to send a message *to the NPC ship*? It didn't exist in editor-time, since it was instantiated (spawned) during the game. The standard Unity approach would be, after getting the NPC GameObject reference from a native Unity callback, usually `OnTriggerEnter`, to get a hold of its target component and execute a method there. Something like `gameObjectHit.GetComponent<Armor>().ApplyDamage(damage);`

The problem is: you've just had to deal with the Armor class' inner structure. Your code is no longer decoupled. "Why is that important?", you might guess. Without turning this into a long essay defending 'decoupling' (there are many online), let's just say it's way more reusable and convenient to just call methods defined in an Interface. First because a message may or may not be consumed by the receiver, it won't break your game (due to a Null Reference error) in any case, minimizing the need for error handling. Second because when you may have a component with multiple interfaces defined and it'll still take that message. For instance, you could have a component like this:

```c#
	public class Vessel: MonoBehaviour, IArmor, ISpell {}
```

This will take all the messages defined in IArmor and ISpell. You could have multiple components mixing and matching interfaces as needed. Abstract classes aren't as flexible, you can inherit from only one, so their concret classes aren't composable.

> PS.: Talking of references, make sure to learn about Dependency Injection frameworks. I recommend Syring (as a "let-me-try-this" entry drug) and especially Zenject when you wrap your mind around DI/IoC advantages and are ready to get serious on the topic.
		
## Quick Start Guide (aka TL;DR)

1. Create an interface with all methods of a certain type you want to run, make it implement `IEventSystemHandler`. All methods callable by a EzMsg should return the `IEnumerable` type. Eg.:

```c#
	using System.Collections;
	using UnityEngine.EventSystems;

	public interface IWeapon: IEventSystemHandler 
	{
		IEnumerable Reload();
		IEnumerable Fire();
	}
```
	
2. Implement the created interface in all classes you need to have that methods/event run. Remember classes may implement multiple interfaces without a problem. Eg.:
	
```c#
	public class Weapon: MonoBehaviour, IWeapon {
		public IEnumerable Reload() { Debug.Log("Reload called"); yield return null; }
		public IEnumerable Fire() { Debug.Log("Fire called"); yield return null; }		
	}
```
	
3. To fire the event on a target gameObject you may use the standard or shorthand notations:
	
  * Shorthand	
```c#
	other.gameObject.Send<IArmor> (_=>_.ApplyDamage(Damage));	// This form doesn't allow pause or wait
```
			
  * Standard
```c#
	EzMsg.Send<IArmor> (other.gameObject, _=>_.ApplyDamage(Damage))
	 	 .wait(2f)					// Waits 2s after the ApplyDamage method is completed
		 .Send<IWeapon>(gameObject, _=>_.Reload())	// then sends the reload message to my owner gameObject
		 .Run();					// Fires immediately. Could be stored and ran later.
```

## Can I use EzMsg to send static messages?

Sure, just make your static component implement a specific message target interface and you're good to go. To make this even easier and way more reliable than by drag-and-dropping things in the Unity IDE, I recommend a DI framework. I've done this short video showing how to set things up in Zenject: https://www.youtube.com/watch?v=uyN9KYvlgCQ


## I know what Sending a message means, but what's a request?

In some circumstances you need to retrieve a value from a method, the decoupled way to do that is by sending the method a request. Follows usage examples, the first using the standard notation followed by the shorthand (GameObject extension) notation:
		```c#
		int h1 = EzMsg.Request<IArmor, int>(other.gameObject, _=>_.GetHealth());
	    int h2 = other.gameObject.Request<IArmor, int>(_=>_.GetHealth());
		```

Methods which reply to a request may return any type in their interface signature, but since they don't return an IEnumerable type these methods can't be paused or sequenced. Chain-able requests are in EzMsg’s wish list, but there are a number of design considerations to work out first. Meanwhile, if you really need chain-able requests you may try sending an Action to update a field in the caller method, using a feature called “closure”. You can read more about it here:
http://stackoverflow.com/questions/999020/why-cant-iterator-methods-take-either-ref-or-out-parameters

		
## What's this `_=>_` thing, is that a smiley?

That's standard C#'s lambda notation. Lambdas define anonymous delegates, which are basically pointers to methods which also hold a state. The Lambda notation expresses that whatever's on the left side "goes to" whatever's at the right side, since you need an identifier to work with. More tipically you'll see things like `x=>x.method()`in examples online, but personally I feel any letter used before a one-liner method call on a class already defined by generics doesn’t add any meaning. It’s like not using ‘var’ to infer the type of what’s an obvious type at the right of the equal sign. Instead of clarifying, it adds "cognitive weight" to the instruction, in practice making it harder to read. An underscore in these cases makes it clear that it’s “bypassing” the Generics type already provided (what’s between < and >). There’s no absolute right or wrong here, since the compiler will gladly accept any valid parameter identifier, so free to write `armor=>armor.GetHealth()` or whatever other format you prefer.

## I don't like those lambda smiley thingies in my code, can I "hide" it somehow?

Yes, you can. Check the beginning of the included Projectile.cs script to learn how to store an EventAction (used by Send) and EventFunc (used by Request) as static fields or local properties. Personally I only use this approach when I'm gonna be calling the same message many times in the same script.

## More details on usage?

The easiest way to learn how to use EzMsg is by checking on the included EzMsg_tst scene, especially Projectile.cs and Armor.cs. For a new scene, take the following steps:

1. Add the EzMsgManager component to any existing GameObject in your scene (could be the Main Camera). This instance is required to host coroutines and keep track of the execution of multiple messages. If you don’t do this step, once a chained Send message is received by the system you’ll get a warning, yet an EzMsgManager game object and component will be added automatically to the scene. 
2. Create a new C# class to define one or more receiving message's interfaces. The script must include System.Collections and UnityEngine.EventSystems, and each defined interface must implement IEventSystemHandler.
3. All method signatures defined in the interface must return type IEnumerable (and not Void), exception being methods to be called by EzMsg.Request
4. The MonoBehaviour script which will send messages must include Ez (add `using Ez;` at the top of the script) 
5. Now your script is ready to send a dynamic message from a certain interface type to another object. Make sure to add .Run() at the end of a chained Send command if you want it to start being processed immediately. Requests are always executed immediately, just as the extension (shorthand) calls, and as such don’t require (and won’t admit) .Run() after them.

## How do I send messages and requests to all children of a target GameObject?

By default both Send and Request instructions are sent to the target game object first and, if applicable, to all its children. If that’s not what you want simply set the ‘sendToChildren’ final parameter in the calls to false.

## Are messages sent to inactive GameObjects in a hierarchy?

No, neither Send nor Requests are sent to inactive objects. There’s no parameter to override that, and I would advise against it, but if you really want it just search for “Include Inactive” in EzMsg.cs to change that behaviour.

## How can I fire a "message method" within the same class?

You can either use a send to the "host" gameObject (the gameObject which contains this component instance), like so:
		```gameObject.Send<IWeapon>(_=>_.Fire());```

or, which is obviously preferrable for performance reasons - and the good old "KISS" phylosophy - start the coroutine from its Enumerator (those methods return Enumerable instead of Enumerator, as required by StartCoroutine), like this:
		```StartCoroutine(Fire().GetEnumerator());```
	
## How can I sequence messages?

EzMsg was designed from the ground up to provide a natural, fluid coding style. Just "chain" your initial Send command by other 'Wait' or 'Send' commands. They will be executed in order, always waiting for the completion of the previous one. You may start with a `Wait` as well if you want, just bear in mind that only the starting `Send` might use the shorthand form. Eg.:
		```c#
	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage))
	        .Wait(2f)
	        .Send<IWeapon>(gameObject, _=>_.Reload())
	        .Run();
		```		
			
Let's break down this instruction. We're asking EzMsg to send an 'ApplyDamage' message, taking the Damage parameter, to any appropriate receiver in other.gameObject. To be a valid receiver, the component should implement the IArmor interface, which has the `IEnumerable ApplyDamage(int);` signature defined. Once matched dynamically, the method is executed and *only after its completion* the execution flow returns to the original message instruction.

If multiple components in gameObject (or one of its children, there's an optional parameter for that) implement IArmor, with different execution times, all of the methods will be started at once and *only after all of them have finished* the Send instruction will resume. When that happens, it'll wait for 2 seconds and Send another message, this time IWeapon.Reload(), to any component in the current (host) gameObject which implements IWeapon.

If there's no such candidate, nothing will happen - and be warned that no error will be raised, for best and worse. As soon as `SendSeqData` is defined (that's what all chainable EzMsg methods return), it's executed by Run(). Note that this is a lazy evaluation structure, so you may store it in a variable and execute it at a later time.

Also, the fact that any method receiving a 'Send' message should return IEnumerable allows you to use all the regular coroutine toolset provided by Unity (like 'yield return new WaitForSeconds(x);'), within the called method block, to control the execution timing. Keep that in mind when defining your overall logic flow.

For more examples, make sure to check the included demo scene and scripts.

Enjoy!

- Breno Azevedo (@brenoazevedo)
@FluidPlay
