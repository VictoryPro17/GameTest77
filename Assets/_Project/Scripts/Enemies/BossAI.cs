using System;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Loot;
using Reborn.Progression;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Golem-Boss "Waldwaechter" (Meilenstein 8, Stufe 2 in 8b). Schlaeft, bis der Spieler naeher als wakeRadius kommt.
    /// Stufe 1: Schlag (nah), Wurzelangriff (fern); 50 % Leben: Bruellen + 3 Woelfe (Phase 2); 25 %: Wut, 3 Wurzeln (Phase 3).
    /// Bei 0 Leben erwacht er als "Uralter Waldwaechter" (Stufe 2, 3 s unverwundbar): mehr Leben, schneller, Schlag 35,
    /// Dornenwelle, Ansturm, Wolfsruf (verwundete Woelfe) und die Ultimate "Zorn des Waldes" (4 s Aufladung, 50 % Leben).
    /// Erst nach Stufe 2 gibt es Beute; er erscheint nach 10 Minuten wieder (SaveSystem).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public class BossAI : MonoBehaviour
    {
        enum State { Sleep, Chase, Windup, Strike, Root, Roar, Transform, ThornCast, ChargeAim, Charging, UltimateCharge, Dead }

        public string bossId = "waldwaechter";
        public string displayName = "Waldwächter";
        public string stage2Name = "Uralter Waldwächter";
        public float wakeRadius = 4f;
        public float speed = 1.2f;
        public float speedPhase2 = 1.8f;
        public int contactDamage = 20;
        public float contactCooldown = 1f;
        public int slamDamage = 30;
        public float slamRange = 1.7f;
        public float slamRadius = 1.2f;
        public float slamWindup = 0.5f;
        public float slamWindupRage = 0.3f;
        public float slamCooldown = 2.2f;
        public float slamCooldownPhase2 = 1.4f;
        public float rootMinDistance = 2.4f;
        public float rootCooldown = 4f;
        public float rootCooldownPhase2 = 2.6f;
        public int xpReward = 600;
        public Vector2 bodyOffset = new Vector2(0f, 0.6f);
        public RootSpike rootSpikePrefab;
        public GameObject wolfPrefab;
        public int wolvesPhase2 = 3;
        public float despawnDelay = 3f;

        [Header("Stufe 2 (Meilenstein 8b)")]
        public SpriteAnimationSet stage2Set;
        public int stage2Health = 1500;
        public float stage2Speed = 2.1f;
        public int stage2SlamDamage = 35;
        public float transformTime = 3f;
        public ThornWave thornWavePrefab;
        public float thornCooldown = 6f;
        public float thornRange = 4f;
        public ChargeTelegraph chargeTelegraphPrefab;
        public float chargeCooldown = 7f;
        public float chargeAimTime = 1f;
        public float chargeSpeed = 9f;
        public float chargeDuration = 0.7f;
        public int chargeDamage = 30;
        public UltimateZone ultimatePrefab;
        public float ultimateFirstDelay = 20f;
        public float ultimateCooldown = 30f;
        public float ultimateChargeTime = 4f;
        public float wolfCallCooldown = 15f;
        public int wolfCallCount = 2;
        public int wolfCallHealth = 20;
        /// <summary>Beute ab dem zweiten Sieg (ohne Amulett)</summary>
        public LootTable repeatLoot;

        /// <summary>Wacher, lebender Boss in der Szene (fuer die Lebensleiste), sonst null</summary>
        public static BossAI Active { get; private set; }
        /// <summary>Boss endgueltig besiegt (nach Stufe 2): ID</summary>
        public static event Action<string> OnDefeated;

        public int Phase { get; private set; } = 1;
        public int Stage { get; private set; } = 1;
        public bool IsAwake => _state != State.Sleep;
        public string StateName => _state.ToString();
        public Health Health => _health;
        public string CurrentName => Stage >= 2 ? stage2Name : displayName;
        /// <summary>Erster Sieg? (false = Wiederholung, Beute ohne Amulett) - setzt das SaveSystem beim Laden</summary>
        public bool FirstKill { get; set; } = true;
        public int SlamCount { get; private set; }
        public int RootCount { get; private set; }
        public int ThornCount { get; private set; }
        public int ChargeCount { get; private set; }
        public int UltimateCount { get; private set; }
        public int WolfCalls { get; private set; }
        public int ChargeHits { get; private set; }
        public bool UltimateCharging => _state == State.UltimateCharge;
        /// <summary>0..1: Aufladung der Ultimate (waehrend sie laedt) bzw. Bereitschaft (sonst), nur Stufe 2</summary>
        public float UltimateProgress => Stage < 2 ? 0f : UltimateCharging ? 1f - _timer / ultimateChargeTime
            : Mathf.Clamp01(1f - _ultimateTimer / Mathf.Max(0.01f, _ultimateWait));

        State _state = State.Sleep;
        Rigidbody2D _body;
        Health _health;
        DirectionalSpriteAnimator _animator;
        Transform _player;
        Health _playerHealth;
        float _timer;
        float _slamTimer;
        float _rootTimer = 1.5f;
        float _contactTimer;
        float _thornTimer = 3f;
        float _chargeTimer = 5f;
        float _ultimateTimer;
        float _ultimateWait = 1f;
        float _wolfTimer;
        Vector2 _slamDirection = Vector2.down;
        Vector2 _chargeDirection = Vector2.down;
        bool _chargeHit;
        UltimateZone _ultimate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active = null;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
        }

        void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDied += HandleDied;
        }

        void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= HandleDied;
            if (Active == this) Active = null;
        }

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _player = player.transform;
            _playerHealth = player.GetComponent<Health>();
        }

        /// <summary>Noch nicht wieder erschienen (Spielstand): Boss entfernen, ohne Beute</summary>
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
            StartRoar(1f);
        }

        void StartRoar(float time)
        {
            _state = State.Roar;
            _timer = time;
            _body.linearVelocity = Vector2.zero;
            if (_animator != null) _animator.Play("roar", true);
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            if (_health.IsDead || Stage >= 2) { if (_state == State.Sleep) Wake(); return; }
            if (_state == State.Sleep) Wake();
            float ratio = (float)_health.Current / _health.maxHealth;
            if (Phase == 1 && ratio <= 0.5f)
            {
                Phase = 2;
                StartRoar(1f);
                SpawnWolves(wolvesPhase2, 0, "Boss_Wolf_");
            }
            else if (Phase == 2 && ratio <= 0.25f)
            {
                Phase = 3;
                StartRoar(1f);
            }
        }

        void SpawnWolves(int count, int health, string prefix)
        {
            if (wolfPrefab == null) return;
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count + Time.time;
                var pos = (Vector2)transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.6f - 0.4f) * 1.8f;
                var wolf = Instantiate(wolfPrefab, pos, Quaternion.identity);
                wolf.name = prefix + i;
                if (health > 0) wolf.GetComponent<Health>().SetMaxHealth(health, true);
            }
        }

        // ------------------------------------------------------------ Tod bzw. Erwachen
        void HandleDied()
        {
            if (Stage == 1) { Awaken(); return; }
            Die();
        }

        /// <summary>Stufe 1 besiegt: 3 s Verwandlung (unverwundbar), dann Stufe 2 mit vollem Leben</summary>
        void Awaken()
        {
            Stage = 2;
            Phase = 1;
            _health.SetMaxHealth(stage2Health, true);
            _health.invulnerable = true;
            if (stage2Set != null && _animator != null) _animator.animationSet = stage2Set;
            StartRoar(transformTime);
            _state = State.Transform;
            foreach (var a in FindObjectsByType<SlimeAI>()) if (a.name.StartsWith("Boss_Wolf")) a.gameObject.SetActive(false);   // alte Woelfe verschwinden
            _ultimateWait = ultimateFirstDelay;
            _ultimateTimer = ultimateFirstDelay;
            _wolfTimer = 5f;
            _thornTimer = 2.5f;
            _chargeTimer = 4f;
        }

        void Die()
        {
            _state = State.Dead;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            if (_ultimate != null) Destroy(_ultimate.gameObject);
            if (Active == this) Active = null;
            if (_animator != null) _animator.Play("death", true);
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

        // ------------------------------------------------------------ Ablauf
        void Update()
        {
            float dt = Time.deltaTime;
            _contactTimer -= dt;
            _slamTimer -= dt;
            _rootTimer -= dt;
            if (Stage >= 2 && _state != State.Transform && _state != State.Dead)
            {
                _thornTimer -= dt;
                _chargeTimer -= dt;
                if (_state != State.UltimateCharge) _ultimateTimer -= dt;
                _wolfTimer -= dt;
            }
            if (_state == State.Dead) return;
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
                case State.Strike:
                case State.Root:
                case State.ThornCast:
                    _timer -= dt;
                    if (_timer <= 0f) _state = State.Chase;
                    return;
                case State.Transform:
                    _timer -= dt;
                    if (_timer <= 0f)
                    {
                        _health.invulnerable = false;
                        _state = State.Chase;
                    }
                    return;
                case State.Windup:
                    _timer -= dt;
                    if (_timer <= 0f) Strike();
                    return;
                case State.ChargeAim:
                    _timer -= dt;
                    if (_timer <= 0f) StartCharging();
                    return;
                case State.Charging:
                    _timer -= dt;
                    // Treffer bei Beruehrung (OnCollisionStay2D) oder wenn die Mittelpunkte nah genug sind
                    if (!_chargeHit && playerAlive && dist <= 1.3f) ChargeHit();
                    if (_timer <= 0f) _state = State.Chase;
                    return;
                case State.UltimateCharge:
                    _timer -= dt;
                    if (_timer <= 0f) ReleaseUltimate();
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
                _animator.Play("walk");
            }
            if (Stage >= 2)
            {
                if (_ultimateTimer <= 0f) { StartUltimate(); return; }
                if (_wolfTimer <= 0f) { _wolfTimer = wolfCallCooldown; WolfCalls++; SpawnWolves(wolfCallCount, wolfCallHealth, "Boss_Wolf_W" + WolfCalls + "_"); StartRoar(0.8f); return; }
                if (dist <= thornRange && _thornTimer <= 0f) { StartThorns(); return; }
                if (dist > 3f && _chargeTimer <= 0f) { StartChargeAim(toPlayer); return; }
            }
            if (dist <= slamRange && _slamTimer <= 0f) StartSlam(toPlayer);
            else if (dist >= rootMinDistance && _rootTimer <= 0f) StartRoots();
        }

        // ------------------------------------------------------------ Schlag und Wurzeln (Stufe 1 und 2)
        void StartSlam(Vector2 toPlayer)
        {
            _state = State.Windup;
            _slamDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector2.down;
            _timer = Phase >= 3 || Stage >= 2 ? slamWindupRage : slamWindup;
            if (_animator != null)
            {
                _animator.SetFacing(_slamDirection);
                _animator.Play("attack", true);
            }
        }

        public Vector2 SlamCenter => (Vector2)transform.position + bodyOffset * 0.5f + _slamDirection * 0.9f;

        void Strike()
        {
            _state = State.Strike;
            _timer = 0.35f;
            SlamCount++;
            _slamTimer = Phase >= 2 || Stage >= 2 ? slamCooldownPhase2 : slamCooldown;
            if (_playerHealth == null) return;
            Vector2 playerBody = (Vector2)_player.position + new Vector2(0f, 0.3f);
            if (Vector2.Distance(playerBody, SlamCenter) <= slamRadius)
                _playerHealth.TakeDamage(Stage >= 2 ? stage2SlamDamage : slamDamage, _slamDirection * 7f);
        }

        void StartRoots()
        {
            _state = State.Root;
            _timer = 0.6f;
            RootCount++;
            _rootTimer = Phase >= 2 || Stage >= 2 ? rootCooldownPhase2 : rootCooldown;
            if (_animator != null) _animator.Play("rootattack", true);
            if (rootSpikePrefab == null || _player == null) return;
            Instantiate(rootSpikePrefab, _player.position, Quaternion.identity);
            if (Phase >= 3)
                foreach (var off in new[] { new Vector2(1.3f, 0.6f), new Vector2(-1.3f, 0.6f) })
                    Instantiate(rootSpikePrefab, (Vector2)_player.position + off, Quaternion.identity);
        }

        // ------------------------------------------------------------ Stufe 2: Dornenwelle, Ansturm, Ultimate
        void StartThorns()
        {
            _state = State.ThornCast;
            _timer = 0.8f;
            _thornTimer = thornCooldown;
            ThornCount++;
            if (_animator != null) _animator.Play("rootattack", true);
            if (thornWavePrefab != null) Instantiate(thornWavePrefab, transform.position, Quaternion.identity);
        }

        void StartChargeAim(Vector2 toPlayer)
        {
            _state = State.ChargeAim;
            _timer = chargeAimTime;
            _chargeDirection = toPlayer.normalized;
            _chargeTimer = chargeCooldown;
            if (_animator != null)
            {
                _animator.SetFacing(_chargeDirection);
                _animator.Play("roar", true);
            }
            if (chargeTelegraphPrefab != null)
            {
                var t = Instantiate(chargeTelegraphPrefab, transform.position, Quaternion.identity);
                t.Setup(_chargeDirection, chargeSpeed * chargeDuration, chargeAimTime);
            }
        }

        void StartCharging()
        {
            _state = State.Charging;
            _timer = chargeDuration;
            _chargeHit = false;
            ChargeCount++;
            if (_animator != null) _animator.Play("walk", true);
        }

        /// <summary>Ansturm trifft (hoechstens einmal pro Ansturm), danach bleibt der Boss stehen</summary>
        void ChargeHit()
        {
            _chargeHit = true;
            if (_playerHealth.TakeDamage(chargeDamage, _chargeDirection * 8f)) ChargeHits++;
            _state = State.Strike;
            _timer = 0.4f;
        }

        void StartUltimate()
        {
            _state = State.UltimateCharge;
            _timer = ultimateChargeTime;
            _body.linearVelocity = Vector2.zero;
            if (_animator != null) _animator.Play("roar", true);
            if (ultimatePrefab != null)
            {
                _ultimate = Instantiate(ultimatePrefab, transform.position, Quaternion.identity);
                _ultimate.Setup(ultimateChargeTime);
            }
        }

        void ReleaseUltimate()
        {
            UltimateCount++;
            _ultimateWait = ultimateCooldown;
            _ultimateTimer = ultimateCooldown;
            _state = State.Strike;
            _timer = 0.6f;
            if (_animator != null) _animator.Play("attack", true);
            float radius = _ultimate != null ? _ultimate.radius : 5.5f;
            if (_ultimate != null) _ultimate.Release();
            _ultimate = null;
            if (_playerHealth == null || _playerHealth.IsDead) return;
            if (Vector2.Distance(_player.position, transform.position) > radius) return;
            int damage = Mathf.CeilToInt(_playerHealth.maxHealth * 0.5f);
            _playerHealth.TakeDamage(damage, Vector2.zero);
        }

        /// <summary>Fuer Tests: Wartezeiten der Stufe-2-Faehigkeiten setzen (Sekunden, negativ = unveraendert)</summary>
        public void SetTimers(float ultimate, float wolves, float thorns, float charge, float roots = -1f)
        {
            if (ultimate >= 0f) { _ultimateTimer = ultimate; _ultimateWait = Mathf.Max(ultimate, 0.01f); }
            if (wolves >= 0f) _wolfTimer = wolves;
            if (thorns >= 0f) _thornTimer = thorns;
            if (charge >= 0f) _chargeTimer = charge;
            if (roots >= 0f) _rootTimer = roots;
        }

        void FixedUpdate()
        {
            if (_state == State.Charging)
                _body.linearVelocity = _chargeDirection * chargeSpeed;
            else if (_state == State.Chase && _player != null && (_playerHealth == null || !_playerHealth.IsDead))
                _body.linearVelocity = ((Vector2)(_player.position - transform.position)).normalized * (Stage >= 2 ? stage2Speed : Phase >= 2 ? speedPhase2 : speed);
            else
                _body.linearVelocity = Vector2.zero;
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (_state == State.Dead || _state == State.Sleep || _state == State.Transform) return;
            var target = collision.collider.GetComponentInParent<Health>();
            if (target == null || target != _playerHealth) return;
            if (_state == State.Charging && !_chargeHit) { ChargeHit(); return; }
            if (_contactTimer > 0f) return;
            Vector2 direction = ((Vector2)(collision.transform.position - transform.position)).normalized;
            if (target.TakeDamage(contactDamage, direction * 6f)) _contactTimer = contactCooldown;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, wakeRadius);
            Gizmos.DrawWireSphere(SlamCenter, slamRadius);
        }
    }
}
