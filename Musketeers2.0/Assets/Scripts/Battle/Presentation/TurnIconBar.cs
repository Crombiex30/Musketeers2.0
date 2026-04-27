using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Musketeers.Battle
{
    public sealed class TurnIconBar : MonoBehaviour
    {
        [SerializeField] private Transform iconRoot;
        [SerializeField] private Image iconPrefab;

        private readonly List<Image> icons = new List<Image>();
        private BattleUiTheme theme;

        public void Bind(BattleController controller, BattleUiTheme uiTheme)
        {
            theme = uiTheme;
            controller.TurnIconsChanged += Refresh;
            Refresh(controller.TurnIcons);
        }

        public void Refresh(BattleTurnIconPool pool)
        {
            if (pool == null || iconRoot == null || iconPrefab == null)
            {
                return;
            }

            EnsureIconCount(pool.TotalIcons);

            int index = 0;
            for (int i = 0; i < pool.FullIcons; i++)
            {
                SetIcon(icons[index++], theme != null ? theme.fullTurnIcon : null, true);
            }

            for (int i = 0; i < pool.HalfIcons; i++)
            {
                SetIcon(icons[index++], theme != null ? theme.halfTurnIcon : null, true);
            }

            for (; index < icons.Count; index++)
            {
                SetIcon(icons[index], theme != null ? theme.emptyTurnIcon : null, false);
            }
        }

        private void EnsureIconCount(int count)
        {
            while (icons.Count < count)
            {
                icons.Add(Instantiate(iconPrefab, iconRoot));
            }
        }

        private static void SetIcon(Image image, Sprite sprite, bool active)
        {
            image.gameObject.SetActive(active);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }
    }
}
