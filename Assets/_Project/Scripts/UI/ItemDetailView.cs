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

        Action _action;

        void Awake()
        {
            if (actionButton != null) actionButton.onClick.AddListener(() => _action?.Invoke());
        }

        public void Clear()
        {
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
            if (content != null) content.SetActive(true);
            if (emptyText != null) emptyText.gameObject.SetActive(false);

            if (iconImage != null) iconImage.sprite = item.icon;
            if (nameText != null)
            {
                nameText.text = count > 1 ? $"{item.itemName}  x{count}" : item.itemName;
                nameText.color = ItemData.RarityColor(item.rarity);
            }
            if (categoryText != null) categoryText.text = $"{item.CategoryLabel} · {ItemData.RarityLabel(item.rarity)}";
            if (descriptionText != null) descriptionText.text = item.description;
            if (statsText != null) statsText.text = BuildStats(item, compareTo, compare);
            if (sellText != null) sellText.text = $"Verkaufswert: {item.sellValue} Gold";

            _action = action;
            if (actionButton != null) actionButton.gameObject.SetActive(action != null);
            if (actionLabel != null) actionLabel.text = actionText ?? "";
        }

        static string Signed(int v) => v >= 0 ? "+" + v : v.ToString();

        public static string BuildStats(ItemData item, ItemData compareTo, bool compare)
        {
            var sb = new StringBuilder();
            if (item.healAmount > 0) sb.AppendLine($"Heilt {item.healAmount} Leben");
            if (item.manaAmount > 0) sb.AppendLine($"Füllt {item.manaAmount} Mana auf");
            var a = item.stats.ToArray();
            var b = compareTo != null ? compareTo.stats.ToArray() : new int[a.Length];
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
                sb.AppendLine(compareTo != null ? $"<color=#BBBBBB>Vergleich mit: {compareTo.itemName}</color>" : "<color=#BBBBBB>Platz ist frei</color>");
            return sb.ToString().TrimEnd();
        }
    }
}
