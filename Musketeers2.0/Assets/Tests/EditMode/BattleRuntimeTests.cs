using System.Collections.Generic;
using Musketeers.Battle;
using NUnit.Framework;
using UnityEngine;

public sealed class BattleRuntimeTests
{
    [Test]
    public void TurnIcons_WeaknessPreservesHalfIcon()
    {
        BattleTurnIconPool pool = new BattleTurnIconPool();
        pool.Reset(3);

        pool.Spend(BattleActionOutcome.Weak);

        Assert.AreEqual(2, pool.FullIcons);
        Assert.AreEqual(1, pool.HalfIcons);
        Assert.AreEqual(3, pool.TotalIcons);
    }

    [Test]
    public void TurnIcons_NullClearsRemainingTurn()
    {
        BattleTurnIconPool pool = new BattleTurnIconPool();
        pool.Reset(4);

        pool.Spend(BattleActionOutcome.Null);

        Assert.AreEqual(0, pool.TotalIcons);
        Assert.IsFalse(pool.HasActions);
    }

    [Test]
    public void AffinityResolver_MapsWeaknessToWeakOutcome()
    {
        BattleUnitDefinition targetDefinition = ScriptableObject.CreateInstance<BattleUnitDefinition>();
        targetDefinition.affinities.Add(new BattleAffinityEntry
        {
            element = BattleElement.Fire,
            response = BattleAffinityResponse.Weak
        });

        BattleUnit target = new BattleUnit(targetDefinition, BattleFaction.Enemy, true);

        Assert.AreEqual(BattleAffinityResponse.Weak, target.GetAffinity(BattleElement.Fire));
        Assert.AreEqual(BattleActionOutcome.Weak, BattleAffinityResolver.ToOutcome(target.GetAffinity(BattleElement.Fire)));
    }

    [Test]
    public void ActionResolver_DamageCanDefeatTarget()
    {
        BattleUnit actor = new BattleUnit(CreateUnit("Hero", 100, 20, 30, 10, 5), BattleFaction.Party, true);
        BattleUnit target = new BattleUnit(CreateUnit("Goblin", 20, 0, 5, 5, 1), BattleFaction.Enemy, true);
        BattleSkillDefinition skill = CreateSkill("Heavy Slash", BattleSkillKind.Damage, BattleElement.Slash, 40);

        BattleResult result = BattleActionResolver.Resolve(new BattleAction(actor, skill, new List<BattleUnit> { target }));

        Assert.IsFalse(target.IsAlive);
        Assert.IsTrue(result.Targets[0].Defeated);
        Assert.Greater(result.Targets[0].Amount, 0);
    }

    [Test]
    public void ActionResolver_HealDoesNotExceedMaxHp()
    {
        BattleUnit healer = new BattleUnit(CreateUnit("Healer", 80, 30, 6, 20, 5), BattleFaction.Party, true);
        BattleUnit target = new BattleUnit(CreateUnit("Ally", 100, 10, 5, 5, 5), BattleFaction.Party, true);
        target.TakeDamage(15);

        BattleSkillDefinition skill = CreateSkill("Heal", BattleSkillKind.Heal, BattleElement.Heal, 80);

        BattleActionResolver.Resolve(new BattleAction(healer, skill, new List<BattleUnit> { target }));

        Assert.AreEqual(100, target.CurrentHP);
    }

    private static BattleUnitDefinition CreateUnit(string name, int hp, int mp, int strength, int magic, int defense)
    {
        BattleUnitDefinition definition = ScriptableObject.CreateInstance<BattleUnitDefinition>();
        definition.displayName = name;
        definition.maxHP = hp;
        definition.maxMP = mp;
        definition.strength = strength;
        definition.magic = magic;
        definition.defense = defense;
        definition.agility = 5;
        return definition;
    }

    private static BattleSkillDefinition CreateSkill(string name, BattleSkillKind kind, BattleElement element, int power)
    {
        BattleSkillDefinition skill = ScriptableObject.CreateInstance<BattleSkillDefinition>();
        skill.displayName = name;
        skill.kind = kind;
        skill.element = element;
        skill.targeting = kind == BattleSkillKind.Heal ? BattleTargeting.SingleAlly : BattleTargeting.SingleEnemy;
        skill.power = power;
        skill.accuracy = 1f;
        return skill;
    }
}
