using Reborn.Guilds;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Gilden-Seite im Handelsfenster (Meilenstein 7c): Begruessung, Beitreten bzw. Wechseln, Rang und Bonus,
    /// aktuelle Quest mit Fortschritt und Belohnung, Knoepfe "Annehmen" und "Abgeben".
    /// </summary>
    public class GuildView : MonoBehaviour
    {
        public Text greetingText;
        public Text statusText;
        public Text questText;
        public Button joinButton;
        public Text joinLabel;
        public Button acceptButton;
        public Button turnInButton;

        TradeWindow _window;
        PlayerGuild _guild;
        GuildId _id;
        bool _setup;

        void Setup()
        {
            if (_setup) return;
            _setup = true;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _guild = player.GetComponent<PlayerGuild>();
            if (joinButton != null) joinButton.onClick.AddListener(() => Join());
            if (acceptButton != null) acceptButton.onClick.AddListener(() => Accept());
            if (turnInButton != null) turnInButton.onClick.AddListener(() => TurnIn());
        }

        public void Open(TradeWindow window, GuildId id, string greeting)
        {
            Setup();
            _window = window;
            _id = id;
            if (greetingText != null) greetingText.text = greeting;
            Refresh();
        }

        public void SelectFirst()
        {
            if (EventSystem.current == null) return;
            foreach (var b in new[] { joinButton, acceptButton, turnInButton })
                if (b != null && b.gameObject.activeInHierarchy && b.interactable) { EventSystem.current.SetSelectedGameObject(b.gameObject); return; }
        }

        public void Refresh()
        {
            if (_guild == null) return;
            var data = _guild.Find(_id);
            bool member = _guild.Guild == _id;
            if (statusText != null)
            {
                string attr = data != null ? Progression.PlayerStats.AttributeLabels[(int)data.attribute] : "";
                statusText.text = member
                    ? $"Rang: {GuildData.RankName(_guild.Rank)}   Bonus: {attr} +{2 * _guild.Rank}   Quests: {Mathf.Min(_guild.QuestIndex, PlayerGuild.QuestsPerRank)}/{PlayerGuild.QuestsPerRank}"
                    : _guild.Guild == GuildId.None
                        ? $"Tritt bei: {attr} +2 und die Gildenwaffe „{(data != null && data.weapon != null ? data.weapon.itemName : "-")}“."
                        : $"Du bist in der {_guild.Data.guildName}. Wechsel kostet {PlayerGuild.SwitchCost} Gold; Rang und Quests gehen verloren.";
            }
            if (joinButton != null) joinButton.gameObject.SetActive(!member);
            if (joinLabel != null) joinLabel.text = _guild.Guild == GuildId.None ? "Beitreten" : $"Wechseln ({PlayerGuild.SwitchCost})";
            var q = member ? _guild.CurrentQuest : null;
            if (questText != null)
            {
                if (!member) questText.text = "";
                else if (q == null) questText.text = "Alle Quests erledigt. Du bist Geselle!";
                else
                {
                    string state = _guild.QuestActive ? $"Fortschritt: {_guild.CurrentProgress}/{_guild.Required}" : "Noch nicht angenommen";
                    questText.text = $"Quest {_guild.QuestIndex + 1}/{PlayerGuild.QuestsPerRank}: {q.title}\n{state}\nBelohnung: {q.gold} Gold, {q.xp} EP";
                }
            }
            if (acceptButton != null) acceptButton.gameObject.SetActive(member && q != null && !_guild.QuestActive);
            if (turnInButton != null)
            {
                turnInButton.gameObject.SetActive(member && q != null && _guild.QuestActive);
                turnInButton.interactable = _guild.IsComplete;
            }
        }

        string Done(string msg, bool save)
        {
            if (_window != null)
            {
                _window.ShowMessage(msg);
                if (save) _window.SaveNow();
            }
            Refresh();
            SelectFirst();
            return msg;
        }

        public string Join()
        {
            Setup();
            var before = _guild.Guild;
            var msg = _guild.Join(_id);
            return Done(msg, _guild.Guild != before);
        }

        public string Accept()
        {
            Setup();
            bool before = _guild.QuestActive;
            var msg = _guild.Accept();
            return Done(msg, _guild.QuestActive != before);
        }

        public string TurnIn()
        {
            Setup();
            int before = _guild.QuestIndex;
            var msg = _guild.TurnIn();
            return Done(msg, _guild.QuestIndex != before);
        }
    }
}
