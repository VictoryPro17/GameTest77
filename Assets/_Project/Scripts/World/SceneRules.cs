using UnityEngine;

namespace Reborn.World
{
    /// <summary>
    /// Regeln der aktuellen Szene (Meilenstein 6). In der Basis ist peaceful = true:
    /// kein Angriff, keine Zauber, Kampf-Knoepfe auf dem Handy aus.
    /// </summary>
    public class SceneRules : MonoBehaviour
    {
        public bool peaceful;

        static SceneRules _current;

        public static bool Peaceful => _current != null && _current.peaceful && !TrainingAllowed;

        /// <summary>Meilenstein 10b: die Einfuehrung erlaubt kurz Angriff und Zauber in der Stadt (Uebung)</summary>
        public static bool TrainingAllowed { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => TrainingAllowed = false;

        void Awake() => _current = this;

        void OnDestroy()
        {
            if (_current == this) _current = null;
        }
    }
}
