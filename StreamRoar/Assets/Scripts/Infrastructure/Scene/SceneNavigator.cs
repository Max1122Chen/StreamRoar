using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 负责场景加载与生命周期事件转发。
    /// </summary>
    public sealed class SceneNavigator : ISceneNavigator, IDisposable
    {
        readonly IEventBus m_EventBus;

        public SceneNavigator(IEventBus eventBus)
        {
            m_EventBus = eventBus;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// 异步加载场景并发布加载事件。
        /// </summary>
        public AsyncOperation LoadScene(string sceneName, LoadSceneMode mode)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            m_EventBus.Publish(new SceneLoadingEvent(sceneName, mode, operation));
            return operation;
        }

        /// <summary>
        /// 注销 Unity 场景回调。
        /// </summary>
        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            m_EventBus.Publish(new SceneLoadedEvent(scene, mode));
        }
    }
}
