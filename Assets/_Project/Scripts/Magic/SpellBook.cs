using Reborn.Core;
using System;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Player;
using Reborn.Progression;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.Magic
{
    /// <summary>
    /// Die 4 Zauber des Spielers: Stufe 1-10 und Erfahrung je Zauber, Freischaltung, Abklingzeit, Zaubern.
    /// Tasten: Spell1-4 (1-4 / RB RT LB LT). Zauber fliegen in Blickrichtung.
    /// </summary>
    public class SpellBook : MonoBehaviour
    {
        public const int SlotCount = 4;
        public const int MaxLevel = 10;

        public InputActionAsset actions;
        public SpellData[] spells = new SpellData[SlotCount];
        public int[] levels = { 1, 1, 1, 1 };
        public int[] xps = new int[SlotCount];
        public LayerMask enemyMask;
        public LayerMask wallMask;
        /// <summary>Startpunkt ueber den Fuessen (Hoehe der Haende)</summary>
        public Vector2 castOffset = new Vector2(0f, 0.45f);
        public float castLock = 0.25f;
        /// <summary>Leuchten an der Hand beim Zaubern</summary>
        public GameObject castFlash;

        /// <summary>Zauber wurde gesprochen (fuer Ausruf, Banner)</summary>
        public event Action<SpellData, int> OnCast;
        public event Action<int> OnLevelUp;

        /// <summary>Warum der letzte Versuch nicht geklappt hat (fuer Anzeige und Tests)</summary>
        public string LastFail { get; private set; } = "";
        public int CastCount { get; private set; }

        PlayerMana _mana;
        PlayerStats _stats;
        PlayerProgression _progression;
        PlayerController _controller;
        Health _health;
        DirectionalSpriteAnimator _animator;
        readonly float[] _cooldowns = new float[SlotCount];
        readonly InputAction[] _actions = new InputAction[SlotCount];

        void Awake()
        {
            _mana = GetComponent<PlayerMana>();
            _stats = GetComponent<PlayerStats>();
            _progression = GetComponent<PlayerProgression>();
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
            if (levels == null || levels.Length != SlotCount) levels = new[] { 1, 1, 1, 1 };
            if (xps == null || xps.Length != SlotCount) xps = new int[SlotCount];
            if (actions != null)
            {
                var map = actions.FindActionMap("Gameplay", true);
                for (int i = 0; i < SlotCount; i++) _actions[i] = map.FindAction("Spell" + (i + 1), true);
            }
        }

        void OnEnable()
        {
            if (_actions[0] != null) _actions[0].actionMap.Enable();
        }

        void Update()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (_cooldowns[i] > 0f) _cooldowns[i] -= Time.deltaTime;
                if (_actions[i] != null && _actions[i].WasPressedThisFrame()) TryCast(i);
            }
        }

        int Intelligence => _stats != null ? _stats.GetAttribute(PlayerAttribute.Intelligenz) : 1;

        public SpellData Get(int slot) => slot >= 0 && slot < SlotCount ? spells[slot] : null;
        public int Level(int slot) => levels[slot];
        public int XP(int slot) => xps[slot];
        public int XPToNextLevel(int slot) => 10 * levels[slot];
        public float CooldownLeft(int slot) => Mathf.Max(0f, _cooldowns[slot]);
        public float LevelFactor(int slot) => 1f + 0.25f * (levels[slot] - 1);
        public float Scale(int slot) => 1f + 0.05f * (levels[slot] - 1);

        /// <summary>(Grundschaden + Intelligenz) x (1 + 0,25 x (Stufe - 1))</summary>
        public int Damage(int slot)
        {
            var s = Get(slot);
            return s == null ? 0 : Mathf.RoundToInt((s.baseDamage + Intelligence) * LevelFactor(slot));
        }

        public int SplashDamage(int slot)
        {
            var s = Get(slot);
            return s == null || s.splashDamage <= 0 ? 0 : Mathf.RoundToInt((s.splashDamage + Intelligence) * LevelFactor(slot));
        }

        public int IndexOf(string id)
        {
            for (int i = 0; i < SlotCount; i++)
                if (spells[i] != null && spells[i].id == id) return i;
            return -1;
        }

        public bool IsUnlocked(int slot)
        {
            var s = Get(slot);
            if (s == null) return false;
            if (_progression != null && _progression.Level < s.unlockPlayerLevel) return false;
            if (!string.IsNullOrEmpty(s.unlockSpellId))
            {
                int other = IndexOf(s.unlockSpellId);
                if (other < 0 || levels[other] < s.unlockSpellLevel) return false;
            }
            return true;
        }

        /// <summary>z. B. "Ab Wasserkugel Stufe 3" oder "Ab Level 2"</summary>
        public string UnlockText(int slot)
        {
            var s = Get(slot);
            if (s == null) return "";
            if (!string.IsNullOrEmpty(s.unlockSpellId))
            {
                int other = IndexOf(s.unlockSpellId);
                string name = other >= 0 ? Loc.T(spells[other].spellName) : s.unlockSpellId;
                return Loc.F("Ab {0} Stufe {1}", name, s.unlockSpellLevel);
            }
            return s.unlockPlayerLevel > 1 ? Loc.F("Ab Level {0}", s.unlockPlayerLevel) : "";
        }

        public bool TryCast(int slot)
        {
            var s = Get(slot);
            if (s == null || s.effectPrefab == null) { LastFail = "fehlt"; return false; }
            if (_health != null && _health.IsDead) { LastFail = "tot"; return false; }
            if (World.SceneRules.Peaceful) { LastFail = "friedlich"; return false; }   // Basis (Meilenstein 6)
            if (!IsUnlocked(slot)) { LastFail = "gesperrt"; return false; }
            if (_cooldowns[slot] > 0f) { LastFail = "abklingzeit"; return false; }
            if (_controller != null && _controller.Busy) { LastFail = "beschaeftigt"; return false; }
            if (_mana != null && !_mana.Spend(s.manaCost)) { LastFail = "mana"; return false; }

            LastFail = "";
            CastCount++;
            _cooldowns[slot] = s.cooldown;
            Vector2 dir = _controller != null ? _controller.FacingDirection : Vector2.down;
            if (_controller != null) _controller.StartCast(castLock);
            if (_animator != null) _animator.Play("cast", true);

            Vector2 origin = (Vector2)transform.position + castOffset + (s.kind == SpellKind.Ring ? Vector2.zero : dir * 0.35f);
            var effect = Instantiate(s.effectPrefab, origin, Quaternion.identity);
            effect.Launch(new SpellCast
            {
                book = this, slot = slot, data = s, origin = s.kind == SpellKind.Ring ? (Vector2)transform.position + new Vector2(0f, 0.15f) : origin,
                direction = dir, damage = Damage(slot), splashDamage = SplashDamage(slot), scale = Scale(slot), level = levels[slot],
                enemyMask = enemyMask, wallMask = wallMask
            });
            // Leuchten an der Hand erst nach dem Effekt: ein Fehler hier darf den Zauber nicht verhindern
            if (castFlash != null)
            {
                var flash = Instantiate(castFlash, origin, Quaternion.identity);
                foreach (var ps in flash.GetComponentsInChildren<ParticleSystem>())
                {
                    var main = ps.main;
                    main.startColor = SpellData.ElementColor(s.element);
                }
                Destroy(flash, 1f);
            }
            OnCast?.Invoke(s, levels[slot]);
            return true;
        }

        /// <summary>Ein Treffer gibt Zauber-Erfahrung; naechste Stufe braucht 10 x Stufe.</summary>
        public void RegisterHit(int slot)
        {
            var s = Get(slot);
            if (s == null || levels[slot] >= MaxLevel) return;
            xps[slot] += Mathf.Max(1, s.xpPerHit);
            AddXPInternal(slot);
        }

        /// <summary>Erfahrung direkt geben (Tests).</summary>
        public void AddXP(int slot, int amount)
        {
            if (slot < 0 || slot >= SlotCount || amount <= 0 || levels[slot] >= MaxLevel) return;
            xps[slot] += amount;
            AddXPInternal(slot);
        }

        void AddXPInternal(int slot)
        {
            while (levels[slot] < MaxLevel && xps[slot] >= XPToNextLevel(slot))
            {
                xps[slot] -= XPToNextLevel(slot);
                levels[slot]++;
                OnLevelUp?.Invoke(slot);
            }
            if (levels[slot] >= MaxLevel) xps[slot] = 0;
        }

        /// <summary>Spielstand laden.</summary>
        public void SetState(int slot, int level, int xp)
        {
            if (slot < 0 || slot >= SlotCount) return;
            levels[slot] = Mathf.Clamp(level, 1, MaxLevel);
            xps[slot] = levels[slot] >= MaxLevel ? 0 : Mathf.Clamp(xp, 0, XPToNextLevel(slot) - 1);
        }

        public void ResetCooldowns()
        {
            for (int i = 0; i < SlotCount; i++) _cooldowns[i] = 0f;
        }
    }
}
