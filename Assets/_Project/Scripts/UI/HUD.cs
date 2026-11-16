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
        public Image manaPotionIcon;
        public Text manaPotionCountText;
        public Image manaFill;
        public Text manaText;
        /// <summary>4 Zauber-Plaetze: Icon, Abklingzeit-Schatten, Stufe</summary>
        public Image[] spellIcons = new Image[Magic.SpellBook.SlotCount];
        public Image[] spellCooldowns = new Image[Magic.SpellBook.SlotCount];
        public Text[] spellLevels = new Text[Magic.SpellBook.SlotCount];

        Magic.SpellBook _book;
        Magic.PlayerMana _mana;
        Color _manaColor;
        float _manaBlink;

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
            _book = player.GetComponent<Magic.SpellBook>();
            _mana = player.GetComponent<Magic.PlayerMana>();
            if (manaFill != null) _manaColor = manaFill.color;
            if (_mana != null) _mana.OnNotEnough += BlinkMana;
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
            if (_mana != null) _mana.OnNotEnough -= BlinkMana;
        }

        void BlinkMana() => _manaBlink = 0.4f;

        /// <summary>Mana und Zauber-Plaetze aendern sich laufend, deshalb jedes Bild neu.</summary>
        void Update()
        {
            if (_mana != null)
            {
                if (manaFill != null)
                {
                    manaFill.fillAmount = _mana.Max > 0 ? _mana.Current / _mana.Max : 0f;
                    if (_manaBlink > 0f)
                    {
                        _manaBlink -= Time.unscaledDeltaTime;
                        manaFill.color = Mathf.Repeat(_manaBlink, 0.2f) > 0.1f ? new Color(1f, 0.3f, 0.3f) : _manaColor;
                    }
                    else manaFill.color = _manaColor;
                }
                if (manaText != null) manaText.text = $"{Mathf.FloorToInt(_mana.Current)} / {_mana.Max}";
            }
            if (_book == null) return;
            for (int i = 0; i < Magic.SpellBook.SlotCount; i++)
            {
                var s = _book.Get(i);
                if (s == null) continue;
                bool open = _book.IsUnlocked(i);
                bool enough = _mana == null || _mana.Current >= s.manaCost;
                if (spellIcons[i] != null)
                {
                    spellIcons[i].sprite = s.icon;
                    spellIcons[i].color = !open ? new Color(0.25f, 0.25f, 0.3f, 1f) : enough ? Color.white : new Color(0.55f, 0.55f, 0.65f, 1f);
                }
                if (spellCooldowns[i] != null)
                    spellCooldowns[i].fillAmount = open && s.cooldown > 0f ? _book.CooldownLeft(i) / s.cooldown : 0f;
                if (spellLevels[i] != null) spellLevels[i].text = open ? "St." + _book.Level(i) : "-";
            }
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
                var manaPotion = _potions.NextManaPotion;
                if (manaPotionIcon != null)
                {
                    manaPotionIcon.enabled = manaPotion != null && manaPotion.icon != null;
                    if (manaPotion != null) manaPotionIcon.sprite = manaPotion.icon;
                }
                if (manaPotionCountText != null) manaPotionCountText.text = manaPotion != null ? _potions.ManaPotionCount.ToString() : "0";
            }
        }
    }
}
