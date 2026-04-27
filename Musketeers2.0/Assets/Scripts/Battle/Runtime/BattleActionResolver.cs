using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    public static class BattleActionResolver
    {
        public static BattleResult Resolve(BattleAction action)
        {
            BattleResult result = new BattleResult
            {
                Actor = action.Actor,
                Skill = action.Skill,
                Message = BuildStartMessage(action)
            };

            if (action.Actor == null || action.Skill == null)
            {
                result.TurnOutcome = BattleActionOutcome.Miss;
                result.Message = "The action failed.";
                return result;
            }

            if (!action.Actor.CanPay(action.Skill))
            {
                result.TurnOutcome = BattleActionOutcome.Miss;
                result.Message = $"{action.Actor.DisplayName} does not have enough MP.";
                return result;
            }

            action.Actor.SpendCost(action.Skill);
            action.Actor.SetGuarding(false);

            switch (action.Skill.kind)
            {
                case BattleSkillKind.Heal:
                    ResolveHeal(action, result);
                    break;
                case BattleSkillKind.Buff:
                case BattleSkillKind.Debuff:
                    ResolveStatus(action, result);
                    break;
                case BattleSkillKind.Guard:
                    action.Actor.SetGuarding(true);
                    result.TurnOutcome = BattleActionOutcome.Normal;
                    result.Message = $"{action.Actor.DisplayName} guards.";
                    break;
                case BattleSkillKind.Analyse:
                    ResolveAnalyse(action, result);
                    break;
                case BattleSkillKind.Pass:
                    result.TurnOutcome = BattleActionOutcome.Pass;
                    result.Message = $"{action.Actor.DisplayName} passes.";
                    break;
                default:
                    ResolveDamage(action, result);
                    break;
            }

            return result;
        }

        private static void ResolveDamage(BattleAction action, BattleResult result)
        {
            BattleActionOutcome strongestOutcome = BattleActionOutcome.Normal;

            for (int i = 0; i < action.Targets.Count; i++)
            {
                BattleUnit target = action.Targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                if (!BattleMath.RollAccuracy(action.Actor, target, action.Skill))
                {
                    result.AddTarget(new BattleTargetResult
                    {
                        Target = target,
                        Affinity = BattleAffinityResponse.Normal,
                        Outcome = BattleActionOutcome.Miss,
                        Amount = 0,
                        Defeated = false
                    });
                    strongestOutcome = PickTurnOutcome(strongestOutcome, BattleActionOutcome.Miss);
                    continue;
                }

                BattleAffinityResponse affinity = target.GetAffinity(action.Skill.element);
                BattleActionOutcome outcome = action.Skill.canTriggerWeakness
                    ? BattleAffinityResolver.ToOutcome(affinity)
                    : BattleActionOutcome.Normal;

                int amount = 0;
                if (outcome == BattleActionOutcome.Drain)
                {
                    amount = BattleMath.CalculateDamage(action.Actor, target, action.Skill, BattleAffinityResponse.Normal);
                    target.Heal(amount);
                }
                else if (outcome == BattleActionOutcome.Repel)
                {
                    amount = BattleMath.CalculateDamage(action.Actor, action.Actor, action.Skill, BattleAffinityResponse.Normal);
                    action.Actor.TakeDamage(amount);
                }
                else if (outcome != BattleActionOutcome.Null)
                {
                    amount = BattleMath.CalculateDamage(action.Actor, target, action.Skill, affinity);
                    target.TakeDamage(amount);
                }

                result.AddTarget(new BattleTargetResult
                {
                    Target = target,
                    Affinity = affinity,
                    Outcome = outcome,
                    Amount = amount,
                    Defeated = !target.IsAlive
                });

                strongestOutcome = PickTurnOutcome(strongestOutcome, outcome);
            }

            result.TurnOutcome = strongestOutcome;
        }

        private static void ResolveHeal(BattleAction action, BattleResult result)
        {
            int amount = BattleMath.CalculateHeal(action.Actor, action.Skill);
            for (int i = 0; i < action.Targets.Count; i++)
            {
                BattleUnit target = action.Targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                target.Heal(amount);
                result.AddTarget(new BattleTargetResult
                {
                    Target = target,
                    Affinity = BattleAffinityResponse.Normal,
                    Outcome = BattleActionOutcome.Normal,
                    Amount = amount
                });
            }

            result.TurnOutcome = BattleActionOutcome.Normal;
        }

        private static void ResolveStatus(BattleAction action, BattleResult result)
        {
            for (int i = 0; i < action.Targets.Count; i++)
            {
                BattleUnit target = action.Targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                for (int modifierIndex = 0; modifierIndex < action.Skill.statModifiers.Count; modifierIndex++)
                {
                    BattleStatModifier modifier = action.Skill.statModifiers[modifierIndex];
                    target.AddStatus(new BattleStatusEffect
                    {
                        id = $"{action.Skill.name}:{modifier.stat}",
                        displayName = action.Skill.displayName,
                        stat = modifier.stat,
                        amount = modifier.amount,
                        turnsRemaining = modifier.turns
                    });
                }

                result.AddTarget(new BattleTargetResult
                {
                    Target = target,
                    Affinity = BattleAffinityResponse.Normal,
                    Outcome = BattleActionOutcome.Normal
                });
            }

            result.TurnOutcome = BattleActionOutcome.Normal;
        }

        private static void ResolveAnalyse(BattleAction action, BattleResult result)
        {
            for (int i = 0; i < action.Targets.Count; i++)
            {
                BattleUnit target = action.Targets[i];
                if (target == null)
                {
                    continue;
                }

                target.Analysed = true;
                result.AddTarget(new BattleTargetResult
                {
                    Target = target,
                    Affinity = BattleAffinityResponse.Normal,
                    Outcome = BattleActionOutcome.Normal
                });
            }

            result.TurnOutcome = BattleActionOutcome.Normal;
        }

        private static BattleActionOutcome PickTurnOutcome(BattleActionOutcome current, BattleActionOutcome next)
        {
            return OutcomePriority(next) > OutcomePriority(current) ? next : current;
        }

        private static int OutcomePriority(BattleActionOutcome outcome)
        {
            switch (outcome)
            {
                case BattleActionOutcome.Null:
                case BattleActionOutcome.Drain:
                case BattleActionOutcome.Repel:
                    return 5;
                case BattleActionOutcome.Resist:
                case BattleActionOutcome.Miss:
                    return 4;
                case BattleActionOutcome.Weak:
                case BattleActionOutcome.Critical:
                    return 3;
                default:
                    return 1;
            }
        }

        private static string BuildStartMessage(BattleAction action)
        {
            string actorName = action.Actor != null ? action.Actor.DisplayName : "Someone";
            string skillName = action.Skill != null ? action.Skill.displayName : "an action";
            return $"{actorName} uses {skillName}.";
        }

        public static List<BattleUnit> ResolveTargets(
            BattleSkillDefinition skill,
            BattleUnit actor,
            BattleUnit selectedTarget,
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies)
        {
            List<BattleUnit> targets = new List<BattleUnit>();
            if (skill == null || actor == null)
            {
                return targets;
            }

            IReadOnlyList<BattleUnit> allies = actor.Faction == BattleFaction.Party ? party : enemies;
            IReadOnlyList<BattleUnit> opponents = actor.Faction == BattleFaction.Party ? enemies : party;

            switch (skill.targeting)
            {
                case BattleTargeting.Self:
                    AddIfAlive(targets, actor);
                    break;
                case BattleTargeting.SingleAlly:
                case BattleTargeting.SingleEnemy:
                    AddIfAlive(targets, selectedTarget);
                    break;
                case BattleTargeting.AllAllies:
                    AddAllAlive(targets, allies);
                    break;
                case BattleTargeting.AllEnemies:
                    AddAllAlive(targets, opponents);
                    break;
            }

            return targets;
        }

        private static void AddAllAlive(List<BattleUnit> targets, IReadOnlyList<BattleUnit> units)
        {
            for (int i = 0; i < units.Count; i++)
            {
                AddIfAlive(targets, units[i]);
            }
        }

        private static void AddIfAlive(List<BattleUnit> targets, BattleUnit unit)
        {
            if (unit != null && unit.IsAlive)
            {
                targets.Add(unit);
            }
        }
    }
}
