using System;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    [Serializable]
    public readonly struct VfxSpawnRequest
    {
        public VfxSpawnRequest(
            Vector3 position,
            Quaternion rotation,
            float scale,
            float speed,
            Transform followTarget,
            VfxFollowMode followMode)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Speed = speed;
            FollowTarget = followTarget;
            FollowMode = followMode;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public float Scale { get; }
        public float Speed { get; }
        public Transform FollowTarget { get; }
        public VfxFollowMode FollowMode { get; }
    }
}
