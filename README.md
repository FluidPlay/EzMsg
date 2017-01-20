# EzMsg for Unity3D

Decoupled, dynamic and type-safe Messaging System for Unity3D

v0.9Beta - Jan 20th, 2017

Disclaimer: This is a Beta release. All intended functionality is in place and basic testing was performed, but no guarantees are provided about its functionality - so use it at your own risk. At the same time, by using it and reporting bugs you'll help me shaping this up to be the best Unity messaging system.

Version: 0.9, new version's public release.

EzMsg (pronounced "Easy Message") is a decoupled, dynamic, type-safe Messaging System for the Unity3D game engine (www.unity3d.com). 

It's based off the innards of Unity's native "ExecuteEvents", meaning it relies on native data types and systems as much as possible, while adding a layer of abstraction and simplification.

EzMsg is interface-based, so it allows you to send messages and requests to methods defined in any interface implementing IEventSystemHandler. It supports fluid notation and command chaining/sequencing, so you have full and easy control of the sequence messages should follow and wait until their consumers are done take.

# Why shouldn't I use another Messaging system? 

There are two forms of messaging, static and dynamic. If you check UnityEvents you'll notice there's a dropdown where you can define how you want it to behave. *Static* refers to messaging targetting editor-defined elements in your scene, eg.: a GameManager sending a message to the Player or an NPC ship warning the GameManager that it was destroyed. This target exists before the game runs, so it's trivial to find its reference*. If all you want is this kind of messaging in code, you can use native C# events or an existing 'observer pattern' helper framework - I recommend Will Miller's: http://www.willrmiller.com/?p=87

Now, what should you do when you want to send a message *to the NPC ship*? It didn't exist in editor-time, since it was instantiated (spawned) during the game. The standard Unity approach would be, after getting the NPC GameObject reference from a native Unity callback, usually "OnTriggerEnter", to get a hold of its target component and execute a method there. Something like "gameObjectHit.GetComponent<Armor>().ApplyDamage(damage);"

The problem is: you've just had to deal with the Armor class' inner structure. Your code is no longer decoupled. "Why is that important?", you might guess. Without turning this into a long essay defending 'decouping' (there are many online), let's just say it's way more reusable and convenient to just call methods defined in an Interface. First because a message may or may not be consumed by the receiver, it won't break your game (due to a Null Reference error) in any case, minimizing the need for error handling. Second because when you may have a component with multiple interfaces defined and it'll still take that message. For instance, you could have a component like this:

	public class Vessel: MonoBehaviour, IArmor, ISpell {}

This will take all the messages defined in IArmor and ISpell. You could have multiple components mixing and matching interfaces as needed. Abstract classes aren't as flexible, you can inherit from only one, so their concret classes aren't composable.

		* Talking of references, make sure to learn about IoC frameworks. I recommend Syring (as a "let-me-try-this entry drug") and especially Zenject when you realize the advantages and are ready to get serious on the topic.
	
	
# Can I use EzMsg to send static messages?

Sure, just make your static component implement a specific message target interface and you're good to go. To make this even easier and way more reliable than by drag-and-dropping things in the Unity IDE, I recommend an IoC framework. I've done this short video showing how to set things up in Zenject: https://www.youtube.com/watch?v=uyN9KYvlgCQ


# I know what Sending a message means, but what's a request?

In some circumstances you need to retrieve a value from a method, the decoupled way to do that is by sending the method a request. Follows usage examples, the first using the standard notation followed by the shorthand (GameObject extension) notation:

	    int h1 = EzMsg.Request<IArmor, int>(other.gameObject, _=>_.GetHealth());
	    int h2 = other.gameObject.Request<IArmor, int>(_=>_.GetHealth());

Methods which reply to a request may return any type in their interface signature, but since they don't return IEnumerable type these methods can't be paused or sequenced. Alternatively you can use a regular Send message with one or more of the called method's parameter using the 'out' modifier. Like so:

		int health;
	    EzMsg.Send<IArmor>(other.gameObject, _=>_.GetHealth(out health));
		
# What's this _=>_ thing, is that a smiley?

That's standard C#'s lambda notation. Lambdas define anonymous delegates, which are basically pointers to methods which also hold a state. The Lambda notation expresses that whatever's on the left side "goes to" whatever's at the right side, since you need an identifier to work with. More tipically you'll see things like `x=>x.method()`in examples online, but personally I feel any letter used adds "cognitive weight" to the instruction, in practice making it harder to read. An underscore makes it clear that it has no meaning inside the call. If the compiler allowed me to type only .method() instead I gladly would, but feel free to write armor=>armor.GetHealth(out health) or whatever other identifier you prefer.

# I don't like those lambda smiley thingies in my code, can I "hide" it somehow?

Yes, you can. Check the beginning of the included Projectile.cs script to learn how to store an EventAction (used by Send) and EventFunc (used by Request) as static fields or local properties. Personally I only use this approach when I'm gonna be calling the same message many times in the same script.

# How to use it?

The easiest way to learn how to use EzMsg is by checking on the included EzMsg_tst scene, especially Projectile.cs and Armor.cs. For a new scene, take the follow steps:

	1. Add the EzMsgManager component to any existing GameObject in your scene (could be the Main Camera). This instance is required to host coroutines and keep track of the execution of multiple messages
	2. Create a new C# class to define one or more receiving message's interfaces. The script must include System.Collections and UnityEngine.EventSystems, and each defined interface must implement IEventSystemHandler.
	3. All method signatures defined in the interface must return type IEnumerable (and not Void), exception being methods to be called by EzMsg.Request
	4. The MonoBehaviour script which will send messages must include Ez.Msg (add `using Ez.Msg;` at the top of the script) 
	5. Now your script is ready to send a dynamic message from a certain interface type to another object. Make sure to add .Run() at the end of any Send command if you want it to execute immediately. Requests are always executed immediately.
	
# How can I sequence messages?

EzMsg was designed from the ground up to provide a natural, fluid coding style. Just "chain" your initial Send command by other 'Wait' or 'Send' commands. They will be executed in order, always waiting for the completion of the previous one. You may start with a `Wait` as well if you want, just bear in mind that only the starting `Send` might use the shorthand form. Eg.:

	    other.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage))
	        .Wait(2f)
	        .Send<IWeapon>(gameObject, _=>_.Reload())
	        .Run();
			
Let's break down this instruction. We're asking EzMsg to send an 'ApplyDamage' message, taking the Damage parameter, to any appropriate receiver in other.gameObject. To be a valid receiver, the component should implement the IArmor interface, which has the `IEnumerable ApplyDamage(int);` signature defined. Once matched dynamically, the method is executed and *only after its completion* the execution flow returns to the original message instruction.

If multiple components in gameObject (or one of its children, there's an optional parameter for that) implement IArmor, with different execution times, all of the methods will be started at once and *only after all of them have finished* the Send instruction will resume. When that happens, it'll wait for 2 seconds and Send another message, this time IWeapon.Reload(), to any component in the current (host) gameObject which implements IWeapon.

If there's no such candidate, nothing will happen - and be warned that no error will be raised, for best and worse. As soon as `SendSeqData` is defined (that's what all chainable EzMsg methods return), it's executed by Run(). Note that this is a lazy evaluation structure, so you may store it in a variable and execute it at a later time.

Also, the fact that any method receiving a 'Send' message should return IEnumerable allows you to use all the regular coroutine toolset provided by Unity (like 'yield return new WaitForSeconds(x);'), within the called method block, to control the execution timing. Keep that in mind when defining your overall logic flow.

For more examples, make sure to check the included demo scene and scripts.

Enjoy!
