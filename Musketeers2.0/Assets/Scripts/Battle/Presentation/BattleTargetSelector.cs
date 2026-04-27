using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Musketeers.Battle
{
    public sealed class BattleTargetSelector : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Transform buttonRoot;
        [SerializeField] private Button buttonPrefab;

        private readonly List<Button> spawnedButtons = new List<Button>();
        private BattleController controller;

        public void Bind(BattleController battleController)
        {
            controller = battleController;
            Hide();
        }

        public void Show(IReadOnlyList<BattleUnit> targets)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            ClearButtons();
            if (buttonPrefab == null || buttonRoot == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                BattleUnit target = targets[i];
                Button button = Instantiate(buttonPrefab, buttonRoot);
                TMP_Text text = button.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = target.DisplayName;
                }

                button.onClick.AddListener(() => controller.ConfirmTarget(target));
                spawnedButtons.Add(button);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void ClearButtons()
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                if (spawnedButtons[i] != null)
                {
                    Destroy(spawnedButtons[i].gameObject);
                }
            }

            spawnedButtons.Clear();
        }
    }
}
