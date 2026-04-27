using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    public sealed class BattleHudPresenter : MonoBehaviour
    {
        [SerializeField] private Transform partyRoot;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private BattleHudRow rowPrefab;

        private readonly List<BattleHudRow> rows = new List<BattleHudRow>();
        private BattleController controller;

        public void Bind(BattleController battleController)
        {
            controller = battleController;
            BuildRows();
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].Refresh();
            }
        }

        private void BuildRows()
        {
            ClearRows();
            if (controller == null || rowPrefab == null)
            {
                return;
            }

            AddRows(controller.Party, partyRoot);
            AddRows(controller.Enemies, enemyRoot);
        }

        private void AddRows(IReadOnlyList<BattleUnit> units, Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = 0; i < units.Count; i++)
            {
                BattleHudRow row = Instantiate(rowPrefab, root);
                row.Bind(units[i]);
                rows.Add(row);
            }
        }

        private void ClearRows()
        {
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                if (rows[i] != null)
                {
                    Destroy(rows[i].gameObject);
                }
            }

            rows.Clear();
        }
    }
}

