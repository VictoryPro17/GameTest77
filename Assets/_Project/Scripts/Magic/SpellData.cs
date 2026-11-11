using UnityEngine;

namespace Reborn.Magic
{
    /// <summary>Beschreibt einen Zauber (Daten, kein Objekt in der Szene).</summary>
    [CreateAssetMenu(menuName = "Reborn/Spell", fileName = "NewSpell")]
    public class SpellData : ScriptableObject
    {
        public string id;
        public string spellName;
        public SpellElement element;
        public SpellKind kind;
        public Sprite icon;
        [TextArea] public string description;

        public int manaCost = 5;
        public int baseDamage = 8;
        /// <summary>Explosionsschaden (Feuerball), skaliert wie baseDamage</summary>
        public int splashDamage;
        public float splashRadius = 1f;
        public float cooldown = 0.5f;
        public float speed = 9f;
        public float range = 6f;
        /// <summary>Trefferradius (Projektil, Drachenkopf) bzw. Endradius (Ring)</summary>
        public float radius = 0.3f;
        public float knockback = 3f;
        public int xpPerHit = 1;

        /// <summary>Grosser Ausruf in der Bildmitte</summary>
        public bool bigChant;

        /// <summary>Freischaltung: Zauber mit dieser ID braucht mindestens unlockSpellLevel (leer = keine Bedingung)</summary>
        public string unlockSpellId;
        public int unlockSpellLevel;
        /// <summary>Freischaltung: Spieler-Level (1 = von Anfang an)</summary>
        public int unlockPlayerLevel = 1;

        /// <summary>Prefab mit einem SpellEffect (Projektil, Drache, Ring)</summary>
        public SpellEffect effectPrefab;

        public static string ElementPrefix(SpellElement element) =>
            element == SpellElement.Wasser ? "Wasser-Erschaffungsmagie" : "Feuer-Erschaffungsmagie";

        public static Color ElementColor(SpellElement element) =>
            element == SpellElement.Wasser ? new Color(0.35f, 0.75f, 1f) : new Color(1f, 0.55f, 0.15f);

        public string ElementLabel => element == SpellElement.Wasser ? "Wasser" : "Feuer";

        /// <summary>Was der Charakter ruft, z. B. "Wasser-Erschaffungsmagie: Wasserdrache!"</summary>
        public string Chant => $"{ElementPrefix(element)}: {spellName}!";
    }
}
