using System.Collections.Generic;
using Reborn.Loot;
using UnityEngine;

namespace Reborn.Economy
{
    /// <summary>Waren eines Haendlers (Meilenstein 6). Preis = ItemData.buyValue, unbegrenzte Menge.</summary>
    [CreateAssetMenu(menuName = "Reborn/Shop", fileName = "NewShop")]
    public class ShopData : ScriptableObject
    {
        public List<ItemData> items = new List<ItemData>();
    }
}
