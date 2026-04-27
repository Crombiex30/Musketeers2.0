using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Musketeers.Battle
{
    public sealed class BattleLogPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;
        [SerializeField] private int maxLines = 5;

        private readonly Queue<string> lines = new Queue<string>();

        public void Add(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lines.Enqueue(message);
            while (lines.Count > maxLines)
            {
                lines.Dequeue();
            }

            Refresh();
        }

        public void Clear()
        {
            lines.Clear();
            Refresh();
        }

        private void Refresh()
        {
            if (logText == null)
            {
                return;
            }

            logText.text = string.Join("\n", lines);
        }
    }
}
