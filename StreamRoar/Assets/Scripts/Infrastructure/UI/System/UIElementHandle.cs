using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 运行时 UI 实例句柄。
    /// </summary>
    sealed class UIElementHandle
    {
        public string ElementId;
        public GameObject Instance;
        public bool IsClosing;
        public bool PendingShow;
        public bool IsEscapable;
        public Coroutine TransitionRoutine;
    }
}
