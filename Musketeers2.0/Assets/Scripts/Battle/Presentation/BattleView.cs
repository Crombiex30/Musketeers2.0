using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    public sealed class BattleView : MonoBehaviour
    {
        [SerializeField] private Transform partyRoot;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private BattleCommandMenu commandMenu;
        [SerializeField] private BattleTargetSelector targetSelector;
        [SerializeField] private BattleHudPresenter hudPresenter;
        [SerializeField] private TurnIconBar turnIconBar;
        [SerializeField] private BattleLogPresenter battleLog;

        private readonly Dictionary<BattleUnit, BattleUnitView> unitViews = new Dictionary<BattleUnit, BattleUnitView>();
        private BattleController controller;

        public void Bind(BattleController battleController, BattleEncounterDefinition encounter)
        {
            controller = battleController;
            ClearViews();
            SpawnUnits(controller.Party, encounter.party, partyRoot);
            SpawnUnits(controller.Enemies, encounter.enemies, enemyRoot);

            commandMenu?.Bind(controller);
            targetSelector?.Bind(controller);
            hudPresenter?.Bind(controller);
            turnIconBar?.Bind(controller, encounter != null ? encounter.uiTheme : null);
            battleLog?.Clear();

            controller.ActionResolved += RefreshAfterAction;
            controller.ActiveActorChanged += HighlightActiveActor;
            controller.BattleEventTriggered += OnBattleEventTriggered;
            controller.BattleEventExpired += OnBattleEventExpired;
        }

        /// <summary>Shows the event announcement banner / log entry when a new event fires.</summary>
        public void ShowEventAnnouncement(BattleEventDefinition eventDef, BattleEventContext ctx)
        {
            if (eventDef == null) return;
            battleLog?.Add($"[EVENT] {eventDef.displayName.ToUpper()}");
            battleLog?.Add(eventDef.description);
            if (ctx != null)
            {
                for (int i = 0; i < ctx.Messages.Count; i++)
                    battleLog?.Add(ctx.Messages[i]);
                ctx.Messages.Clear();
            }
        }

        private void OnBattleEventTriggered(BattleEventDefinition eventDef, BattleEventContext ctx)
        {
            ShowEventAnnouncement(eventDef, ctx);
        }

        private void OnBattleEventExpired(BattleEventDefinition eventDef, BattleEventContext ctx)
        {
            if (eventDef != null)
                battleLog?.Add($"[EVENT OVER] {eventDef.displayName} has ended.");
            if (ctx != null)
            {
                for (int i = 0; i < ctx.Messages.Count; i++)
                    battleLog?.Add(ctx.Messages[i]);
                ctx.Messages.Clear();
            }
        }

        public void ShowCommandOptions(BattleUnit actor)
        {
            commandMenu?.Show(actor);
            targetSelector?.Hide();
        }

        public void ShowTargetOptions(IReadOnlyList<BattleUnit> targets)
        {
            commandMenu?.Hide();
            targetSelector?.Show(targets);
        }

        public void ShowMessage(string message)
        {
            battleLog?.Add(message);
        }

        public void ShowResult(BattleResult result)
        {
            if (result == null)
            {
                return;
            }

            battleLog?.Add(result.Message);
            for (int i = 0; i < result.Targets.Count; i++)
            {
                BattleTargetResult target = result.Targets[i];
                if (target.Target == null)
                {
                    continue;
                }

                string verb = result.Skill != null && result.Skill.kind == BattleSkillKind.Heal ? "restored" : "took";
                if (target.Outcome == BattleActionOutcome.Null)
                {
                    battleLog?.Add($"{target.Target.DisplayName} nullified the attack.");
                }
                else if (target.Outcome == BattleActionOutcome.Drain)
                {
                    battleLog?.Add($"{target.Target.DisplayName} drained {target.Amount} HP.");
                }
                else if (target.Outcome == BattleActionOutcome.Repel)
                {
                    battleLog?.Add($"{target.Target.DisplayName} repelled the attack.");
                }
                else if (target.Outcome == BattleActionOutcome.Miss)
                {
                    battleLog?.Add($"{result.Actor.DisplayName}'s attack missed.");
                }
                else if (target.Amount > 0)
                {
                    battleLog?.Add($"{target.Target.DisplayName} {verb} {target.Amount} HP.");
                }

                if (target.Outcome == BattleActionOutcome.Weak)
                {
                    battleLog?.Add("Weakness hit! Turn icon preserved.");
                }
            }

            hudPresenter?.Refresh();
        }

        public IEnumerator PlayAction(BattleAction action, BattleResult result)
        {
            if (action == null || action.Skill == null)
            {
                yield break;
            }

            BattleUnitView actorView = GetView(action.Actor);
            if (actorView != null)
            {
                yield return actorView.PlayAct(action.Skill);
            }

            if (action.Skill.vfx != null)
            {
                SpawnVfx(action.Skill.vfx.castPrefab, actorView != null ? actorView.EffectOrigin : null, Vector3.zero);
                for (int i = 0; i < result.Targets.Count; i++)
                {
                    BattleUnitView targetView = GetView(result.Targets[i].Target);
                    SpawnVfx(action.Skill.vfx.impactPrefab, targetView != null ? targetView.EffectOrigin : null, action.Skill.vfx.impactOffset);
                }

                yield return new WaitForSeconds(action.Skill.vfx.actionDelay);
            }
        }

        private void RefreshAfterAction(BattleResult result)
        {
            hudPresenter?.Refresh();
            for (int i = 0; i < controller.Party.Count; i++)
            {
                GetView(controller.Party[i])?.Refresh();
            }

            for (int i = 0; i < controller.Enemies.Count; i++)
            {
                GetView(controller.Enemies[i])?.Refresh();
            }
        }

        private void HighlightActiveActor(BattleUnit actor)
        {
            foreach (KeyValuePair<BattleUnit, BattleUnitView> pair in unitViews)
            {
                pair.Value.SetHighlighted(pair.Key == actor);
            }
        }

        private void SpawnUnits(IReadOnlyList<BattleUnit> units, List<BattleFormationSlot> slots, Transform root)
        {
            int unitIndex = 0;
            for (int slotIndex = 0; slotIndex < slots.Count && unitIndex < units.Count; slotIndex++)
            {
                BattleFormationSlot slot = slots[slotIndex];
                if (slot == null || slot.unit == null)
                {
                    continue;
                }

                BattleUnit unit = units[unitIndex++];
                GameObject prefab = unit.Definition != null ? unit.Definition.battlePrefab : null;
                GameObject instance = prefab != null ? Instantiate(prefab, root) : new GameObject(unit.DisplayName);
                instance.transform.localPosition = slot.scenePosition;

                BattleUnitView view = instance.GetComponent<BattleUnitView>();
                if (view == null)
                {
                    view = instance.AddComponent<BattleUnitView>();
                }

                view.Bind(unit);
                unitViews[unit] = view;
            }
        }

        private BattleUnitView GetView(BattleUnit unit)
        {
            return unit != null && unitViews.TryGetValue(unit, out BattleUnitView view) ? view : null;
        }

        private void SpawnVfx(GameObject prefab, Transform origin, Vector3 offset)
        {
            if (prefab == null)
            {
                return;
            }

            Vector3 position = origin != null ? origin.position + offset : transform.position + offset;
            Instantiate(prefab, position, Quaternion.identity);
        }

        private void ClearViews()
        {
            unitViews.Clear();
            ClearRoot(partyRoot);
            ClearRoot(enemyRoot);
        }

        private static void ClearRoot(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}