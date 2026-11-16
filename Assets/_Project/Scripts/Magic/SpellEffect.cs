using Reborn.Combat;
using UnityEngine;

namespace Reborn.Magic
{
    /// <summary>Alles, was ein Zauber-Effekt beim Start wissen muss.</summary>
    public struct SpellCast
    {
        public SpellBook book;
        public int slot;
        public SpellData data;
        public Vector2 origin;
        public Vector2 direction;
        public int damage;
        public int splashDamage;
        /// <summary>Groesse: 1 + 5 % pro Stufe ueber 1</summary>
        public float scale;
        public int level;
        public LayerMask enemyMask;
        public LayerMask wallMask;
    }

    /// <summary>Grundklasse fuer Zauber-Effekte (Projektil, Drache, Ring).</summary>
    public abstract class SpellEffect : MonoBehaviour
    {
        /// <summary>Partikel-Spritzer beim Treffer</summary>
        public GameObject hitEffect;
        /// <summary>Goldener Rand ab Stufe 10 (ist im Prefab ausgeschaltet)</summary>
        public GameObject goldAura;
        /// <summary>Partikel-Schweife, die nach dem Ende noch ausklingen duerfen</summary>
        public ParticleSystem[] trails = new ParticleSystem[0];

        protected SpellCast Cast;

        public void Launch(SpellCast cast)
        {
            Cast = cast;
            transform.position = cast.origin;
            if (goldAura != null) goldAura.SetActive(cast.level >= SpellBook.MaxLevel);
            OnLaunch();
        }

        protected abstract void OnLaunch();

        /// <summary>Schaden austeilen. Zaehlt fuer die Zauber-Erfahrung, wenn countXp.</summary>
        protected bool Hit(Health target, int damage, Vector2 knockback, bool countXp)
        {
            if (target == null || target.IsDead) return false;
            UI.DamageNumbers.PendingColor = SpellData.ElementColor(Cast.data.element);
            bool hit = target.TakeDamage(damage, knockback);
            UI.DamageNumbers.PendingColor = null;
            if (!hit) return false;
            if (countXp && Cast.book != null) Cast.book.RegisterHit(Cast.slot);
            SpawnHit(target.transform.position + new Vector3(0f, 0.25f, 0f), 1f);
            return true;
        }

        protected void SpawnHit(Vector3 position, float scale)
        {
            if (hitEffect == null) return;
            var fx = Instantiate(hitEffect, position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * Cast.scale * scale;
            Destroy(fx, 1.5f);
        }

        /// <summary>Schweife abloesen, damit sie nach dem Zerstoeren noch ausklingen.</summary>
        protected void ReleaseTrails()
        {
            foreach (var t in trails)
            {
                if (t == null) continue;
                t.transform.SetParent(null, true);
                t.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(t.gameObject, 1.5f);
            }
        }

        protected static float Angle(Vector2 dir) => Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}
