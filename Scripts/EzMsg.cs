namespace Ez {

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
public static class EzMsg  {

    /// <summary> Singleton used to hold all coroutines used by EzMsg</summary>
    private static MonoBehaviour _manager;
    public static MonoBehaviour Manager {
        get
        {
            if (_manager == null)
                _manager = Object.FindObjectOfType<EzMsgManager>();
            if (_manager == null)
            {
                var newGO = new GameObject() {name = "_EzMsgManager"};
                _manager = newGO.AddComponent<EzMsgManager>();
                Debug.LogWarning("EzMsgManager component wasn't found in Scene, created dynamically.");}
            return _manager;
        }
        set { _manager = value; }
    }

    public class SendSeqData
    {
        public List<IEnumerable> Coroutines = new List<IEnumerable>();

        public void Run()
        {
            if (Coroutines == null)
            {
                Debug.LogWarning("EzMsg: No Coroutines found to be run.");
                return;
            }


            //Debug.Log("# of Coroutines Scheduled: "+Coroutines.Count);

            if (Manager == null)
            {
                Debug.LogWarning("EzMsg: EzMsgManager component not found in Scene. Please rectify");
                return;
            }

            Manager.StartCoroutine(RunCoroutines(Coroutines));
        }

        private IEnumerator RunCoroutines(List<IEnumerable> funcs)
        {
            for (var i = 0; i < funcs.Count; i++)
            {
                var func = funcs[i];
                //Debug.Log(func.ToString());
                if (func != null)
                    yield return Manager.StartCoroutine(func.GetEnumerator());
            }
        }
    }

    #region #Region SendSeqData Extension Methods

    /// <summary> [Extension] Wait the given amount of seconds before proceeding the Sequence Data </summary>
    /// <param name="sendSeqData"></param>
    /// <param name="waitTime">Time to wait, in seconds</param>
    /// <param name="realtime">Ignores Time.TimeScale</param>
    public static SendSeqData Wait(this SendSeqData sendSeqData, float waitTime, bool realtime = false)
    {
        return Wait(waitTime, realtime, sendSeqData);
    }

    /// <summary> [Extension] Send EzMsg with Timer/Chaining capability. Eg.:
    /// EzMsg.Send<IArmor>(col, _=>_.ApplyDamage, true)
    /// .Wait(0.5f).Send<IWeapon>(gameObject, _=>_.Reload);
    /// </summary>
    public static SendSeqData Send<T>(this SendSeqData sendSeqData, GameObject target,
        EventAction<T> functor, bool sendToChildren = false)
        where T : IEventSystemHandler
    {
        return Send(target, functor, sendToChildren, sendSeqData);
    }

    #endregion << SendSeqData Extension Methods

    /// <summary>
    /// Non-chainable counterpart to Send, returning the first response from a given target, interface and Functor.
    /// Eg.: var curState = EzMsg.Request<IFSM, FSMState>(gameObject, (x,y) => x.GetCurrentState());
    /// </summary>
    /// <param name="target">Target GameObject</param>
    /// <param name="functor">Method to call, must return a T2 type</param>
    /// <param name="requestToChildren">Should the request be sent to children of target?</param>
    /// <param name="responseCheck">Should each request check for a non-null response? Use nullable types with this.</param>
    /// <typeparam name="T1">Required interface which should be implemented in a matching component</typeparam>
    /// <typeparam name="T2">Required method return type</typeparam>
    /// <returns>Response to Request, T2 type</returns>
    public static T2 Request<T1,T2>(GameObject target, EventFunc<T1, T2> functor, bool requestToChildren = true)
        where T1 : IEventSystemHandler
    {
        var response = Request(target, null, functor);

        if (!IsGenericNull(response) || target.transform.childCount == 0 || !requestToChildren)
            return response;

        // GetComponentsInChildren(false) = exclude inactive
        for (var i = 0; i < target.GetComponentsInChildren<Transform>(false).Length; i++) {
            Transform child = target.GetComponentsInChildren<Transform>(false)[i];
            // Skips `target` so it isn't requested twice
            if (child == target.transform)
                continue;
            response = Request(child.gameObject, null, functor);
            if (!IsGenericNull(response))
                return response;
        }
        return response;
    }

    /// <summary> Send EzMsg with Timer/Chaining capability. Eg.:
    /// EzMsg.Send<IArmor>(col, _=>_.ApplyDamage, true)
    /// .Wait(0.5f).Send<IWeapon>(gameObject, _=>_.Reload);
    /// </summary>
    public static SendSeqData Send<T>(GameObject target,
        EventAction<T> functor, bool sendToChildren = false, SendSeqData sendSeqData = null)
        where T : IEventSystemHandler
    {
        if (sendSeqData == null)
            sendSeqData = new SendSeqData();

        sendSeqData.Coroutines.Add ( ExecuteRecursive(target, functor, sendToChildren) );

        return sendSeqData;
    }

    public static SendSeqData Wait(float waitTime, bool realtime = false, SendSeqData sendSeqData = null)
    {
        if (sendSeqData == null)
            sendSeqData = new SendSeqData();

        sendSeqData.Coroutines.Add(WaitCoroutine(waitTime, realtime));
        return sendSeqData;
    }

    private static IEnumerable WaitCoroutine(float waitTime, bool realtime = false)
    {
          #if UNITY_5_4_OR_NEWER
          if (realtime)
             yield return new WaitForSecondsRealtime(waitTime);
          else
          #endif
            yield return new WaitForSeconds(waitTime);
    }

    //     ### Variation with C# 'Timers'
//        var startTime = realtime ? Time.realtimeSinceStartup : Time.time;
//        yield return new WaitWhile(() => Time.time < startTime + waitTime );
//        var timer = new Timer(delay);
//        timer.Elapsed += (_, __) =>
//        {
//            Debug.Log("Sending message to " + typeof(T) + " after " + delay / 1000 + " s.");
//            // Fire Event
//            //ExecuteRecursive(target, functor, sendToChildren, sendSeqData);
//            callbackAction.SafeInvoke();
//            timer.Stop();
//            timer.Dispose();
//        };
//        timer.Start();

    public static IEnumerable ExecuteRecursive<T>(GameObject target, EventAction<T> functor,
        bool sendToChildren = false)
        where T : IEventSystemHandler
    {
        yield return Manager.StartCoroutine(Manager.ExecuteSeq(target, functor).GetEnumerator());

        if (target == null || target.transform.childCount == 0 || !sendToChildren)
            yield break;

        // Will wait for the logic completion of all valid targets in the target's hierarchy
        // (false) = don't include inactive
        for (var i = 0; i < target.GetComponentsInChildren<Transform>(false).Length; i++)
        {
            Transform child = target.GetComponentsInChildren<Transform>(false)[i];
            // Skips `target` so it isn't requested twice
            if (child == target.transform)
                continue;
            yield return Manager.StartCoroutine(Manager.ExecuteSeq(child.gameObject, functor).GetEnumerator());
        }
    }

    public static IEnumerable ExecuteSeq<T>(this MonoBehaviour mb, GameObject target, EventAction<T> functor) where T : IEventSystemHandler
    {
        var internalHandlers = s_HandlerListPool.Get();
        GetEventList<T>(target, internalHandlers);
        //	if (s_InternalHandlers.Count > 0)
        //		Debug.Log("Executing " + typeof (T) + " on " + target);

        for (var i = 0; i < internalHandlers.Count; i++)
        {
            T arg;
            try
            {
                arg = (T)internalHandlers[i];
            }
            catch (Exception e)
            {
                var temp = internalHandlers[i];
                Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                continue;
            }

            if (arg == null || functor == null)
            {
                Debug.LogError("EzMsg: Signature Type Incompatible or Functor not assigned");
                yield break;
            }
            yield return mb.StartCoroutine(functor(arg).GetEnumerator());
        }

        //        var handlerCount = internalHandlers.Count;
        //        s_HandlerListPool.Release(internalHandlers);
        //        return handlerCount > 0;
    }

    /// <summary>Generic-compatible version of the equality operator (==)</summary>
    /// <param name="requestValue"></param>
    /// <typeparam name="T2"></typeparam>
    /// <returns></returns>
    private static bool IsGenericNull<T2>(T2 requestValue)
    {
        return EqualityComparer<T2>.Default.Equals(requestValue, default(T2));
    }

    #region #Region ##### Additions / Tweaks to original ExecuteEvents #####
    /// Adapted from //https://github.com/tenpn/unity3d-ui/blob/master/UnityEngine.UI/EventSystem/ExecuteEvents.cs

    public delegate IEnumerable EventAction<T1>(T1 handler);
    public delegate T2 EventFunc<T1,T2>(T1 handler);

    internal class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll { get; private set; }

        public int countActive {
            get
            { return this.countAll - this.countInactive; }
        }

        public int countInactive  {
            get { return this.m_Stack.Count; }
        }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease) {
            this.m_ActionOnGet = actionOnGet;
            this.m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T obj;
            if (this.m_Stack.Count == 0)
            {
                obj = Activator.CreateInstance<T>();
                ++this.countAll;
            }
            else
                obj = this.m_Stack.Pop();
            if (this.m_ActionOnGet != null)
                this.m_ActionOnGet(obj);
            return obj;
        }

        public void Release(T element)
        {
            if (this.m_Stack.Count > 0 && object.ReferenceEquals((object) this.m_Stack.Peek(), (object) element))
                Debug.LogError((object) "Internal error. Trying to destroy object that is already released to pool.");
            if (this.m_ActionOnRelease != null)
                this.m_ActionOnRelease(element);
            this.m_Stack.Push(element);
        }
    }

    private static readonly ObjectPool<List<IEventSystemHandler>> s_HandlerListPool = new ObjectPool<List<IEventSystemHandler>>(null, l => l.Clear());

    private static void GetEventChain(GameObject root, IList<Transform> eventChain)
    {
        eventChain.Clear();
        if (root == null)
            return;

        var t = root.transform;
        while (t != null)
        {
            eventChain.Add(t);
            t = t.parent;
        }
    }

    public static bool Execute<T>(GameObject target, EventAction<T> functor) where T : IEventSystemHandler
    {
        var internalHandlers = s_HandlerListPool.Get();
        GetEventList<T>(target, internalHandlers);
        //	if (s_InternalHandlers.Count > 0)
        //		Debug.Log("Executing " + typeof (T) + " on " + target);

        for (var i = 0; i < internalHandlers.Count; i++)
        {
            T arg;
            try
            {
                arg = (T)internalHandlers[i];
            }
            catch (Exception e)
            {
                var temp = internalHandlers[i];
                Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                continue;
            }

            try
            {
                functor(arg);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        var handlerCount = internalHandlers.Count;
        s_HandlerListPool.Release(internalHandlers);
        return handlerCount > 0;
    }

    /// <summary> Execute the specified event on the first game object underneath the current touch. </summary>
    private static readonly List<Transform> s_InternalTransformList = new List<Transform>(30);

    internal static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>((UnityAction<List<T>>) null, (UnityAction<List<T>>) (l => l.Clear()));

        public static List<T> Get()
        {
            return ListPool<T>.s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            ListPool<T>.s_ListPool.Release(toRelease);
        }
    }

    /// <summary>
    /// A Request sends a message asking for the second parameter type as return. Instead of a success/failure bool
    /// For primitives you must use, eg. bool? or int? in the method call to get `null as return, if no answer found
    /// </summary>
    /// <param name="target">Target GameObject to send the request</param>
    /// <param name="eventData">Event Data</param>
    /// <param name="functor">Actual Method to be executed</param>
    /// <typeparam name="T1">Functor type</typeparam>
    /// <typeparam name="T2">Return Type</typeparam>
    /// <returns></returns>
    public static T2 Request<T1,T2>(GameObject target, BaseEventData eventData, EventFunc<T1,T2> functor)
        where T1 : IEventSystemHandler
    {
        var internalHandlers = s_HandlerListPool.Get();
        GetEventList<T1>(target, internalHandlers);
        // Debug Purposes Only
//            Debug.Log(internalHandlers.Count + " scripts found of type " + typeof (T1) + " on " + target.name);
//
//            if (internalHandlers.Count > 0)
//                Debug.Log("Executing " + typeof (T1) + " on " + target);

        T2 returnValue = default(T2);
        for (var i = 0; i < internalHandlers.Count; i++)
        {
            T1 arg;
            try
            {
                arg = (T1)internalHandlers[i];
            }
            catch (Exception e)
            {
                var temp = internalHandlers[i];
                Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T1).Name, temp.GetType().Name), e));
                continue;
            }

            try
            {
                if (arg == null || functor == null)
                {
                    Debug.LogError("EzMsg: Incompatible Signature Type or Functor not assigned");
                    return returnValue;
                }
                returnValue = functor(arg);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        return returnValue;
    }

    public static GameObject ExecuteHierarchy<T>(GameObject root, EventAction<T> callbackAction) where T : IEventSystemHandler
    {
        GetEventChain(root, s_InternalTransformList);

        for (var i = 0; i < s_InternalTransformList.Count; i++)
        {
            var transform = s_InternalTransformList[i];
            if (Execute(transform.gameObject, callbackAction))
                return transform.gameObject;
        }
        return null;
    }

    private static bool ShouldSendToComponent<T>(Component component) where T : IEventSystemHandler
    {
        var valid = component is T;
        if (!valid)
            return false;

        var behaviour = component as Behaviour;
        if (behaviour != null)
            return behaviour.enabled;
        return true;
    }

    private static void GetEventList<T>(GameObject go, IList<IEventSystemHandler> results) where T : IEventSystemHandler
    {
        if (results == null)
            throw new ArgumentException("Results array is null", "results");
        if (go == null || !go.activeInHierarchy)
            return;
        List<Component> componentList = ListPool<Component>.Get();
        go.GetComponents<Component>(componentList);
        for (int index = 0; index < componentList.Count; ++index)
        {
            if (ShouldSendToComponent<T>(componentList[index]))
                results.Add(componentList[index] as IEventSystemHandler);
        }
        ListPool<Component>.Release(componentList);
    }

    /// <summary> Whether the specified game object will be able to handle the specified event. </summary>
    public static bool CanHandleEvent<T>(GameObject go) where T : IEventSystemHandler
    {
        var internalHandlers = s_HandlerListPool.Get();
        GetEventList<T>(go, internalHandlers);
        var handlerCount = internalHandlers.Count;
        s_HandlerListPool.Release(internalHandlers);
        return handlerCount != 0;
    }

    /// <summary> Bubble the specified event on the game object, figuring out which object will actually receive the event. </summary>
    public static GameObject GetEventHandler<T>(GameObject root) where T : IEventSystemHandler
    {
        if (root == null)
            return null;

        Transform t = root.transform;
        while (t != null)
        {
            if (CanHandleEvent<T>(t.gameObject))
                return t.gameObject;
            t = t.parent;
        }
        return null;
    }

    #endregion

}

}
