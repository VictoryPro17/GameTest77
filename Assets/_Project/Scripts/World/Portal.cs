using UnityEngine;

namespace Reborn.World
{
    /// <summary>
    /// Portal (Meilenstein 6): Laeuft der Spieler hinein, wird gespeichert, abgedunkelt und die Zielszene geladen.
    /// Erst nach armDelay Sekunden aktiv, damit man nach der Ankunft nicht gleich zurueckreist.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Portal : MonoBehaviour
    {
        public string targetScene = SceneTravel.FightScene;
        public string targetSpawn = "Ankunft";
        public float armDelay = 0.5f;

        float _startTime;

        void Start() => _startTime = Time.time;

        void OnTriggerEnter2D(Collider2D other) => Try(other);
        void OnTriggerStay2D(Collider2D other) => Try(other);

        void Try(Collider2D other)
        {
            if (Time.time - _startTime < armDelay || SceneTravel.IsTraveling) return;
            var body = other.attachedRigidbody;
            if (body == null || !body.CompareTag("Player")) return;
            var health = body.GetComponent<Combat.Health>();
            if (health != null && health.IsDead) return;
            Enter();
        }

        /// <summary>Reise starten (auch fuer Tests)</summary>
        public void Enter() => SceneTravel.Go(targetScene, targetSpawn);
    }
}
