using Reborn.Core;
using Reborn.World;
using UnityEngine;

namespace Reborn.Combat
{
    /// <summary>
    /// Tod des Spielers (Meilenstein 6): "Du wurdest besiegt", -10 % Gold, dann zurueck in die Basis.
    /// Leben und Mana sind dort wieder voll (Leben 0 im Spielstand laedt als volles Leben). Gegenstaende und Erfahrung bleiben.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DeathHandler : MonoBehaviour
    {
        public const string Message = "Du wurdest besiegt";
        [Range(0f, 1f)] public float goldLoss = 0.1f;
        public float messageTime = 2f;

        /// <summary>Gold, das beim letzten Tod verloren ging (fuer Tests)</summary>
        public int LastLoss { get; private set; }

        Health _health;

        void Awake() => _health = GetComponent<Health>();
        void OnEnable() => _health.OnDied += HandleDied;
        void OnDisable() => _health.OnDied -= HandleDied;

        void HandleDied()
        {
            var wallet = GetComponent<Economy.PlayerWallet>();
            LastLoss = wallet != null ? wallet.LoseFraction(goldLoss) : 0;
            var mana = GetComponent<Magic.PlayerMana>();
            if (mana != null) mana.SetCurrent(mana.Max);
            SceneTravel.Go(SceneTravel.BaseScene, "Start", 0.5f, Loc.T(Message), messageTime);
        }
    }
}
