using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Musketeers.Battle
{
    public sealed class BattleCommandMenu : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Transform buttonRoot;
        [SerializeField] private Button buttonPrefab;

        private BattleController controller;
        private readonly List<Button> spawnedButtons = new List<Button>();

        public void Bind(BattleController battleController)
        {
            controller = battleController;
            Hide();
        }

        public void Show(BattleUnit actor)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            ClearButtons();
            if (actor == null || buttonPrefab == null || buttonRoot == null)
            {
                return;
            }

            AddButton("Attack", () => controller.SelectSkill(actor.Definition.basicAttack));
            for (int i = 0; i < actor.Definition.skills.Count; i++)
            {
                BattleSkillDefinition skill = actor.Definition.skills[i];
                if (skill != null)
                {
                    AddButton(skill.displayName, () => controller.SelectSkill(skill), actor.CanPay(skill));
                }
            }

            AddButton("Guard", controller.Guard);
            AddButton("Analyse", () => controller.Analyse(null));
            AddButton("Pass", controller.PassTurn);
            AddButton("Escape", controller.Escape);
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void AddButton(string label, UnityEngine.Events.UnityAction onClick, bool interactable = true)
        {
            Button button = Instantiate(buttonPrefab, buttonRoot);
            button.interactable = interactable;
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = label;
            }

            button.onClick.AddListener(onClick);
            spawnedButtons.Add(button);
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