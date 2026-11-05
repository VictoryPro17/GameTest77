using Reborn.Combat;
using Reborn.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Lebensbalken, Erfahrungsbalken und Level-Anzeige.</summary>
    public class HUD : MonoBehaviour
    {
        public Image healthFill;
        public Image xpFill;
        public Text levelText;
        public Text healthText;

        Health _health;
        PlayerProgression _progression;

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _health = player.GetComponent<Health>();
            _progression = player.GetComponent<PlayerProgression>();
            if (_health != null) _health.OnChanged += Refresh;
            if (_progression != null) _progression.OnChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (_health != null) _health.OnChanged -= Refresh;
            if (_progression != null) _progression.OnChanged -= Refresh;
        }

        void Refresh()
        {
            if (_health != null)
            {
                if (healthFill != null) healthFill.fillAmount = (float)_health.Current / _health.maxHealth;
                if (healthText != null) healthText.text = $"{_health.Current} / {_health.maxHealth}";
            }
            if (_progression != null)
            {
                if (xpFill != null) xpFill.fillAmount = (float)_progression.XP / _progression.XPToNextLevel;
                if (levelText != null) levelText.text = $"Lv {_progression.Level}";
            }
        }
    }
}
