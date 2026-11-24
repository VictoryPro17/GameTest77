using System;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Loot;
using Reborn.Progression;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Eisdrache (Meilenstein 9a), Boss des Frostgipfels. Schlaeft, bis der Spieler naeher als wakeRadius kommt.
    /// Eisatem: Warn-Kegel 1 s, dann Kegel-Schaden. Eisregen: 5 Warnkreise, Eiszapfen fallen. Fluegelschlag: Rueckstoss-Welle (nah).
    /// Ab 50 % Leben einmal: fliegt 5 s hoch (unverwundbar) und ruft 2 Frostgeister.
    /// Ultimate "Blizzard": 5 s Aufladung, trifft die ganze Arena mit 60 % des Lebens - ausser hinter einer Eissaeule (IcePillar).
    /// Erscheint wie der Golem nach 10 Minuten wieder (SaveSystem).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public class DragonAI : MonoBehaviour, IBossBarSource
    {
        enum State { Sleep, Roar, Chase, BreathAim, Breath, RainCast, WingWindup, WingCast, Fly, UltimateCharge, Recover, Dead }

        public string bossId = "eisdrache";
        public string displayName = "Eisdrache";
        public float wakeRadius = 4.5f;
        public float speed = 1.6f;
        public float stopDistance = 1.8f;
        public int contactDamage = 20;
        public float contactCooldown = 1f;
        public int xpReward = 1000;
        public float despawnDelay = 3f;

        [Header("Eisatem")]
        public ChargeTelegraph breathWarningPrefab;
        public ChargeTelegraph breathConePrefab;
        public float breathRange = 4.5f;
        public float breathLength = 4.2f;
        public float breathHalfAngle = 30f;
        public float breathAimTime = 1f;
        public float breathCooldown = 5f;
        public int breathDamage = 30;
        public Vector2 headOffset = new Vector2(0f, 0.5f);

        [Header("Eisregen")]
        public RootSpike iciclePrefab;
        public int icicleCount = 5;
        public float icicleSpread = 3f;
        public float rainCooldown = 9f;

        [Header("Fluegelschlag")]
        public ThornWave wingWavePrefab;
        public float wingRange = 2.6f;
        public float wingWindup = 0.5f;
        public float wingCooldown = 6f;

        [Header("Flugphase (ab 50 %)")]
        public GameObject spiritPrefab;
        public int spiritCount = 2;
        public float flyTime = 5f;

        [Header("Blizzard")]
        public UltimateZone blizzardPrefab;
        public float blizzardFirstDelay = 25f;
        public float blizzardCooldown = 35f;
        public float blizzardChargeTime = 5f;
        public float blizzardDamageShare = 0.6f;

        public LootTable repeatLoot;

        public static DragonAI Active { get; private set; }
        public static event Action<string> OnDefeated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active = null;

        public Health Health => _health;
        public bool IsAwake => _state != State.Sleep;
        public string StateName => _state.ToString();
        public bool HasFlown { get; private set; }
        public bool IsFlying => _state == State.Fly;
        public bool FirstKill { get; set; } = true;
        public int BreathCount { get; private set; }
        public int BreathHits { get; private set; }
        public int RainCount { get; private set; }
        public int WingCount { get; private set; }
        public int WingHits { get; private set; }
        public int BlizzardCount { get; private set; }
        public int BlizzardHits { get; private set; }
        public int BlizzardBlocked { get; private set; }
        /// <summary>Breath-Richtung der letzten Attacke (fuer Tests)</summary>
        public Vector2 BreathDirection => _breathDirection;

        // IBossBarSource
        public string BarName => displayName + (IsFlying ? "  (in der Luft)" : HasFlown ? "  (Phase 2)" : "");
        public bool ShowUltimateBar => IsAwake;
        public bool UltimateCharging => _state == State.UltimateCharge;
        public float UltimateProgress => UltimateCharging ? 1f - _timer / blizzardChargeTime
            : Mathf.Clamp01(1f - _blizzardTimer / Mathf.Max(0.01f, _blizzardWait));
        public string WarningText => "Blizzard!  Versteck dich hinter einer Eissäule!";

        State _state = State.Sleep;
        Rigidbody2D _body;
        Health _health;
        DirectionalSpriteAnimator _animator;
        Transform _player;
        Health _playerHealth;
        float _timer;
        float _contactTimer;
        float _breathTimer = 2f;
        float _rainTimer = 4f;
        float _wingTimer = 2f;
        float _blizzardTimer;
        float _blizzardWait = 1f;
        Vector2 _breathDirection = Vector2.down;
        UltimateZone _blizzard;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
        }

        void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDied += Die;
        }

        void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= Die;
            if (Active == this) Active = null;
        }

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _player = player.transform;
            _playerHealth = player.GetComponent<Health>();
        }

        /// <summary>Noch nicht wieder erschienen (Spielstand): ohne Beute entfernen</summary>
        public void RemoveDefeated()
        {
            _state = State.Dead;
            if (Active == this) Active = null;
            Destroy(gameObject);
        }

        public void Wake()
        {
            if (_state != State.Sleep) return;
            Active = this;
            _blizzardTimer = blizzardFirstDelay;
            _blizzardWait = blizzardFirstDelay;
            Enter(State.Roar, 1f, "roar");
        }

        void Enter(State state, float time, string anim)
        {
            _state = state;
            _timer = time;
            _body.linearVelocity = Vector2.zero;
            if (anim != null && _animator != null) _animator.Play(anim, true);
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            if (_health.IsDead) return;
            if (_state == State.Sleep) Wake();
            if (!HasFlown && _health.Current <= _health.maxHealth / 2 && _state != State.UltimateCharge) StartFly();
        }

        // ------------------------------------------------------------ Ablauf
        void Update()
        {
            float dt = Time.deltaTime;
            _contactTimer -= dt;
            if (_state == State.Dead) return;
            if (_state != State.Sleep && _state != State.Fly)
            {
                _breathTimer -= dt;
                _rainTimer -= dt;
                _wingTimer -= dt;
                if (_state != State.UltimateCharge) _blizzardTimer -= dt;
            }
            bool playerAlive = _player != null && (_playerHealth == null || !_playerHealth.IsDead);
            Vector2 toPlayer = playerAlive ? (Vector2)(_player.position - transform.position) : Vector2.zero;
            float dist = toPlayer.magnitude;

            switch (_state)
            {
                case State.Sleep:
                    if (_animator != null) _animator.Play("idle");
                    if (playerAlive && dist <= wakeRadius) Wake();
                    return;
                case State.Roar:
                case State.Breath:
                case State.RainCast:
                case State.WingCast:
                case State.Recover:
                    _timer -= dt;
                    if (_timer <= 0f) _state = State.Chase;
                    return;
                case State.BreathAim:
                    _timer -= dt;
                    if (_timer <= 0f) ReleaseBreath();
                    return;
                case State.WingWindup:
                    _timer -= dt;
                    if (_timer <= 0f) ReleaseWing();
                    return;
                case State.Fly:
                    _timer -= dt;
                    if (_timer <= 0f) Land();
                    return;
                case State.UltimateCharge:
                    _timer -= dt;
                    if (_timer <= 0f) ReleaseBlizzard();
                    return;
            }

            if (!playerAlive)
            {
                if (_animator != null) _animator.Play("idle");
                return;
            }
            if (_animator != null)
            {
                _animator.SetFacing(toPlayer);
                _animator.Play(dist > stopDistance ? "walk" : "idle");
            }
            if (_blizzardTimer <= 0f) { StartBlizzard(); return; }
            if (dist <= wingRange && _wingTimer <= 0f) { StartWing(toPlayer); return; }
            if (dist <= breathRange && _breathTimer <= 0f) { StartBreath(toPlayer); return; }
            if (_rainTimer <= 0f) StartRain();
        }

        // ------------------------------------------------------------ Eisatem
        Vector2 HeadPosition => (Vector2)transform.position + headOffset;

        void StartBreath(Vector2 toPlayer)
        {
            _breathDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector2.down;
            _breathTimer = breathCooldown;
            Enter(State.BreathAim, breathAimTime, "roar");
            if (_animator != null) _animator.SetFacing(_breathDirection);
            if (breathWarningPrefab != null)
                Instantiate(breathWarningPrefab, HeadPosition, Quaternion.identity).Setup(_breathDirection, breathLength, breathAimTime);
        }

        /// <summary>Steht ein Punkt im Atem-Kegel?</summary>
        public bool InBreathCone(Vector2 point)
        {
            Vector2 d = point - HeadPosition;
            if (d.magnitude > breathLength) return false;
            return Vector2.Angle(_breathDirection, d) <= breathHalfAngle || d.magnitude < 0.6f;
        }

        void ReleaseBreath()
        {
            BreathCount++;
            Enter(State.Breath, 0.7f, "breath");
            if (_animator != null) _animator.SetFacing(_breathDirection);
            if (breathConePrefab != null)
                Instantiate(breathConePrefab, HeadPosition, Quaternion.identity).Setup(_breathDirection, breathLength, 0.6f);
            if (_playerHealth == null || _playerHealth.IsDead) return;
            if (InBreathCone((Vector2)_player.position + new Vector2(0f, 0.3f)) && _playerHealth.TakeDamage(breathDamage, _breathDirection * 6f))
                BreathHits++;
        }

        // ------------------------------------------------------------ Eisregen
        void StartRain()
        {
            RainCount++;
            _rainTimer = rainCooldown;
            Enter(State.RainCast, 0.8f, "roar");
            if (iciclePrefab == null || _player == null) return;
            Instantiate(iciclePrefab, _player.position, Quaternion.identity);
            for (int i = 1; i < icicleCount; i++)
            {
                Vector2 off = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(1.4f, icicleSpread);
                Instantiate(iciclePrefab, (Vector2)_player.position + off, Quaternion.identity);
            }
        }

        // ------------------------------------------------------------ Fluegelschlag
        void StartWing(Vector2 toPlayer)
        {
            _wingTimer = wingCooldown;
            Enter(State.WingWindup, wingWindup, "wing");
            if (_animator != null) _animator.SetFacing(toPlayer);
        }

        void ReleaseWing()
        {
            WingCount++;
            int before = ThornWave.HitCount;
            Enter(State.WingCast, 0.6f, null);
            if (wingWavePrefab != null) Instantiate(wingWavePrefab, transform.position, Quaternion.identity);
            // Treffer zaehlt die Welle selbst (ThornWave.HitCount); hier nur der Rueckstoss-Zaehler fuer Tests
            _wingHitBase = before;
        }

        int _wingHitBase = -1;

        void LateUpdate()
        {
            if (_wingHitBase >= 0 && ThornWave.HitCount > _wingHitBase) { WingHits++; _wingHitBase = -1; }
        }

        // ------------------------------------------------------------ Flugphase
        void StartFly()
        {
            HasFlown = true;
            _health.invulnerable = true;
            if (_blizzard != null) { Destroy(_blizzard.gameObject); _blizzard = null; }
            Enter(State.Fly, flyTime, "fly");
            if (spiritPrefab == null) return;
            for (int i = 0; i < spiritCount; i++)
            {
                float a = i * Mathf.PI * 2f / spiritCount + 0.6f;
                var pos = (Vector2)transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.6f - 0.5f) * 2.2f;
                var s = Instantiate(spiritPrefab, pos, Quaternion.identity);
                s.name = "Dragon_Spirit_" + i;
            }
        }

        void Land()
        {
            _health.invulnerable = false;
            Enter(State.Roar, 0.8f, "roar");
        }

        // ------------------------------------------------------------ Blizzard
        void StartBlizzard()
        {
            Enter(State.UltimateCharge, blizzardChargeTime, "wing");
            if (blizzardPrefab != null)
            {
                _blizzard = Instantiate(blizzardPrefab, transform.position, Quaternion.identity);
                _blizzard.Setup(blizzardChargeTime);
            }
        }

        /// <summary>Steht eine Eissaeule zwischen Drache und Punkt?</summary>
        public bool IsSheltered(Vector2 point)
        {
            Vector2 from = (Vector2)transform.position + new Vector2(0f, 0.3f);
            foreach (var hit in Physics2D.LinecastAll(from, point))
                if (hit.collider != null && hit.collider.GetComponentInParent<IcePillar>() != null) return true;
            return false;
        }

        void ReleaseBlizzard()
        {
            BlizzardCount++;
            _blizzardWait = blizzardCooldown;
            _blizzardTimer = blizzardCooldown;
            Enter(State.Recover, 0.8f, "roar");
            if (_blizzard != null) _blizzard.Release();
            _blizzard = null;
            if (_playerHealth == null || _playerHealth.IsDead) return;
            if (IsSheltered((Vector2)_player.position + new Vector2(0f, 0.3f))) { BlizzardBlocked++; return; }
            if (_playerHealth.TakeDamage(Mathf.CeilToInt(_playerHealth.maxHealth * blizzardDamageShare), Vector2.zero)) BlizzardHits++;
        }

        // ------------------------------------------------------------ Tod
        void Die()
        {
            _state = State.Dead;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            if (_blizzard != null) Destroy(_blizzard.gameObject);
            if (Active == this) Active = null;
            if (_animator != null) _animator.Play("death", true);
            foreach (var a in FindObjectsByType<SlimeAI>()) if (a.name.StartsWith("Dragon_Spirit")) a.gameObject.SetActive(false);
            var dropper = GetComponent<LootDropper>();
            if (dropper != null)
            {
                if (!FirstKill && repeatLoot != null) dropper.table = repeatLoot;
                dropper.Drop();
            }
            if (_player != null)
            {
                var progression = _player.GetComponent<PlayerProgression>();
                if (progression != null) progression.AddXP(xpReward);
            }
            OnDefeated?.Invoke(bossId);
            Destroy(gameObject, despawnDelay);
        }

        /// <summary>Fuer Tests: Wartezeiten setzen (Sekunden, negativ = unveraendert)</summary>
        public void SetTimers(float blizzard, float breath, float rain, float wing)
        {
            if (blizzard >= 0f) { _blizzardTimer = blizzard; _blizzardWait = Mathf.Max(blizzard, 0.01f); }
            if (breath >= 0f) _breathTimer = breath;
            if (rain >= 0f) _rainTimer = rain;
            if (wing >= 0f) _wingTimer = wing;
        }

        void FixedUpdate()
        {
            if (_state == State.Chase && _player != null && (_playerHealth == null || !_playerHealth.IsDead))
            {
                Vector2 to = _player.position - transform.position;
                _body.linearVelocity = to.magnitude > stopDistance ? to.normalized * speed : Vector2.zero;
            }
            else _body.linearVelocity = Vector2.zero;
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (_state == State.Dead || _state == State.Sleep || _state == State.Fly || _contactTimer > 0f) return;
            var target = collision.collider.GetComponentInParent<Health>();
            if (target == null || target != _playerHealth) return;
            Vector2 direction = ((Vector2)(collision.transform.position - transform.position)).normalized;
            if (target.TakeDamage(contactDamage, direction * 6f)) _contactTimer = contactCooldown;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, wakeRadius);
        }
    }
}
