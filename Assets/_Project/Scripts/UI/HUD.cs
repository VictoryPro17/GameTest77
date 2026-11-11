using Reborn.Combat;
using Reborn.Inventory;
using Reborn.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Lebensbalken, Erfahrungsbalken, Level, Trank-Platz und Hinweis auf freie Punkte.</summary>
    public class HUD : MonoBehaviour
    {
        public Image healthFill;
        public Image xpFill;
        public Text levelText;
        public Text healthText;
        public Image potionIcon;
        public Text potionCountText;
        public Text pointsHint;

        Health _health;
        PlayerProgression _progression;
        PlayerStats _stats;
        PlayerInventory _inventory;
        PotionUser _potions;

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _health = player.GetComponent<Health>();
            _progression = player.GetComponent<PlayerProgression>();
            _stats = player.GetComponent<PlayerStats>();
            _inventory = player.GetComponent<PlayerInventory>();
            _potions = player.GetComponent<PotionUser>();
            if (_health != null) _health.OnChanged += Refresh;
            if (_progression != null) _progression.OnChanged += Refresh;
            if (_stats != null) _stats.OnChanged += Refresh;
            if (_inventory != null) _inventory.OnChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (_health != null) _health.OnChanged -= Refresh;
            if (_progression != null) _progression.OnChanged -= Refresh;
            if (_stats != null) _stats.OnChanged -= Refresh;
            if (_inventory != null) _inventory.OnChanged -= Refresh;
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
            if (pointsHint != null) pointsHint.text = _stats != null && _stats.UnspentPoints > 0 ? $"+{_stats.UnspentPoints} Punkte" : "";
            if (_potions != null)
            {
                var potion = _potions.NextPotion;
                if (potionIcon != null)
                {
                    potionIcon.enabled = potion != null && potion.icon != null;
                    if (potion != null) potionIcon.sprite = potion.icon;
                }
                if (potionCountText != null) potionCountText.text = potion != null ? _potions.PotionCount.ToString() : "0";
            }
        }
    }
}
