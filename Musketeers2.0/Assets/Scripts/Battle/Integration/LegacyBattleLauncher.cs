using UnityEngine;

namespace Musketeers.Battle
{
    public sealed class LegacyBattleLauncher : MonoBehaviour
    {
        [SerializeField] private BattleEncounterDefinition encounter;
        [SerializeField] private BattleController controller;
        [SerializeField] private bool launchOnStart = true;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<BattleController>();
            }
        }

        private void Start()
        {
            if (launchOnStart)
            {
                Launch();
            }
        }

        public void Launch()
        {
            if (controller == null || encounter == null)
            {
                Debug.LogWarning("LegacyBattleLauncher needs a BattleController and BattleEncounterDefinition.");
                return;
            }

            controller.StartBattle(encounter);
        }
    }
}
