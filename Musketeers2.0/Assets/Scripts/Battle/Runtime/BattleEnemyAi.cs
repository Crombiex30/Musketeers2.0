using System.Collections.Generic;

namespace Musketeers.Battle
{
    public sealed class BattleEnemyAi
    {
        public BattleAction ChooseAction(BattleUnit actor, IReadOnlyList<BattleUnit> party, IReadOnlyList<BattleUnit> enemies)
        {
            if (actor == null || actor.Definition == null)
            {
                return null;
            }

            BattleSkillDefinition skill = ChooseSkill(actor, party);
            BattleUnit target = ChooseTarget(actor, skill, party, enemies);
            List<BattleUnit> targets = BattleActionResolver.ResolveTargets(skill, actor, target, party, enemies);
            return new BattleAction(actor, skill, targets);
        }

        private static BattleSkillDefinition ChooseSkill(BattleUnit actor, IReadOnlyList<BattleUnit> party)
        {
            BattleSkillDefinition bestSkill = actor.Definition.basicAttack;
            BattleUnit weakestTarget = FindWeakestLiving(party);

            for (int i = 0; i < actor.Definition.skills.Count; i++)
            {
                BattleSkillDefinition candidate = actor.Definition.skills[i];
                if (candidate == null || !actor.CanPay(candidate))
                {
                    continue;
                }

                if (bestSkill == null)
                {
                    bestSkill = candidate;
                    continue;
                }

                if (candidate.kind == BattleSkillKind.Heal && NeedsHealing(actor))
                {
                    return candidate;
                }

                if (candidate.kind == BattleSkillKind.Damage &&
                    weakestTarget != null &&
                    weakestTarget.GetAffinity(candidate.element) == BattleAffinityResponse.Weak)
                {
                    return candidate;
                }

                if (candidate.kind == BattleSkillKind.Damage && candidate.power > bestSkill.power)
                {
                    bestSkill = candidate;
                }
            }

            return bestSkill;
        }

        private static BattleUnit ChooseTarget(
            BattleUnit actor,
            BattleSkillDefinition skill,
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies)
        {
            if (skill == null)
            {
                return FindWeakestLiving(party);
            }

            switch (skill.targeting)
            {
                case BattleTargeting.SingleAlly:
                    return FindWeakestLiving(enemies);
                case BattleTargeting.Self:
                    return actor;
                case BattleTargeting.SingleEnemy:
                    return FindBestOffensiveTarget(skill, party);
                default:
                    return FindWeakestLiving(party);
            }
        }

        private static BattleUnit FindBestOffensiveTarget(BattleSkillDefinition skill, IReadOnlyList<BattleUnit> targets)
        {
            BattleUnit fallback = FindWeakestLiving(targets);
            for (int i = 0; i < targets.Count; i++)
            {
                BattleUnit target = targets[i];
                if (target != null && target.IsAlive && target.GetAffinity(skill.element) == BattleAffinityResponse.Weak)
                {
                    return target;
                }
            }

            return fallback;
        }

        private static BattleUnit FindWeakestLiving(IReadOnlyList<BattleUnit> units)
        {
            BattleUnit weakest = null;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                if (unit == null || !unit.IsAlive)
                {
                    continue;
                }

                if (weakest == null || unit.CurrentHP < weakest.CurrentHP)
                {
                    weakest = unit;
                }
            }

            return weakest;
        }

        private static bool NeedsHealing(BattleUnit unit)
        {
            return unit.Definition != null && unit.CurrentHP <= unit.Definition.maxHP / 2;
        }
    }
}