using Reborn.Core;
using System;
using System.Text;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Rechte Seite im Inventar: Name, Kategorie, Beschreibung, Werte mit Vergleich, Verkaufswert, Aktion.</summary>
    public class ItemDetailView : MonoBehaviour
    {
        public GameObject content;
        public Text emptyText;
        public Image iconImage;
        public Text nameText;
        public Text categoryText;
        public Text descriptionText;
        public Text statsText;
        public Text sellText;
        public Button actionButton;
        public Text actionLabel;

        [Header("Meilenstein 10a: Wegwerfen")]
        public Button dropButton;
        public Button dropAllButton;

        Action _action;
        Action _drop;
        Action _dropAll;

        void Awake()
        {
            if (actionButton != null) actionButton.onClick.AddListener(() => _action?.Invoke());
            if (dropButton != null) dropButton.onClick.AddListener(() => _drop?.Invoke());
            if (dropAllButton != null) dropAllButton.onClick.AddListener(() => _dropAll?.Invoke());
        }

        /// <summary>Wegwerf-Knoepfe zeigen (null = Knopf aus). Show() blendet sie zuerst aus.</summary>
        public void ShowDrop(Action one, Action all)
        {
            _drop = one;
            _dropAll = all;
            if (dropButton != null) dropButton.gameObject.SetActive(one != null);
            if (dropAllButton != null) dropAllButton.gameObject.SetActive(all != null);
        }

        public void Clear()
        {
            ShowDrop(null, null);
            _action = null;
            if (content != null) content.SetActive(false);
            if (emptyText != null) emptyText.gameObject.SetActive(true);
        }

        /// <param name="compareTo">Angelegtes Teil, das ersetzt wuerde (null = kein Vergleich)</param>
        /// <param name="compare">true, wenn ein Vergleich angezeigt werden soll (auch gegen einen leeren Platz)</param>
        public void Show(ItemData item, int count, ItemData compareTo, bool compare, string actionText, Action action)
        {
            if (item == null)
            {
                Clear();
                return;
            }
            ShowDrop(null, null);
            if (content != null) content.SetActive(true);
            if (emptyText != null) emptyText.gameObject.SetActive(false);

            if (iconImage != null) iconImage.sprite = item.icon;
            if (nameText != null)
            {
                string name = DisplayName(item);
                nameText.text = count > 1 ? $"{name}  x{count}" : name;
                nameText.color = ItemData.RarityColor(item.rarity);
            }
            if (categoryText != null) categoryText.text = $"{item.CategoryLabel} · {ItemData.RarityLabel(item.rarity)}";
            if (descriptionText != null) descriptionText.text = Loc.T(item.description);
            if (statsText != null) statsText.text = BuildStats(item, compareTo, compare);
            if (sellText != null) sellText.text = Loc.F("Verkaufswert: {0} Gold", item.sellValue);

            _action = action;
            if (actionButton != null) actionButton.gameObject.SetActive(action != null);
            if (actionLabel != null) actionLabel.text = actionText ?? "";
        }

        static string Signed(int v) => v >= 0 ? "+" + v : v.ToString();

        static Economy.UpgradeBook _upgrades;

        /// <summary>Schmiede-Stufen des Spielers (Meilenstein 6), null wenn keine</summary>
        public static Economy.UpgradeBook Upgrades
        {
            get
            {
                if (_upgrades == null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null) _upgrades = player.GetComponent<Economy.UpgradeBook>();
                }
                return _upgrades;
            }
        }

        /// <summary>Name mit Schmiede-Stufe, z. B. "Holzschwert +2"</summary>
        public static string DisplayName(ItemData item)
        {
            int level = Upgrades != null ? Upgrades.Level(item) : 0;
            return level > 0 ? $"{Loc.T(item.itemName)} +{level}" : Loc.T(item.itemName);
        }

        /// <summary>Werte mit Schmiede-Stufe</summary>
        public static StatBlock StatsOf(ItemData item) =>
            item == null ? new StatBlock() : Upgrades != null ? item.stats + Upgrades.Bonus(item) : item.stats;

        public static string BuildStats(ItemData item, ItemData compareTo, bool compare)
        {
            var sb = new StringBuilder();
            if (item.healAmount > 0) sb.AppendLine(Loc.F("Heilt {0} Leben", item.healAmount));
            if (item.manaAmount > 0) sb.AppendLine(Loc.F("Füllt {0} Mana auf", item.manaAmount));
            var a = StatsOf(item).ToArray();
            var b = compareTo != null ? StatsOf(compareTo).ToArray() : new int[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0 && b[i] == 0) continue;
                sb.Append($"{StatBlock.Labels[i]} {Signed(a[i])}");
                int diff = a[i] - b[i];
                if (compare && diff != 0)
                {
                    string color = diff > 0 ? "#66E266" : "#FF6666";
                    sb.Append($"   <color={color}>({Signed(diff)})</color>");
                }
                sb.AppendLine();
            }
            if (compare && item.IsEquipment)
                sb.AppendLine(compareTo != null ? $"<color=#BBBBBB>{Loc.F("Vergleich mit: {0}", Loc.T(compareTo.itemName))}</color>" : $"<color=#BBBBBB>{Loc.T("Platz ist frei")}</color>");
            return sb.ToString().TrimEnd();
        }
    }
}
