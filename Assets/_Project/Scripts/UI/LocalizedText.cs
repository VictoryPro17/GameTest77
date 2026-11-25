using Reborn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Feste Beschriftung in der gewaehlten Sprache (Meilenstein 10a). key = deutscher Text
    /// (leer: der Text beim Start). Schreibt ein Skript spaeter selbst in das Textfeld, gewinnt dessen Text
    /// bis zum naechsten Sprachwechsel - solche Texte uebersetzt das Skript mit Loc.T.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [TextArea] public string key = "";

        Text _text;

        void Awake()
        {
            _text = GetComponent<Text>();
            if (string.IsNullOrEmpty(key)) key = _text.text;
        }

        void OnEnable()
        {
            GameSettings.OnChanged += Refresh;
            Refresh();
        }

        void OnDisable() => GameSettings.OnChanged -= Refresh;

        public void Refresh()
        {
            if (_text != null && !string.IsNullOrEmpty(key)) _text.text = Loc.T(key);
        }
    }
}
