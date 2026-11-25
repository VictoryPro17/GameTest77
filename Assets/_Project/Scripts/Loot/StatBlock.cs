using Reborn.Core;
using System;
using System.Collections.Generic;

namespace Reborn.Loot
{
    /// <summary>Werte, die ein Gegenstand (oder die Summe der Ausruestung) gibt.</summary>
    [Serializable]
    public struct StatBlock
    {
        public int attack;
        public int defense;
        public int maxHealth;
        public int strength;
        public int dexterity;
        public int vitality;
        public int intelligence;
        public int luck;

        public static StatBlock operator +(StatBlock a, StatBlock b)
        {
            return new StatBlock
            {
                attack = a.attack + b.attack,
                defense = a.defense + b.defense,
                maxHealth = a.maxHealth + b.maxHealth,
                strength = a.strength + b.strength,
                dexterity = a.dexterity + b.dexterity,
                vitality = a.vitality + b.vitality,
                intelligence = a.intelligence + b.intelligence,
                luck = a.luck + b.luck
            };
        }

        public static string[] Labels => new[]
            { Loc.T("Angriff"), Loc.T("Verteidigung"), Loc.T("Leben"), Loc.T("Stärke"), Loc.T("Geschick"), Loc.T("Vitalität"), Loc.T("Intelligenz"), Loc.T("Glück") };

        /// <summary>Werte in der Reihenfolge von Labels.</summary>
        public int[] ToArray()
        {
            return new[] { attack, defense, maxHealth, strength, dexterity, vitality, intelligence, luck };
        }

        /// <summary>Nur Werte ungleich 0, als (Name, Wert).</summary>
        public IEnumerable<(string label, int value)> NonZero()
        {
            var values = ToArray();
            for (int i = 0; i < values.Length; i++)
                if (values[i] != 0) yield return (Labels[i], values[i]);
        }
    }
}
