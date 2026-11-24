using Reborn.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Grosse Lebensleiste des Bosses oben in der Mitte (Meilenstein 8); nur sichtbar, solange ein Boss wach ist.
    /// Meilenstein 8b: Name je Stufe, Lade-Leiste der Ultimate (Stufe 2) und Warnhinweis waehrend der Aufladung.
    /// Meilenstein 9a: gilt fuer jeden Boss (Golem und Eisdrache, IBossBarSource).
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
            var boss = ActiveBoss.Current;
            bool show = boss != null && boss.Health != null && !boss.Health.IsDead;
            if (root != null && root.activeSelf != show) root.SetActive(show);
            bool warn = show && boss.UltimateCharging;
            if (warningText != null)
            {
                if (warningText.gameObject.activeSelf != warn) warningText.gameObject.SetActive(warn);
                if (warn) warningText.text = boss.WarningText;
            }
            if (!show) return;
            if (fill != null) fill.fillAmount = (float)boss.Health.Current / boss.Health.maxHealth;
            if (nameText != null) nameText.text = boss.BarName;
            bool ult = boss.ShowUltimateBar;
            if (ultimateRoot != null && ultimateRoot.activeSelf != ult) ultimateRoot.SetActive(ult);
            if (ult && ultimateFill != null) ultimateFill.fillAmount = boss.UltimateProgress;
        }
    }
}
