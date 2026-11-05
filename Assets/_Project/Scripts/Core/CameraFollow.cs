using UnityEngine;

namespace Reborn.Core
{
    /// <summary>Kamera folgt dem Ziel weich.</summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.15f;
        public Vector3 offset = new Vector3(0f, 0.4f, -10f);

        Vector3 _velocity;

        void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
            if (target != null) transform.position = target.position + offset;
        }

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref _velocity, smoothTime);
        }
    }
}
