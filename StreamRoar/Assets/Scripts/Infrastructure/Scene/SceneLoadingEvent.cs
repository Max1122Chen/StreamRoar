using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 场景开始加载。
    /// </summary>
    public readonly struct SceneLoadingEvent : IEvent
    {
        public SceneLoadingEvent(string sceneName, LoadSceneMode mode, AsyncOperation operation)
        {
            SceneName = sceneName;
            Mode = mode;
            Operation = operation;
        }

        public string SceneName { get; }
        public LoadSceneMode Mode { get; }
        public AsyncOperation Operation { get; }
    }
}
