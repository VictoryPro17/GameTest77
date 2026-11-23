using Reborn.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Grosse Lebensleiste des Bosses oben in der Mitte (Meilenstein 8); nur sichtbar, solange ein Boss wach ist.</summary>
    public class BossBar : MonoBehaviour
    {
        public GameObject root;
        public Image fill;
        public Text nameText;

        public bool Visible => root != null && root.activeSelf;

        void Update()
        {
            var boss = BossAI.Active;
            bool show = boss != null && boss.Health != null && !boss.Health.IsDead;
            if (root != null && root.activeSelf != show) root.SetActive(show);
            if (!show) return;
            if (fill != null) fill.fillAmount = (float)boss.Health.Current / boss.Health.maxHealth;
            if (nameText != null)
            {
                string phase = boss.Phase >= 3 ? "  (Wut!)" : boss.Phase == 2 ? "  (Phase 2)" : "";
                nameText.text = boss.displayName + phase;
            }
        }
    }
}
