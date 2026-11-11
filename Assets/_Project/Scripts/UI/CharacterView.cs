using Reborn.Combat;
using Reborn.Progression;
using Reborn.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Reiter "Charakter": Werte, Attribute mit "+", Spielstand loeschen.</summary>
    public class CharacterView : MonoBehaviour
    {
        public Text statsText;
        public Text pointsText;
        public Text[] attributeTexts = new Text[PlayerStats.AttributeCount];
        public Text[] effectTexts = new Text[PlayerStats.AttributeCount];
        public Button[] plusButtons = new Button[PlayerStats.AttributeCount];
        public Button deleteButton;
        public GameObject confirmPanel;
        public Button confirmYes;
        public Button confirmNo;
        public Selectable fallbackSelection;

        static readonly string[] Effects =
        {
            "+1 Angriff pro Punkt",
            "Schneller angreifen und ausweichen",
            "+5 Leben pro Punkt",
            "+5 Mana, +1 Magieschaden pro Punkt",
            "+3 % Beute-Chance pro Punkt"
        };

        PlayerStats _stats;
        PlayerProgression _progression;
        Health _health;
        SaveSystem _save;
        bool _setup;

        void Setup()
        {
            if (_setup) return;
            _setup = true;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _stats = player.GetComponent<PlayerStats>();
                _progression = player.GetComponent<PlayerProgression>();
                _health = player.GetComponent<Health>();
                _save = player.GetComponent<SaveSystem>();
            }
            for (int i = 0; i < plusButtons.Length; i++)
            {
                int a = i;
                if (plusButtons[i] != null) plusButtons[i].onClick.AddListener(() => AddPoint(a));
            }
            if (deleteButton != null) deleteButton.onClick.AddListener(AskDelete);
            if (confirmYes != null) confirmYes.onClick.AddListener(() => { if (_save != null) _save.DeleteAndRestart(); });
            if (confirmNo != null) confirmNo.onClick.AddListener(CancelDelete);
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

        void OnEnable()
        {
            Setup();
            if (_stats != null) _stats.OnChanged += Refresh;
            if (_progression != null) _progression.OnChanged += Refresh;
            if (_health != null) _health.OnChanged += Refresh;
            if (confirmPanel != null) confirmPanel.SetActive(false);
            Refresh();
        }

        void OnDisable()
        {
            if (_stats != null) _stats.OnChanged -= Refresh;
            if (_progression != null) _progression.OnChanged -= Refresh;
            if (_health != null) _health.OnChanged -= Refresh;
        }

        public void Refresh()
        {
            if (_stats == null) return;
            if (statsText != null)
            {
                statsText.text =
                    $"Level {_progression.Level}\n" +
                    $"Erfahrung {_progression.XP} / {_progression.XPToNextLevel}\n\n" +
                    $"Leben {_health.Current} / {_health.maxHealth}\n" +
                    $"Angriff {_stats.Attack}\n" +
                    $"Verteidigung {_stats.Defense}\n" +
                    $"Angriff alle {_stats.AttackCooldown:0.00} s\n" +
                    $"Ausweichen alle {_stats.DodgeCooldown:0.00} s";
            }
            int points = _stats.UnspentPoints;
            if (pointsText != null) pointsText.text = points > 0 ? $"Freie Punkte: {points}" : "Keine freien Punkte";
            for (int i = 0; i < PlayerStats.AttributeCount; i++)
            {
                var attribute = (PlayerAttribute)i;
                int total = _stats.GetAttribute(attribute);
                int bonus = total - _stats.BaseAttribute(attribute);
                if (attributeTexts[i] != null)
                    attributeTexts[i].text = bonus != 0 ? $"{PlayerStats.AttributeLabels[i]}  {total}  <color=#66E266>(+{bonus})</color>" : $"{PlayerStats.AttributeLabels[i]}  {total}";
                if (effectTexts[i] != null) effectTexts[i].text = Effects[i];
                if (plusButtons[i] != null) plusButtons[i].gameObject.SetActive(points > 0);
            }
        }

        void AddPoint(int attribute)
        {
            if (_stats == null) return;
            _stats.AddPoint((PlayerAttribute)attribute);
            if (_stats.UnspentPoints <= 0) SelectFirst();
        }

        public void SelectFirst()
        {
            if (EventSystem.current == null) return;
            GameObject target = null;
            foreach (var b in plusButtons)
                if (b != null && b.gameObject.activeInHierarchy) { target = b.gameObject; break; }
            if (target == null && deleteButton != null) target = deleteButton.gameObject;
            EventSystem.current.SetSelectedGameObject(target);
        }

        void AskDelete()
        {
            if (confirmPanel == null) return;
            confirmPanel.SetActive(true);
            if (EventSystem.current != null && confirmNo != null) EventSystem.current.SetSelectedGameObject(confirmNo.gameObject);
        }

        void CancelDelete()
        {
            if (confirmPanel != null) confirmPanel.SetActive(false);
            SelectFirst();
        }
    }
}
