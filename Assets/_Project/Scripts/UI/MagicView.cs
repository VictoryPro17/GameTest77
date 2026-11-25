using Reborn.Core;
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
                    titles[i].text = Loc.F("<color=#{0}>{1}</color>  Stufe {2}/{3}   <color=#999999>[{4}]</color>", hex, Loc.T(s.spellName), lvl, SpellBook.MaxLevel, Keys[i]);
                }
                if (xpFills[i] != null)
                    xpFills[i].fillAmount = lvl >= SpellBook.MaxLevel ? 1f : (float)_book.XP(i) / _book.XPToNextLevel(i);
                if (infos[i] != null)
                {
                    string dmg = _book.SplashDamage(i) > 0 ? Loc.F("{0} + {1} Explosion", _book.Damage(i), _book.SplashDamage(i)) : _book.Damage(i).ToString();
                    string xp = lvl >= SpellBook.MaxLevel ? Loc.T("höchste Stufe") : Loc.F("Erfahrung {0}/{1}", _book.XP(i), _book.XPToNextLevel(i));
                    infos[i].text = open
                        ? Loc.F("{0} · Mana {1} · Schaden {2} · Abklingzeit {3:0.0} s · {4}", s.ElementLabel, s.manaCost, dmg, s.cooldown, xp)
                        : Loc.F("<color=#FF8866>Gesperrt: {0}</color>", _book.UnlockText(i));
                }
            }
            if (_mana != null)
            {
                string next = _mana.Level >= PlayerMana.MaxLevel ? Loc.T("höchste Stufe") : Loc.F("Erfahrung {0}/{1}", _mana.XP, _mana.XPToNextLevel);
                if (manaText != null)
                    manaText.text = Loc.F("Mana {0}/{1}   Mana-Stufe {2}/{3} ({4})", Mathf.FloorToInt(_mana.Current), _mana.Max, _mana.Level, PlayerMana.MaxLevel, next) + "\n" +
                                    Loc.F("Füllt sich mit {0:0.0} pro Sekunde. Meditieren (M / B halten): {1:0} pro Sekunde und Mana-Training.", _mana.RegenRate, _mana.meditateRate);
                if (manaXpFill != null)
                    manaXpFill.fillAmount = _mana.Level >= PlayerMana.MaxLevel ? 1f : (float)_mana.XP / _mana.XPToNextLevel;
            }
        }
    }
}
