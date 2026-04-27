using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    public enum BattleActionOutcome
    {
        Normal,
        Weak,
        Critical,
        Resist,
        Miss,
        Null,
        Drain,
        Repel,
        Pass
    }

    public enum BattlePhase
    {
        Idle,
        Setup,
        PlayerCommand,
        PlayerTarget,
        ResolvingAction,
        EnemyTurn,
        Won,
        Lost,
        Escaped
    }

    [Serializable]
    public sealed class BattleStatusEffect
    {
        public string id;
        public string displayName;
        public BattleStat stat;
        public int amount;
        public int turnsRemaining;

        public bool IsExpired => turnsRemaining <= 0;
    }

    public sealed class BattleUnit
    {
        private readonly List<BattleStatusEffect> statusEffects = new List<BattleStatusEffect>();

        public BattleUnit(BattleUnitDefinition definition, BattleFaction faction, bool frontRow)
        {
            Definition = definition;
            Faction = faction;
            FrontRow = frontRow;
            CurrentHP = definition != null ? definition.maxHP : 1;
            CurrentMP = definition != null ? definition.maxMP : 0;
        }

        public BattleUnitDefinition Definition { get; }
        public BattleFaction Faction { get; }
        public bool FrontRow { get; set; }
        public int CurrentHP { get; private set; }
        public int CurrentMP { get; private set; }
        public bool IsGuarding { get; private set; }
        public bool Analysed { get; set; }
        public bool IsAlive => CurrentHP > 0;
        public string DisplayName => Definition != null ? Definition.displayName : "Missing Unit";
        public IReadOnlyList<BattleStatusEffect> StatusEffects => statusEffects;

        public int Strength => GetModifiedStat(BattleStat.Strength, Definition != null ? Definition.strength : 1);
        public int Magic => GetModifiedStat(BattleStat.Magic, Definition != null ? Definition.magic : 1);
        public int Defense => GetModifiedStat(BattleStat.Defense, Definition != null ? Definition.defense : 0);
        public int Agility => GetModifiedStat(BattleStat.Agility, Definition != null ? Definition.agility : 0);

        public bool CanPay(BattleSkillDefinition skill)
        {
            return skill == null || CurrentMP >= skill.mpCost;
        }

        public void SpendCost(BattleSkillDefinition skill)
        {
            if (skill == null)
            {
                return;
            }

            CurrentMP = Mathf.Max(0, CurrentMP - skill.mpCost);
        }

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(0, amount));
        }

        public void Heal(int amount)
        {
            int maxHp = Definition != null ? Definition.maxHP : CurrentHP;
            CurrentHP = Mathf.Min(maxHp, CurrentHP + Mathf.Max(0, amount));
        }

        public void RestoreMP(int amount)
        {
            int maxMp = Definition != null ? Definition.maxMP : CurrentMP;
            CurrentMP = Mathf.Min(maxMp, CurrentMP + Mathf.Max(0, amount));
        }

        public void SetGuarding(bool guarding)
        {
            IsGuarding = guarding;
        }

        public void AddStatus(BattleStatusEffect status)
        {
            if (status == null || string.IsNullOrWhiteSpace(status.id))
            {
                return;
            }

            BattleStatusEffect existing = statusEffects.Find(effect => effect.id == status.id);
            if (existing != null)
            {
                existing.amount = status.amount;
                existing.turnsRemaining = Mathf.Max(existing.turnsRemaining, status.turnsRemaining);
                return;
            }

            statusEffects.Add(status);
        }

        public void TickStatuses()
        {
            for (int i = statusEffects.Count - 1; i >= 0; i--)
            {
                statusEffects[i].turnsRemaining--;
                if (statusEffects[i].IsExpired)
                {
                    statusEffects.RemoveAt(i);
                }
            }
        }

        public BattleAffinityResponse GetAffinity(BattleElement element)
        {
            if (Definition == null || element == BattleElement.Almighty || element == BattleElement.Heal || element == BattleElement.Support)
            {
                return BattleAffinityResponse.Normal;
            }

            return Definition.GetAffinity(element);
        }

        private int GetModifiedStat(BattleStat stat, int baseValue)
        {
            int value = baseValue;
            for (int i = 0; i < statusEffects.Count; i++)
            {
                if (statusEffects[i].stat == stat)
                {
                    value += statusEffects[i].amount;
                }
            }

            return Mathf.Max(1, value);
        }
    }

    public sealed class BattleAction
    {
        public BattleAction(BattleUnit actor, BattleSkillDefinition skill, IReadOnlyList<BattleUnit> targets)
        {
            Actor = actor;
            Skill = skill;
            Targets = targets;
        }

        public BattleUnit Actor { get; }
        public BattleSkillDefinition Skill { get; }
        public IReadOnlyList<BattleUnit> Targets { get; }
    }

    public sealed class BattleTargetResult
    {
        public BattleUnit Target { get; set; }
        public BattleAffinityResponse Affinity { get; set; }
        public BattleActionOutcome Outcome { get; set; }
        public int Amount { get; set; }
        public bool Defeated { get; set; }
    }

    public sealed class BattleResult
    {
        private readonly List<BattleTargetResult> targets = new List<BattleTargetResult>();

        public BattleUnit Actor { get; set; }
        public BattleSkillDefinition Skill { get; set; }
        public BattleActionOutcome TurnOutcome { get; set; } = BattleActionOutcome.Normal;
        public string Message { get; set; }
        public IReadOnlyList<BattleTargetResult> Targets => targets;

        public void AddTarget(BattleTargetResult target)
        {
            if (target != null)
            {
                targets.Add(target);
            }
        }
    }

    public sealed class BattleTurnIconPool
    {
        public int FullIcons { get; private set; }
        public int HalfIcons { get; private set; }
        public int TotalIcons => FullIcons + HalfIcons;
        public bool HasActions => TotalIcons > 0;

        public void Reset(int combatants)
        {
            FullIcons = Mathf.Max(1, combatants);
            HalfIcons = 0;
        }

        public void Clear()
        {
            FullIcons = 0;
            HalfIcons = 0;
        }

        public void Spend(BattleActionOutcome outcome)
        {
            switch (outcome)
            {
                case BattleActionOutcome.Weak:
                case BattleActionOutcome.Critical:
                    ConvertFullToHalfOrSpendHalf();
                    break;
                case BattleActionOutcome.Pass:
                    Pass();
                    break;
                case BattleActionOutcome.Resist:
                case BattleActionOutcome.Miss:
                    SpendOne();
                    SpendOne();
                    break;
                case BattleActionOutcome.Null:
                case BattleActionOutcome.Drain:
                case BattleActionOutcome.Repel:
                    Clear();
                    break;
                default:
                    SpendOne();
                    break;
            }
        }

        private void Pass()
        {
            if (FullIcons > 0)
            {
                FullIcons--;
                HalfIcons++;
                return;
            }

            SpendOne();
        }

        private void ConvertFullToHalfOrSpendHalf()
        {
            if (FullIcons > 0)
            {
                FullIcons--;
                HalfIcons++;
                return;
            }

            SpendOne();
        }

        private void SpendOne()
        {
            if (HalfIcons > 0)
            {
                HalfIcons--;
                return;
            }

            if (FullIcons > 0)
            {
                FullIcons--;
            }
        }
    }

    public static class BattleAffinityResolver
    {
        public static BattleActionOutcome ToOutcome(BattleAffinityResponse affinity)
        {
            switch (affinity)
            {
                case BattleAffinityResponse.Weak:
                    return BattleActionOutcome.Weak;
                case BattleAffinityResponse.Resist:
                    return BattleActionOutcome.Resist;
                case BattleAffinityResponse.Null:
                    return BattleActionOutcome.Null;
                case BattleAffinityResponse.Drain:
                    return BattleActionOutcome.Drain;
                case BattleAffinityResponse.Repel:
                    return BattleActionOutcome.Repel;
                default:
                    return BattleActionOutcome.Normal;
            }
        }
    }

    public static class BattleMath
    {
        public static int CalculateDamage(BattleUnit actor, BattleUnit target, BattleSkillDefinition skill, BattleAffinityResponse affinity)
        {
            if (actor == null || target == null || skill == null)
            {
                return 0;
            }

            int attackStat = IsMagic(skill.element) ? actor.Magic : actor.Strength;
            float rowMultiplier = actor.FrontRow ? 1f : 0.8f;
            float affinityMultiplier = AffinityDamageMultiplier(affinity);
            float guardMultiplier = target.IsGuarding ? 0.5f : 1f;
            int raw = Mathf.RoundToInt(((skill.power + attackStat) - (target.Defense * 0.5f)) * rowMultiplier * affinityMultiplier * guardMultiplier);
            return Mathf.Max(1, raw);
        }

        public static int CalculateHeal(BattleUnit actor, BattleSkillDefinition skill)
        {
            if (actor == null || skill == null)
            {
                return 0;
            }

            return Mathf.Max(1, skill.power + Mathf.RoundToInt(actor.Magic * 0.75f));
        }

        public static bool RollAccuracy(BattleUnit actor, BattleUnit target, BattleSkillDefinition skill)
        {
            if (skill == null || skill.kind != BattleSkillKind.Damage)
            {
                return true;
            }

            float agilityDelta = actor != null && target != null ? (actor.Agility - target.Agility) * 0.01f : 0f;
            float chance = Mathf.Clamp01(skill.accuracy + agilityDelta);
            return UnityEngine.Random.value <= chance;
        }

        private static bool IsMagic(BattleElement element)
        {
            return element == BattleElement.Fire ||
                   element == BattleElement.Ice ||
                   element == BattleElement.Electric ||
                   element == BattleElement.Wind ||
                   element == BattleElement.Light ||
                   element == BattleElement.Dark ||
                   element == BattleElement.Almighty;
        }

        private static float AffinityDamageMultiplier(BattleAffinityResponse affinity)
        {
            switch (affinity)
            {
                case BattleAffinityResponse.Weak:
                    return 1.35f;
                case BattleAffinityResponse.Resist:
                    return 0.5f;
                default:
                    return 1f;
            }
        }
    }
}
