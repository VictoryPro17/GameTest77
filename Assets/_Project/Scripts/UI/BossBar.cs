using Reborn.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Grosse Lebensleiste des Bosses oben in der Mitte (Meilenstein 8); nur sichtbar, solange ein Boss wach ist.
    /// Meilenstein 8b: Name je Stufe, Lade-Leiste der Ultimate (Stufe 2) und Warnhinweis waehrend der Aufladung.
    /// </summary>
    public class BossBar : MonoBehaviour
    {
        public GameObject root;
        public Image fill;
        public Text nameText;
        public GameObject ultimateRoot;
        public Image ultimateFill;
        public Text warningText;

        public bool Visible => root != null && root.activeSelf;
        public bool WarningVisible => warningText != null && warningText.gameObject.activeSelf && warningText.text != "";

        void Update()
        {
            var boss = BossAI.Active;
            bool show = boss != null && boss.Health != null && !boss.Health.IsDead;
            if (root != null && root.activeSelf != show) root.SetActive(show);
            bool warn = show && boss.UltimateCharging;
            if (warningText != null)
            {
                if (warningText.gameObject.activeSelf != warn) warningText.gameObject.SetActive(warn);
                if (warn) warningText.text = "Zorn des Waldes!  Lauf weg oder weiche aus!";
            }
            if (!show) return;
            if (fill != null) fill.fillAmount = (float)boss.Health.Current / boss.Health.maxHealth;
            if (nameText != null)
            {
                string extra = boss.Stage >= 2 ? "  (Stufe 2)" : boss.Phase >= 3 ? "  (Wut!)" : boss.Phase == 2 ? "  (Phase 2)" : "";
                nameText.text = boss.CurrentName + extra;
            }
            bool ult = boss.Stage >= 2;
            if (ultimateRoot != null && ultimateRoot.activeSelf != ult) ultimateRoot.SetActive(ult);
            if (ult && ultimateFill != null) ultimateFill.fillAmount = boss.UltimateProgress;
        }
    }
}
