using UnityEngine;

namespace Musketeers.Battle
{
    public sealed class LegacyBattleLauncher : MonoBehaviour
    {
        [SerializeField] private BattleEncounterDefinition encounter;
        [SerializeField] private BattleController controller;
        [SerializeField] private BattleSystem legacyBattleSystem;
        [SerializeField] private bool disableLegacyBattleSystem = true;
        [SerializeField] private bool launchOnStart = true;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<BattleController>();
            }

            if (legacyBattleSystem == null)
            {
                legacyBattleSystem = GetComponent<BattleSystem>();
            }
        }

        private void Start()
        {
            if (disableLegacyBattleSystem && legacyBattleSystem != null)
            {
                legacyBattleSystem.enabled = false;
            }

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
