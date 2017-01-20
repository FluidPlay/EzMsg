using UnityEngine;

namespace Ez.Msg
{
    /// <summary>This Manager is required in any scene using EzMsg. It's used to hold the execution of all coroutines.
    /// </summary>
    public class EzMsgManager : MonoBehaviour {
        public void Awake()
        {
            EzMsg.Manager = this;
        }
    }
}
