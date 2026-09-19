using UnityEngine.SceneManagement;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 场景加载完成。
    /// </summary>
    public readonly struct SceneLoadedEvent : IEvent
    {
        public SceneLoadedEvent(Scene scene, LoadSceneMode mode)
        {
            Scene = scene;
            Mode = mode;
        }

        public Scene Scene { get; }
        public LoadSceneMode Mode { get; }
    }
}
