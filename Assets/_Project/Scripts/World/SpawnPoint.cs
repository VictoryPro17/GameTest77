using UnityEngine;

namespace Reborn.World
{
    /// <summary>Ankunftsort nach einem Szenenwechsel (Meilenstein 6). Passt die ID zu SceneTravel.PendingSpawn, steht der Spieler hier.</summary>
    public class SpawnPoint : MonoBehaviour
    {
        public string id = "Start";

        /// <summary>ID des zuletzt benutzten Ankunftspunkts (fuer Tests; beim Play-Start leer)</summary>
        public static string LastArrival { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => LastArrival = "";

        void Start()
        {
            if (SceneTravel.PendingSpawn != id) return;
            SceneTravel.PendingSpawn = null;
            LastArrival = id;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = transform.position;
            player.transform.position = transform.position;
            var cam = Object.FindAnyObjectByType<Core.CameraFollow>();
            if (cam != null) cam.transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z);
        }
    }
}
