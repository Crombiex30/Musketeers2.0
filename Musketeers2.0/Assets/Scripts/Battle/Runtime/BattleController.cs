using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] private BattleEncounterDefinition encounter;
        [SerializeField] private BattleView battleView;
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private float actionPause = 0.35f;

        private readonly List<BattleUnit> party = new List<BattleUnit>();
        private readonly List<BattleUnit> enemies = new List<BattleUnit>();
        private readonly BattleTurnIconPool turnIcons = new BattleTurnIconPool();
        private readonly BattleEnemyAi enemyAi = new BattleEnemyAi();

        private BattlePhase phase = BattlePhase.Idle;
        private BattleFaction activeFaction = BattleFaction.Party;
        private int activeActorIndex;
        private BattleSkillDefinition pendingSkill;
        private bool resolving;

        public event Action<BattlePhase> PhaseChanged;
        public event Action<BattleResult> ActionResolved;
        public event Action<BattleUnit> ActiveActorChanged;
        public event Action<BattleTurnIconPool> TurnIconsChanged;

        public BattlePhase Phase => phase;
        public BattleUnit ActiveActor { get; private set; }
        public IReadOnlyList<BattleUnit> Party => party;
        public IReadOnlyList<BattleUnit> Enemies => enemies;
        public BattleTurnIconPool TurnIcons => turnIcons;

        private void Awake()
        {
            if (battleView == null)
            {
                battleView = GetComponentInChildren<BattleView>();
            }
        }

        private void Start()
        {
            if (startOnAwake && encounter != null)
            {
                StartBattle(encounter);
            }
        }

        public void StartBattle(BattleEncounterDefinition encounterDefinition)
        {
            StopAllCoroutines();
            encounter = encounterDefinition;
            party.Clear();
            enemies.Clear();
            activeActorIndex = 0;
            pendingSkill = null;
            resolving = false;
            SetPhase(BattlePhase.Setup);

            BuildUnits(encounter.party, BattleFaction.Party, party);
            BuildUnits(encounter.enemies, BattleFaction.Enemy, enemies);

            if (battleView != null)
            {
                battleView.Bind(this, encounter);
            }

            StartFactionTurn(BattleFaction.Party);
        }

        public void SelectSkill(BattleSkillDefinition skill)
        {
            if (phase != BattlePhase.PlayerCommand || ActiveActor == null || resolving)
            {
                return;
            }

            if (skill == null)
            {
                skill = ActiveActor.Definition != null ? ActiveActor.Definition.basicAttack : null;
            }

            if (skill == null || !ActiveActor.CanPay(skill))
            {
                battleView?.ShowMessage("That action cannot be used.");
                return;
            }

            pendingSkill = skill;
            if (RequiresTargetSelection(skill))
            {
                SetPhase(BattlePhase.PlayerTarget);
                battleView?.ShowTargetOptions(GetSelectableTargets(skill, ActiveActor));
                return;
            }

            BattleUnit target = GetDefaultTarget(skill, ActiveActor);
            ConfirmTarget(target);
        }

        public void ConfirmTarget(BattleUnit target)
        {
            if ((phase != BattlePhase.PlayerTarget && phase != BattlePhase.PlayerCommand) || ActiveActor == null || pendingSkill == null || resolving)
            {
                return;
            }

            List<BattleUnit> targets = BattleActionResolver.ResolveTargets(pendingSkill, ActiveActor, target, party, enemies);
            if (targets.Count == 0)
            {
                battleView?.ShowMessage("No valid targets.");
                return;
            }

            StartCoroutine(ResolvePlayerAction(new BattleAction(ActiveActor, pendingSkill, targets)));
        }

        public void PassTurn()
        {
            if (phase != BattlePhase.PlayerCommand || ActiveActor == null || resolving)
            {
                return;
            }

            BattleResult result = new BattleResult
            {
                Actor = ActiveActor,
                TurnOutcome = BattleActionOutcome.Pass,
                Message = $"{ActiveActor.DisplayName} passes."
            };
            CompleteAction(result);
        }

        public void Guard()
        {
            if (ActiveActor == null)
            {
                return;
            }

            BattleSkillDefinition guardSkill = ScriptableObject.CreateInstance<BattleSkillDefinition>();
            guardSkill.displayName = "Guard";
            guardSkill.kind = BattleSkillKind.Guard;
            guardSkill.targeting = BattleTargeting.Self;
            pendingSkill = guardSkill;
            ConfirmTarget(ActiveActor);
        }

        public void Analyse(BattleUnit target)
        {
            if (ActiveActor == null)
            {
                return;
            }

            BattleSkillDefinition analyseSkill = ScriptableObject.CreateInstance<BattleSkillDefinition>();
            analyseSkill.displayName = "Analyse";
            analyseSkill.kind = BattleSkillKind.Analyse;
            analyseSkill.targeting = BattleTargeting.SingleEnemy;
            pendingSkill = analyseSkill;
            ConfirmTarget(target != null ? target : GetDefaultTarget(analyseSkill, ActiveActor));
        }

        public void Escape()
        {
            if (phase != BattlePhase.PlayerCommand)
            {
                return;
            }

            SetPhase(BattlePhase.Escaped);
            battleView?.ShowMessage("The party escaped.");
        }

        private IEnumerator ResolvePlayerAction(BattleAction action)
        {
            resolving = true;
            SetPhase(BattlePhase.ResolvingAction);
            yield return ResolveActionRoutine(action);
            resolving = false;
            ContinueAfterAction();
        }

        private IEnumerator RunEnemyTurn()
        {
            SetPhase(BattlePhase.EnemyTurn);

            while (turnIcons.HasActions && HasLiving(enemies) && HasLiving(party))
            {
                BattleUnit actor = GetNextLivingActor(enemies, ref activeActorIndex);
                if (actor == null)
                {
                    break;
                }

                ActiveActor = actor;
                ActiveActorChanged?.Invoke(ActiveActor);
                BattleAction action = enemyAi.ChooseAction(actor, party, enemies);
                if (action == null || action.Targets.Count == 0)
                {
                    turnIcons.Spend(BattleActionOutcome.Pass);
                    TurnIconsChanged?.Invoke(turnIcons);
                    continue;
                }

                yield return ResolveActionRoutine(action);
                if (CheckBattleEnd())
                {
                    yield break;
                }
            }

            StartFactionTurn(BattleFaction.Party);
        }

        private IEnumerator ResolveActionRoutine(BattleAction action)
        {
            pendingSkill = null;
            BattleResult result = BattleActionResolver.Resolve(action);

            if (battleView != null)
            {
                yield return battleView.PlayAction(action, result);
            }

            yield return new WaitForSeconds(actionPause);
            CompleteAction(result);
        }

        private void CompleteAction(BattleResult result)
        {
            turnIcons.Spend(result.TurnOutcome);
            TurnIconsChanged?.Invoke(turnIcons);
            ActionResolved?.Invoke(result);
            battleView?.ShowResult(result);
            CheckBattleEnd();
        }

        private void ContinueAfterAction()
        {
            if (CheckBattleEnd())
            {
                return;
            }

            if (!turnIcons.HasActions)
            {
                StartFactionTurn(BattleFaction.Enemy);
                return;
            }

            SelectNextPlayerActor();
        }

        private void StartFactionTurn(BattleFaction faction)
        {
            if (CheckBattleEnd())
            {
                return;
            }

            activeFaction = faction;
            activeActorIndex = 0;
            IReadOnlyList<BattleUnit> units = faction == BattleFaction.Party ? party : enemies;
            turnIcons.Reset(CountLiving(units));
            TurnIconsChanged?.Invoke(turnIcons);

            for (int i = 0; i < units.Count; i++)
            {
                units[i]?.TickStatuses();
                units[i]?.SetGuarding(false);
            }

            if (faction == BattleFaction.Party)
            {
                SelectNextPlayerActor();
            }
            else
            {
                StartCoroutine(RunEnemyTurn());
            }
        }

        private void SelectNextPlayerActor()
        {
            ActiveActor = GetNextLivingActor(party, ref activeActorIndex);
            ActiveActorChanged?.Invoke(ActiveActor);
            SetPhase(BattlePhase.PlayerCommand);
            battleView?.ShowCommandOptions(ActiveActor);
        }

        private BattleUnit GetNextLivingActor(IReadOnlyList<BattleUnit> units, ref int index)
        {
            if (units.Count == 0)
            {
                return null;
            }

            for (int attempts = 0; attempts < units.Count; attempts++)
            {
                BattleUnit unit = units[index % units.Count];
                index = (index + 1) % units.Count;
                if (unit != null && unit.IsAlive)
                {
                    return unit;
                }
            }

            return null;
        }

        private bool CheckBattleEnd()
        {
            if (!HasLiving(enemies))
            {
                SetPhase(BattlePhase.Won);
                battleView?.ShowMessage("Victory!");
                return true;
            }

            if (!HasLiving(party))
            {
                SetPhase(BattlePhase.Lost);
                battleView?.ShowMessage("Defeat...");
                return true;
            }

            return false;
        }

        private void SetPhase(BattlePhase nextPhase)
        {
            if (phase == nextPhase)
            {
                return;
            }

            phase = nextPhase;
            PhaseChanged?.Invoke(phase);
        }

        private void BuildUnits(List<BattleFormationSlot> slots, BattleFaction faction, List<BattleUnit> destination)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                BattleFormationSlot slot = slots[i];
                if (slot == null || slot.unit == null)
                {
                    continue;
                }

                destination.Add(new BattleUnit(slot.unit, faction, slot.frontRow));
            }
        }

        private bool RequiresTargetSelection(BattleSkillDefinition skill)
        {
            return skill.targeting == BattleTargeting.SingleAlly || skill.targeting == BattleTargeting.SingleEnemy;
        }

        private List<BattleUnit> GetSelectableTargets(BattleSkillDefinition skill, BattleUnit actor)
        {
            List<BattleUnit> targets = new List<BattleUnit>();
            IReadOnlyList<BattleUnit> source = skill.targeting == BattleTargeting.SingleAlly ? party : enemies;
            if (actor.Faction == BattleFaction.Enemy)
            {
                source = skill.targeting == BattleTargeting.SingleAlly ? enemies : party;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null && source[i].IsAlive)
                {
                    targets.Add(source[i]);
                }
            }

            return targets;
        }

        private BattleUnit GetDefaultTarget(BattleSkillDefinition skill, BattleUnit actor)
        {
            List<BattleUnit> targets = GetSelectableTargets(skill, actor);
            return targets.Count > 0 ? targets[0] : actor;
        }

        private static bool HasLiving(IReadOnlyList<BattleUnit> units)
        {
            return CountLiving(units) > 0;
        }

        private static int CountLiving(IReadOnlyList<BattleUnit> units)
        {
            int count = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null && units[i].IsAlive)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
