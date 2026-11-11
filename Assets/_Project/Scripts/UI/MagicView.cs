using Reborn.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Reiter "Magie": die 4 Zauber mit Stufe, Erfahrung und Werten, dazu Mana-Stufe.</summary>
    public class MagicView : MonoBehaviour
    {
        public Image[] icons = new Image[SpellBook.SlotCount];
        public Text[] titles = new Text[SpellBook.SlotCount];
        public Image[] xpFills = new Image[SpellBook.SlotCount];
        public Text[] infos = new Text[SpellBook.SlotCount];
        public Text manaText;
        public Image manaXpFill;

        SpellBook _book;
        PlayerMana _mana;

        void OnEnable()
        {
            if (_book == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _book = player.GetComponent<SpellBook>();
                    _mana = player.GetComponent<PlayerMana>();
                }
            }
            Refresh();
        }

        void Update() => Refresh();

        static readonly string[] Keys = { "1 / RB", "2 / RT", "3 / LB", "4 / LT" };

        public void Refresh()
        {
            if (_book == null) return;
            for (int i = 0; i < SpellBook.SlotCount; i++)
            {
                var s = _book.Get(i);
                if (s == null) continue;
                bool open = _book.IsUnlocked(i);
                int lvl = _book.Level(i);
                if (icons[i] != null)
                {
                    icons[i].sprite = s.icon;
                    icons[i].color = open ? Color.white : new Color(0.35f, 0.35f, 0.4f, 1f);
                }
                if (titles[i] != null)
                {
                    string hex = ColorUtility.ToHtmlStringRGB(SpellData.ElementColor(s.element));
                    titles[i].text = $"<color=#{hex}>{s.spellName}</color>  Stufe {lvl}/{SpellBook.MaxLevel}   <color=#999999>[{Keys[i]}]</color>";
                }
                if (xpFills[i] != null)
                    xpFills[i].fillAmount = lvl >= SpellBook.MaxLevel ? 1f : (float)_book.XP(i) / _book.XPToNextLevel(i);
                if (infos[i] != null)
                {
                    string dmg = _book.SplashDamage(i) > 0 ? $"{_book.Damage(i)} + {_book.SplashDamage(i)} Explosion" : _book.Damage(i).ToString();
                    string xp = lvl >= SpellBook.MaxLevel ? "höchste Stufe" : $"Erfahrung {_book.XP(i)}/{_book.XPToNextLevel(i)}";
                    infos[i].text = open
                        ? $"{s.ElementLabel} · Mana {s.manaCost} · Schaden {dmg} · Abklingzeit {s.cooldown:0.0} s · {xp}"
                        : $"<color=#FF8866>Gesperrt: {_book.UnlockText(i)}</color>";
                }
            }
            if (_mana != null)
            {
                string next = _mana.Level >= PlayerMana.MaxLevel ? "höchste Stufe" : $"Erfahrung {_mana.XP}/{_mana.XPToNextLevel}";
                if (manaText != null)
                    manaText.text = $"Mana {Mathf.FloorToInt(_mana.Current)}/{_mana.Max}   Mana-Stufe {_mana.Level}/{PlayerMana.MaxLevel} ({next})\n" +
                                    $"Füllt sich mit {_mana.RegenRate:0.0} pro Sekunde. Meditieren (M / B halten): {_mana.meditateRate:0} pro Sekunde und Mana-Training.";
                if (manaXpFill != null)
                    manaXpFill.fillAmount = _mana.Level >= PlayerMana.MaxLevel ? 1f : (float)_mana.XP / _mana.XPToNextLevel;
            }
        }
    }
}
