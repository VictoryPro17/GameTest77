using System;
using Reborn.Combat;
using Reborn.Progression;
using Reborn.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.Magic
{
    /// <summary>
    /// Mana des Spielers. Fuellt sich langsam auf, beim Meditieren (Taste halten) schnell.
    /// Ausgegebenes Mana und Meditieren geben Mana-Erfahrung: Mana-Stufe 1-10 erhoeht das Maximum.
    /// </summary>
    public class PlayerMana : MonoBehaviour
    {
        public const int MaxLevel = 10;

        public InputActionAsset actions;
        public int baseMana = 20;
        public int manaPerIntelligence = 5;
        public int manaPerLevel = 8;
        public float regenBase = 1f;
        public float regenPerIntelligence = 0.2f;
        public float meditateRate = 5f;
        /// <summary>Wird beim Meditieren eingeschaltet (Partikel-Aura)</summary>
        public GameObject meditateAura;

        public float Current { get; private set; }
        public int Level { get; private set; } = 1;
        public int XP { get; private set; }
        public int XPToNextLevel => 50 * Level;
        public bool Meditating { get; private set; }

        public int Max => baseMana + manaPerIntelligence * Intelligence + manaPerLevel * (Level - 1);
        public float RegenRate => regenBase + regenPerIntelligence * Intelligence;

        public event Action OnLevelUp;
        /// <summary>Zauber wollte mehr Mana, als da ist</summary>
        public event Action OnNotEnough;

        PlayerStats _stats;
        Health _health;
        Player.PlayerController _controller;
        InputAction _meditate;
        float _meditateXp;
        bool _initialized;

        int Intelligence => _stats != null ? _stats.GetAttribute(PlayerAttribute.Intelligenz) : 1;

        void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _health = GetComponent<Health>();
            _controller = GetComponent<Player.PlayerController>();
            if (actions != null) _meditate = actions.FindActionMap("Gameplay", true).FindAction("Meditate", true);
            if (meditateAura != null) meditateAura.SetActive(false);
        }

        void OnEnable()
        {
            if (_meditate != null) _meditate.actionMap.Enable();
        }

        void Start()
        {
            if (!_initialized) Current = Max;
            _initialized = true;
        }

        void Update()
        {
            bool alive = _health == null || !_health.IsDead;
            bool wantsMeditate = alive && !GameMenu.IsOpen && _meditate != null && _meditate.enabled && _meditate.IsPressed();
            SetMeditating(wantsMeditate);

            float rate = Meditating ? meditateRate : RegenRate;
            if (alive) Current = Mathf.Min(Max, Current + rate * Time.deltaTime);

            if (Meditating)
            {
                _meditateXp += Time.deltaTime;
                if (_meditateXp >= 1f)
                {
                    int whole = Mathf.FloorToInt(_meditateXp);
                    _meditateXp -= whole;
                    AddXP(whole);
                }
            }
        }

        void SetMeditating(bool value)
        {
            if (Meditating == value) return;
            Meditating = value;
            if (_controller != null) _controller.Meditating = value;
            if (meditateAura != null) meditateAura.SetActive(value);
        }

        /// <summary>Zieht Mana ab. false (und OnNotEnough), wenn nicht genug da ist.</summary>
        public bool Spend(int cost)
        {
            if (cost <= 0) return true;
            if (Current + 0.0001f < cost)
            {
                OnNotEnough?.Invoke();
                return false;
            }
            Current -= cost;
            AddXP(cost);
            return true;
        }

        public void AddXP(int amount)
        {
            if (amount <= 0 || Level >= MaxLevel) return;
            XP += amount;
            while (Level < MaxLevel && XP >= XPToNextLevel)
            {
                XP -= XPToNextLevel;
                Level++;
                OnLevelUp?.Invoke();
            }
            if (Level >= MaxLevel) XP = 0;
        }

        /// <summary>Mana direkt setzen (Spielstand, Tests). current &lt; 0 = voll.</summary>
        public void SetState(int level, int xp, float current)
        {
            Level = Mathf.Clamp(level, 1, MaxLevel);
            XP = Level >= MaxLevel ? 0 : Mathf.Clamp(xp, 0, XPToNextLevel - 1);
            Current = current < 0f ? Max : Mathf.Clamp(current, 0f, Max);
            _initialized = true;
        }

        public void SetCurrent(float value) => Current = Mathf.Clamp(value, 0f, Max);
    }
}
