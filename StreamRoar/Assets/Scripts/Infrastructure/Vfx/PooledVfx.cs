using UnityEngine;

namespace StreamRoar.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class PooledVfx : MonoBehaviour, IPoolLifecycle
    {
        ParticleSystem[] m_ParticleSystems;
        Vector3[] m_BaseLocalScales;
        float[] m_BaseSimulationSpeeds;

        Transform m_FollowTarget;
        bool m_FollowPosition;
        bool m_FollowRotation;
        Vector3 m_FollowLocalPosition;
        Quaternion m_FollowLocalRotation;

        void Awake()
        {
            m_ParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
            m_BaseLocalScales = new Vector3[m_ParticleSystems.Length];
            m_BaseSimulationSpeeds = new float[m_ParticleSystems.Length];

            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                ParticleSystem particle = m_ParticleSystems[i];
                m_BaseLocalScales[i] = particle.transform.localScale;
                m_BaseSimulationSpeeds[i] = particle.main.simulationSpeed;
            }
        }

        /// <summary>
        /// 让特效跟随目标。在当前世界位姿下采样相对偏移，
        /// 因此挂点上的 LocalOffset 等微调会被保留。
        /// </summary>
        public void SetFollow(Transform target, VfxFollowMode mode)
        {
            if (!target || mode == VfxFollowMode.None)
            {
                m_FollowPosition = false;
                m_FollowRotation = false;
                m_FollowTarget = null;
                return;
            }

            m_FollowTarget = target;
            m_FollowPosition = mode is VfxFollowMode.Position or VfxFollowMode.Both;
            m_FollowRotation = mode is VfxFollowMode.Rotation or VfxFollowMode.Both;
            m_FollowLocalPosition = target.InverseTransformPoint(transform.position);
            m_FollowLocalRotation = Quaternion.Inverse(target.rotation) * transform.rotation;
        }

        void LateUpdate()
        {
            if (!m_FollowPosition && !m_FollowRotation)
                return;

            // 挂点可能在特效存活期间被销毁，这里仍需一次 Unity 判空。
            if (!m_FollowTarget)
                return;

            if (m_FollowPosition)
                transform.position = m_FollowTarget.TransformPoint(m_FollowLocalPosition);
            if (m_FollowRotation)
                transform.rotation = m_FollowTarget.rotation * m_FollowLocalRotation;
        }

        public void OnPoolRent()
        {
            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                ParticleSystem particle = m_ParticleSystems[i];
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear(true);
                particle.Play(true);
            }
        }

        public void SetSpeed(float speed)
        {
            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                ParticleSystem.MainModule main = m_ParticleSystems[i].main;
                main.simulationSpeed = m_BaseSimulationSpeeds[i] * speed;
            }
        }

        public void SetScale(float scale)
        {
            for (int i = 0; i < m_ParticleSystems.Length; i++)
                m_ParticleSystems[i].transform.localScale = m_BaseLocalScales[i] * scale;
        }

        public bool IsAlive()
        {
            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                ParticleSystem particle = m_ParticleSystems[i];
                if (particle && particle.IsAlive(true))
                    return true;
            }

            return false;
        }

        public void OnPoolReturn()
        {
            m_FollowTarget = null;
            m_FollowPosition = false;
            m_FollowRotation = false;
            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                ParticleSystem particle = m_ParticleSystems[i];
                particle.transform.localScale = m_BaseLocalScales[i];

                ParticleSystem.MainModule main = particle.main;
                main.simulationSpeed = m_BaseSimulationSpeeds[i];

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particle.Clear(true);
            }
        }

    }
}
