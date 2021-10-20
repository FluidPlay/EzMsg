namespace Ez
{
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>EzMsg ShortHand Methods (called from GameObject)
/// </summary>
public static class EzMsgExtensions {
    /// <summary>Shorthand extension method for EzMsg.Request(gameObject, eventFunc). Eg.:
    /// int h1 = EzMsg.Request<IArmor, int>(col.gameObject, _=>_.GetHealth());
    /// </summary>
    /// <param name="gO">Target GameObject</param>
    /// <param name="eventFunc">Method to be executed</param>
    /// <param name="sendToChildren">Should the request be sent to children of Target?</param>
    /// <typeparam name="T1">System Handler Interface Type</typeparam>
    /// <typeparam name="T2">Return type</typeparam>
    /// <returns></returns>
    public static T2 Request<T1, T2>(this GameObject gO, EzMsg.EventFunc<T1, T2> eventFunc, bool sendToChildren = true)
        where T1: IEventSystemHandler
    {
        return EzMsg.Request(gO, eventFunc, sendToChildren);
    }

    /// <summary>Shorthand, auto-run extension method for EzMsg.Send(gameObject, eventAction). Eg.:
    /// col.gameObject.Send<IArmor>(_=>_.ApplyDamage(Damage));
    /// This can NOT be chained / sequenced
    /// </summary>
    /// <param name="gO">Target GameObject</param>
    /// <param name="eventAction">Method to be executed</param>
    /// <typeparam name="T">Interface Type to be matched</typeparam>
    public static void Send<T>(this GameObject gO, EzMsg.EventAction<T> eventAction,
        bool sendToChildren = false, EzMsg.SendSeqData sendSeqData = null)
        where T: IEventSystemHandler
    {
        EzMsg.Send(gO, eventAction, sendToChildren, sendSeqData).Run();
    }

    public static EzMsg.SendSeqData Wait(this GameObject gO, float timeToWait, bool realtime = false, EzMsg.SendSeqData sendSeqData = null)
    {
        return EzMsg.Wait(timeToWait, realtime, sendSeqData);
    }
}

}
