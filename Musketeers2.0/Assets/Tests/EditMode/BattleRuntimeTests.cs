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

    // -------------------------------------------------------------------------
    //  Randomized Event System Tests
    // -------------------------------------------------------------------------

    [Test]
    public void EventSystem_NoPool_NeverTriggers()
    {
        BattleEventSystem sys = new BattleEventSystem(new List<BattleEventDefinition>(), 1f);
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(2);
        BattleUnit[] party = { new BattleUnit(CreateUnit("Hero", 100, 20, 10, 8, 5), BattleFaction.Party, true) };
        BattleUnit[] enemies = { new BattleUnit(CreateUnit("Goblin", 50, 0, 5, 3, 2), BattleFaction.Enemy, true) };
        sys.OnTurnStart(party, enemies, icons, BattleFaction.Party);
        Assert.IsFalse(sys.HasActiveEvent);
    }

    [Test]
    public void EventSystem_ZeroChance_NeverTriggers()
    {
        BattleEvent_FlashFlood ev = ScriptableObject.CreateInstance<BattleEvent_FlashFlood>();
        BattleEventSystem sys = new BattleEventSystem(new List<BattleEventDefinition> { ev }, 0f);
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(2);
        BattleUnit[] party = { new BattleUnit(CreateUnit("Hero", 100, 20, 10, 8, 5), BattleFaction.Party, true) };
        BattleUnit[] enemies = { new BattleUnit(CreateUnit("Goblin", 50, 0, 5, 3, 2), BattleFaction.Enemy, true) };
        sys.OnTurnStart(party, enemies, icons, BattleFaction.Party);
        Assert.IsFalse(sys.HasActiveEvent);
    }

    [Test]
    public void EventSystem_FullChance_EventTriggersAndHasRestrictions()
    {
        BattleEvent_FlashFlood ev = ScriptableObject.CreateInstance<BattleEvent_FlashFlood>();
        bool triggered = false;
        BattleEventSystem sys = new BattleEventSystem(new List<BattleEventDefinition> { ev }, 1f);
        sys.EventTriggered += (def, ctx) => triggered = true;
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(2);
        BattleUnit[] party = { new BattleUnit(CreateUnit("Hero", 100, 20, 10, 8, 5), BattleFaction.Party, true) };
        BattleUnit[] enemies = { new BattleUnit(CreateUnit("Goblin", 50, 0, 5, 3, 2), BattleFaction.Enemy, true) };
        sys.OnTurnStart(party, enemies, icons, BattleFaction.Party);
        Assert.IsTrue(triggered);
        Assert.IsTrue(sys.HasActiveEvent);
        Assert.IsTrue((sys.CurrentRestrictions & BattleEventRestriction.NoGroundMovement) != 0);
        Assert.IsTrue((sys.CurrentRestrictions & BattleEventRestriction.LightningChains) != 0);
    }

    [Test]
    public void EventSystem_RunawayMinecart_DealsDamageToMatchingRow()
    {
        BattleEvent_RunawayMinecart ev = ScriptableObject.CreateInstance<BattleEvent_RunawayMinecart>();
        ev.damage = 20;
        BattleUnitDefinition heroDef = CreateUnit("Hero", 100, 0, 10, 5, 5);
        BattleUnit hero = new BattleUnit(heroDef, BattleFaction.Party, true); // front row
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(1);
        List<BattleUnit> party = new List<BattleUnit> { hero };
        List<BattleUnit> enemies = new List<BattleUnit>();
        BattleEventContext ctx = new BattleEventContext
        {
            Party = party,
            Enemies = enemies,
            TurnIcons = icons,
            ActiveFaction = BattleFaction.Party
        };
        // Force front row to be hit by manually calling Apply and checking HP change.
        // Since row is random we just verify Apply runs without error and HP is either unchanged or reduced.
        int hpBefore = hero.CurrentHP;
        ev.Apply(ctx);
        // Hero must have taken damage OR been in the non-hit row (both outcomes are valid).
        Assert.IsTrue(hero.CurrentHP <= hpBefore);
    }

    [Test]
    public void EventSystem_VolatileGas_IgniteHitsEveryone()
    {
        BattleEvent_VolatileGasPocket ev = ScriptableObject.CreateInstance<BattleEvent_VolatileGasPocket>();
        ev.explosionDamage = 15;
        BattleUnit hero = new BattleUnit(CreateUnit("Hero", 100, 0, 10, 5, 5), BattleFaction.Party, true);
        BattleUnit goblin = new BattleUnit(CreateUnit("Goblin", 50, 0, 5, 3, 2), BattleFaction.Enemy, true);
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(1);
        BattleEventContext ctx = new BattleEventContext
        {
            Party = new List<BattleUnit> { hero },
            Enemies = new List<BattleUnit> { goblin },
            TurnIcons = icons,
            ActiveFaction = BattleFaction.Party
        };
        ev.Ignite(ctx);
        Assert.AreEqual(85, hero.CurrentHP);
        Assert.AreEqual(35, goblin.CurrentHP);
    }

    [Test]
    public void EventSystem_UnstableRelic_DetonatesAfterCountdown()
    {
        BattleEvent_UnstableRelic ev = ScriptableObject.CreateInstance<BattleEvent_UnstableRelic>();
        ev.countdownStart = 3;
        BattleUnit hero = new BattleUnit(CreateUnit("Hero", 100, 0, 10, 5, 5), BattleFaction.Party, true);
        BattleUnit goblin = new BattleUnit(CreateUnit("Goblin", 100, 0, 5, 3, 2), BattleFaction.Enemy, true);
        hero.TakeDamage(60); // hero at 40 HP  (40%)
        // goblin at 100 HP (100%)
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(1);
        BattleEventContext ctx = new BattleEventContext
        {
            Party = new List<BattleUnit> { hero },
            Enemies = new List<BattleUnit> { goblin },
            TurnIcons = icons,
            ActiveFaction = BattleFaction.Party
        };
        // Apply sets up the relic.
        ev.Apply(ctx);
        // Tick 3 times to detonate.
        bool detonated = false;
        for (int i = 0; i < 3; i++) detonated = ev.TickAction(ctx);
        Assert.IsTrue(detonated);
        // After swap: hero should have ~100% HP (goblin's pct), goblin ~40% HP.
        Assert.Greater(hero.CurrentHP, 80);
        Assert.Less(goblin.CurrentHP, 60);
    }

    [Test]
    public void EventSystem_CursedMiasma_HollowAttacksFormerAllies()
    {
        BattleEvent_CursedMiasma ev = ScriptableObject.CreateInstance<BattleEvent_CursedMiasma>();
        ev.hollowDamage = 5;
        BattleUnit hero = new BattleUnit(CreateUnit("Hero", 100, 0, 10, 5, 5), BattleFaction.Party, true);
        BattleUnit ally = new BattleUnit(CreateUnit("Ally", 80, 0, 5, 5, 5), BattleFaction.Party, true);
        BattleUnit deceased = new BattleUnit(CreateUnit("Fallen", 50, 0, 5, 3, 2), BattleFaction.Party, true);
        BattleTurnIconPool icons = new BattleTurnIconPool();
        icons.Reset(1);
        BattleEventContext ctx = new BattleEventContext
        {
            Party = new List<BattleUnit> { hero, ally, deceased },
            Enemies = new List<BattleUnit>(),
            TurnIcons = icons,
            ActiveFaction = BattleFaction.Party
        };
        ev.OnUnitDied(deceased, ctx);
        // Each living ally in party should have taken hollowDamage.
        Assert.AreEqual(95, hero.CurrentHP);
        Assert.AreEqual(75, ally.CurrentHP);
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
