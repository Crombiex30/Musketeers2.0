using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    public enum BattleFaction
    {
        Party,
        Enemy
    }

    public enum BattleStat
    {
        Strength,
        Magic,
        Defense,
        Agility
    }

    public enum BattleSkillKind
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Guard,
        Analyse,
        Pass,
        Escape
    }

    public enum BattleTargeting
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    public enum BattleElement
    {
        Slash,
        Strike,
        Pierce,
        Fire,
        Ice,
        Electric,
        Wind,
        Light,
        Dark,
        Almighty,
        Heal,
        Support
    }

    public enum BattleAffinityResponse
    {
        Normal,
        Weak,
        Resist,
        Null,
        Drain,
        Repel
    }

    [Serializable]
    public sealed class BattleAffinityEntry
    {
        public BattleElement element;
        public BattleAffinityResponse response = BattleAffinityResponse.Normal;
    }

    [Serializable]
    public sealed class BattleStatModifier
    {
        public BattleStat stat;
        public int amount;
        [Min(1)] public int turns = 3;
    }

    [Serializable]
    public sealed class BattleFormationSlot
    {
        public BattleUnitDefinition unit;
        public Vector3 scenePosition;
        public bool frontRow = true;
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Unit Definition")]
    public sealed class BattleUnitDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Unit";
        public Sprite portrait;
        public GameObject battlePrefab;
        public BattleFaction defaultFaction = BattleFaction.Party;
        public bool playerControlled = true;

        [Header("Stats")]
        [Min(1)] public int maxHP = 100;
        [Min(0)] public int maxMP = 30;
        public int strength = 10;
        public int magic = 8;
        public int defense = 5;
        public int agility = 5;

        [Header("Battle Options")]
        public BattleSkillDefinition basicAttack;
        public List<BattleSkillDefinition> skills = new List<BattleSkillDefinition>();
        public List<BattleAffinityEntry> affinities = new List<BattleAffinityEntry>();

        public BattleAffinityResponse GetAffinity(BattleElement element)
        {
            for (int i = 0; i < affinities.Count; i++)
            {
                if (affinities[i].element == element)
                {
                    return affinities[i].response;
                }
            }

            return BattleAffinityResponse.Normal;
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Skill Definition")]
    public sealed class BattleSkillDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Skill";
        [TextArea] public string description;
        public Sprite icon;

        [Header("Rules")]
        public BattleSkillKind kind = BattleSkillKind.Damage;
        public BattleElement element = BattleElement.Slash;
        public BattleTargeting targeting = BattleTargeting.SingleEnemy;
        [Min(0)] public int mpCost;
        [Min(0)] public int power = 10;
        [Range(0f, 1f)] public float accuracy = 0.95f;
        public bool canTriggerWeakness = true;
        public bool endsTurnOnUse = true;
        public List<BattleStatModifier> statModifiers = new List<BattleStatModifier>();

        [Header("Presentation")]
        public BattleVfxDefinition vfx;
        public AudioClip audioCue;
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Encounter Definition")]
    public sealed class BattleEncounterDefinition : ScriptableObject
    {
        public string encounterName = "Encounter";
        public List<BattleFormationSlot> party = new List<BattleFormationSlot>();
        public List<BattleFormationSlot> enemies = new List<BattleFormationSlot>();
        public AudioClip battleMusic;
        public BattleUiTheme uiTheme;

        [Header("Randomized Events")]
        [Tooltip("Pool of events that can be drawn each turn. Leave empty to disable the event system.")]
        public List<BattleEventDefinition> eventPool = new List<BattleEventDefinition>();
        [Range(0f, 1f)]
        [Tooltip("Probability per-turn that a random event triggers.")]
        public float eventChance = 0.4f;
    }

    // -------------------------------------------------------------------------
    //  Randomized Combat Events
    // -------------------------------------------------------------------------

    public enum BattleEventId
    {
        // Level 1 – Fantasy Underground Mine
        RunawayMinecart = 1,
        VolatileGasPocket = 2,
        CrystalResonance = 3,
        GraveRobbersFolly = 4,
        PitchBlack = 5,
        PowderKegHotPotato = 6,
        TheMotherlode = 7,
        StalactiteShadows = 8,
        FlashFlood = 9,
        SeismicChasm = 10,

        // Level 2 – Hybrid Mine / Ruins
        ExcavationCollapse = 11,
        AwakenedGargoyle = 12,
        CursedMiasma = 13,
        AncientDefenseTurret = 14,
        LeylineEruption = 15,
        SpectralAudience = 16,
        MagneticOre = 17,
        FungalTether = 18,
        UnstableRelic = 19,
        SmugglersTunnels = 20,

        // Level 3 – Sprawling Ancient Civilization
        CrumblingAqueduct = 21,
        JudgmentOfTheForgottenGods = 22,
        ChronosFracture = 23,
        ToweringColossus = 24,
        AncestralPossession = 25,
        ArchitectsGrid = 26,
        AetherialHivemind = 27,
        VoidRift = 28,
        HolographicScramble = 29,
        PillarsOfTheHeavens = 30
    }

    /// <summary>
    /// Flags that gate which actions are legal while an event is active.
    /// Multiple flags may be combined.
    /// </summary>
    [Flags]
    public enum BattleEventRestriction
    {
        None                 = 0,
        NoVoiceSpells        = 1 << 0,   // VolatileGasPocket
        NoGroundMovement     = 1 << 1,   // FlashFlood, CrumblingAqueduct
        NoRangedAttacks      = 1 << 2,   // SeismicChasm (cross-chasm), VoidRift, PillarsOfTheHeavens
        NoFreeTargeting      = 1 << 3,   // PitchBlack (back-row attacks random)
        ForcedFrontRow       = 1 << 4,   // MagneticOre (heavy-armored units pulled forward)
        NoHealingMagic       = 1 << 5,   // JudgmentOfTheForgottenGods – Hubris side
        ZeroMpCosts          = 1 << 6,   // JudgmentOfTheForgottenGods – Desperation side
        SharedActionPoints   = 1 << 7,   // AetherialHivemind
        KnightMoveBonus      = 1 << 8,   // ArchitectsGrid
        FlankedNextAttack    = 1 << 9,   // SmugglersTunnels (guaranteed backstab flag)
        AttackReplaced       = 1 << 10,  // LeylineEruption (standard Attack → Ancient Magic)
        LightningChains      = 1 << 11,  // FlashFlood
        PhantomRepeat        = 1 << 12,  // ChronosFracture
        BlindedCombatants    = 1 << 13,  // HolographicScramble
    }

    /// <summary>
    /// Base ScriptableObject for all randomized combat events.
    /// Subclass and override <see cref="Apply"/> / <see cref="OnTurnEnd"/> for custom logic.
    /// </summary>
    public abstract class BattleEventDefinition : ScriptableObject
    {
        [Header("Event Identity")]
        public BattleEventId eventId;
        public string displayName = "Event";
        [TextArea] public string description;
        public Sprite eventIcon;

        [Header("Duration")]
        [Tooltip("How many full turns the event stays active. 0 = expires at end of this turn.")]
        [Min(0)] public int durationTurns = 1;

        [Header("Restrictions")]
        [Tooltip("Which actions are blocked / modified while this event is active.")]
        public BattleEventRestriction restrictions = BattleEventRestriction.None;

        /// <summary>Applied immediately when the event triggers.</summary>
        public abstract void Apply(BattleEventContext ctx);

        /// <summary>Called at the end of each turn the event is active.</summary>
        public virtual void OnTurnEnd(BattleEventContext ctx) { }

        /// <summary>Called when the event expires (duration hits 0).</summary>
        public virtual void OnExpire(BattleEventContext ctx) { }
    }

    /// <summary>
    /// All the live battle state the event needs to read/mutate.
    /// </summary>
    public sealed class BattleEventContext
    {
        public IReadOnlyList<BattleUnit> Party;
        public IReadOnlyList<BattleUnit> Enemies;
        public BattleTurnIconPool TurnIcons;
        public BattleFaction ActiveFaction;

        /// <summary>Messages queued by the event to be displayed in the battle log.</summary>
        public readonly List<string> Messages = new List<string>();

        /// <summary>
        /// Which unit (if any) has been specially marked by the event
        /// (e.g., turret target, tethered unit, possessed unit).
        /// </summary>
        public BattleUnit MarkedUnit;

        /// <summary>Secondary marked unit for two-unit interactions (e.g., Fungal Tether pair).</summary>
        public BattleUnit SecondMarkedUnit;

        public void Log(string msg) => Messages.Add(msg);

        public BattleUnit RandomLiving(IReadOnlyList<BattleUnit> pool)
        {
            if (pool == null || pool.Count == 0)
            {
                return null;
            }

            int count = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && pool[i].IsAlive) count++;
            }
            if (count == 0) return null;
            int pick = UnityEngine.Random.Range(0, count);
            int seen = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && pool[i].IsAlive)
                {
                    if (seen == pick) return pool[i];
                    seen++;
                }
            }
            return null;
        }

        public BattleUnit RandomLivingAny()
        {
            List<BattleUnit> all = new List<BattleUnit>();
            for (int i = 0; i < Party.Count; i++)
                if (Party[i] != null && Party[i].IsAlive) all.Add(Party[i]);
            for (int i = 0; i < Enemies.Count; i++)
                if (Enemies[i] != null && Enemies[i].IsAlive) all.Add(Enemies[i]);
            if (all.Count == 0) return null;
            return all[UnityEngine.Random.Range(0, all.Count)];
        }
    }

    // -------------------------------------------------------------------------
    //  Level 1 – Fantasy Underground Mine Events
    // -------------------------------------------------------------------------

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Runaway Minecart")]
    public sealed class BattleEvent_RunawayMinecart : BattleEventDefinition
    {
        [Min(1)] public int damage = 30;

        public override void Apply(BattleEventContext ctx)
        {
            // Hit every unit in a random "row". We model rows as FrontRow == true/false.
            // Pick a row (0 = front, 1 = back) randomly.
            bool hitFront = UnityEngine.Random.value < 0.5f;
            string rowName = hitFront ? "front" : "back";
            ctx.Log($"A runaway minecart barrels down the {rowName} row!");

            foreach (BattleUnit u in ctx.Party)
            {
                if (u.IsAlive && u.FrontRow == hitFront)
                {
                    u.TakeDamage(damage);
                    ctx.Log($"{u.DisplayName} was hit by the minecart for {damage} damage!");
                }
            }
            foreach (BattleUnit u in ctx.Enemies)
            {
                if (u.IsAlive && u.FrontRow == hitFront)
                {
                    u.TakeDamage(damage);
                    ctx.Log($"{u.DisplayName} was hit by the minecart for {damage} damage!");
                }
            }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Volatile Gas Pocket")]
    public sealed class BattleEvent_VolatileGasPocket : BattleEventDefinition
    {
        [Min(1)] public int explosionDamage = 40;

        public BattleEvent_VolatileGasPocket()
        {
            restrictions = BattleEventRestriction.NoVoiceSpells;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Toxic gas floods the battlefield! Voice-based spells are blocked.");
            ctx.Log("WARNING: Casting any Fire spell will ignite the gas!");
        }

        /// <summary>
        /// Called externally by BattleController when a Fire spell is cast during this event.
        /// </summary>
        public void Ignite(BattleEventContext ctx)
        {
            ctx.Log("The gas ignites! Massive AoE explosion!");
            foreach (BattleUnit u in ctx.Party)
                if (u.IsAlive) { u.TakeDamage(explosionDamage); ctx.Log($"{u.DisplayName} took {explosionDamage} explosion damage!"); }
            foreach (BattleUnit u in ctx.Enemies)
                if (u.IsAlive) { u.TakeDamage(explosionDamage); ctx.Log($"{u.DisplayName} took {explosionDamage} explosion damage!"); }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Crystal Resonance")]
    public sealed class BattleEvent_CrystalResonance : BattleEventDefinition
    {
        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Magic crystals sprout from the ground! Spells may bounce to random targets.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Grave Robbers Folly")]
    public sealed class BattleEvent_GraveRobbersFolly : BattleEventDefinition
    {
        [Min(1)] public int minerCount = 2;
        [Min(1)] public int minerHp = 15;
        [Min(1)] public int minerDamage = 8;

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log($"{minerCount} Neutral Undead Miners dig up from the ground! They attack the party's front row!");
            // Miners deal damage to the highest-HP front-row party member as a simple simulation.
            BattleUnit target = HighestHpFront(ctx.Party);
            if (target != null)
            {
                int totalDmg = minerDamage * minerCount;
                target.TakeDamage(totalDmg);
                ctx.Log($"The Undead Miners slam {target.DisplayName} for {totalDmg} damage!");
            }
        }

        private static BattleUnit HighestHpFront(IReadOnlyList<BattleUnit> units)
        {
            BattleUnit best = null;
            foreach (BattleUnit u in units)
                if (u.IsAlive && u.FrontRow && (best == null || u.CurrentHP > best.CurrentHP))
                    best = u;
            return best;
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Pitch Black")]
    public sealed class BattleEvent_PitchBlack : BattleEventDefinition
    {
        public BattleEvent_PitchBlack()
        {
            restrictions = BattleEventRestriction.NoFreeTargeting;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The torches go out! Back-row enemies cannot be targeted specifically — attacks hit randomly!");
        }

        public override void OnExpire(BattleEventContext ctx)
        {
            ctx.Log("A torch is relit. Visibility restored.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Powder Keg Hot Potato")]
    public sealed class BattleEvent_PowderKegHotPotato : BattleEventDefinition
    {
        [Min(1)] public int fuseTurns = 3;
        [Min(1)] public int explosionDamage = 60;

        private int _fuse;
        private bool _onEnemySide;

        public override void Apply(BattleEventContext ctx)
        {
            _fuse = fuseTurns;
            _onEnemySide = false;
            ctx.Log($"A Powder Keg with a {fuseTurns}-turn fuse drops into the arena! Push it to the enemy side!");
        }

        public override void OnTurnEnd(BattleEventContext ctx)
        {
            _fuse--;
            ctx.Log($"Powder Keg fuse: {_fuse} turn(s) remaining.");
            if (_fuse <= 0)
            {
                IReadOnlyList<BattleUnit> victims = _onEnemySide ? ctx.Enemies : ctx.Party;
                ctx.Log("BOOM! The Powder Keg explodes!");
                foreach (BattleUnit u in victims)
                    if (u.IsAlive) { u.TakeDamage(explosionDamage); ctx.Log($"{u.DisplayName} took {explosionDamage} blast damage!"); }
            }
        }

        /// <summary>Player calls this to push the keg toward enemies.</summary>
        public void PushToEnemy(BattleEventContext ctx) { _onEnemySide = true; ctx.Log("Powder Keg pushed toward the enemies!"); }
        /// <summary>Enemy AI calls this to push back.</summary>
        public void PushToParty(BattleEventContext ctx) { _onEnemySide = false; ctx.Log("Powder Keg pushed back toward the party!"); }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/The Motherlode")]
    public sealed class BattleEvent_TheMotherlode : BattleEventDefinition
    {
        [Min(0)] public int goldReward = 200;

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log($"A massive gold vein appears! Spend a turn to mine it and gain {goldReward} gold — but beast enemies may get distracted!");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Stalactite Shadows")]
    public sealed class BattleEvent_StalactiteShadows : BattleEventDefinition
    {
        [Min(1)] public int stalactiteCount = 3;
        [Min(1)] public int pinDamage = 25;

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log($"Shadows appear over {stalactiteCount} grid spaces! Stalactites will fall at the end of this round!");
            // Damage random living units across both sides.
            List<BattleUnit> candidates = new List<BattleUnit>();
            foreach (BattleUnit u in ctx.Party) if (u.IsAlive) candidates.Add(u);
            foreach (BattleUnit u in ctx.Enemies) if (u.IsAlive) candidates.Add(u);

            int hits = Mathf.Min(stalactiteCount, candidates.Count);
            for (int i = 0; i < hits; i++)
            {
                int idx = UnityEngine.Random.Range(0, candidates.Count);
                BattleUnit victim = candidates[idx];
                candidates.RemoveAt(idx);
                victim.TakeDamage(pinDamage);
                victim.AddStatus(new BattleStatusEffect { id = "Pinned", displayName = "Pinned", turnsRemaining = 1, stat = BattleStat.Agility, amount = -999 });
                ctx.Log($"A stalactite pins {victim.DisplayName} for {pinDamage} damage! They are pinned!");
            }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Flash Flood")]
    public sealed class BattleEvent_FlashFlood : BattleEventDefinition
    {
        public BattleEvent_FlashFlood()
        {
            restrictions = BattleEventRestriction.NoGroundMovement | BattleEventRestriction.LightningChains;
            durationTurns = 2;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Pipes burst! The arena floods. Ground movement halved — but Lightning magic chains to all units!");
        }

        public override void OnExpire(BattleEventContext ctx)
        {
            ctx.Log("The floodwaters recede.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level1/Seismic Chasm")]
    public sealed class BattleEvent_SeismicChasm : BattleEventDefinition
    {
        public BattleEvent_SeismicChasm()
        {
            restrictions = BattleEventRestriction.NoRangedAttacks;
            durationTurns = 2;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The battlefield splits in two! Melee attacks across sides are disabled for 2 turns. Only ranged/magic can cross.");
        }

        public override void OnExpire(BattleEventContext ctx)
        {
            ctx.Log("The chasm closes.");
        }
    }

    // -------------------------------------------------------------------------
    //  Level 2 – Hybrid Mine / Ruins Events
    // -------------------------------------------------------------------------

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Excavation Collapse")]
    public sealed class BattleEvent_ExcavationCollapse : BattleEventDefinition
    {
        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The floor collapses! All combatants drop into an ancient ruin. Turn order scrambled, all buffs/debuffs stripped!");
            StripStatuses(ctx.Party);
            StripStatuses(ctx.Enemies);
            // Randomize turn-icon pool count to simulate scrambled order.
            int living = 0;
            foreach (BattleUnit u in ctx.Party) if (u.IsAlive) living++;
            foreach (BattleUnit u in ctx.Enemies) if (u.IsAlive) living++;
            ctx.TurnIcons.Reset(Mathf.Max(1, living / 2));
            ctx.Log("All status effects stripped!");
        }

        private static void StripStatuses(IReadOnlyList<BattleUnit> units)
        {
            foreach (BattleUnit u in units)
            {
                while (u.StatusEffects.Count > 0)
                    u.TickStatuses(); // tick to 0 to clear — brute force safe given small counts
            }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Awakened Gargoyle")]
    public sealed class BattleEvent_AwakenedGargoyle : BattleEventDefinition
    {
        [Min(1)] public int gargoyleHp = 3; // hits before it shatters
        [Min(1)] public int shatterDamage = 20;

        private int _hitsRemaining;

        public override void Apply(BattleEventContext ctx)
        {
            _hitsRemaining = gargoyleHp;
            ctx.Log($"A stone gargoyle awakens in the middle of the field! It can absorb {gargoyleHp} ranged hits before shattering.");
        }

        /// <summary>Call when a ranged attack would be blocked by the gargoyle.</summary>
        public bool AbsorbHit(BattleEventContext ctx)
        {
            _hitsRemaining--;
            ctx.Log($"The gargoyle absorbs the hit! ({_hitsRemaining} hits remaining)");
            if (_hitsRemaining <= 0)
            {
                Shatter(ctx);
                return false;
            }
            return true;
        }

        private void Shatter(BattleEventContext ctx)
        {
            ctx.Log("The gargoyle shatters into shrapnel! Everyone nearby takes damage!");
            foreach (BattleUnit u in ctx.Party) if (u.IsAlive) { u.TakeDamage(shatterDamage); ctx.Log($"{u.DisplayName} took {shatterDamage} shrapnel damage!"); }
            foreach (BattleUnit u in ctx.Enemies) if (u.IsAlive) { u.TakeDamage(shatterDamage); ctx.Log($"{u.DisplayName} took {shatterDamage} shrapnel damage!"); }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Cursed Miasma")]
    public sealed class BattleEvent_CursedMiasma : BattleEventDefinition
    {
        [Min(1)] public int hollowDamage = 5; // damage the Hollow deals on its one attack

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The boundary between life and death thins! The next unit to fall will rise as a Hollow and strike their former allies!");
        }

        /// <summary>
        /// Call when any unit dies while this event is active.
        /// The Hollow deals one retaliatory hit to the deceased's former allies.
        /// </summary>
        public void OnUnitDied(BattleUnit deceased, BattleEventContext ctx)
        {
            IReadOnlyList<BattleUnit> former = deceased.Faction == BattleFaction.Party ? ctx.Party : ctx.Enemies;
            ctx.Log($"{deceased.DisplayName} rises as a Hollow!");
            foreach (BattleUnit ally in former)
            {
                if (ally.IsAlive)
                {
                    ally.TakeDamage(hollowDamage);
                    ctx.Log($"The Hollow strikes {ally.DisplayName} for {hollowDamage} damage before turning to dust!");
                }
            }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Ancient Defense Turret")]
    public sealed class BattleEvent_AncientDefenseTurret : BattleEventDefinition
    {
        [Min(1)] public int turretDamage = 50;

        public override void Apply(BattleEventContext ctx)
        {
            BattleUnit highest = HighestDamageDealer(ctx.Party);
            if (highest == null) highest = ctx.RandomLiving(ctx.Party);
            ctx.MarkedUnit = highest;
            ctx.Log($"An ancient ballista boots up! Its laser sight locks onto {highest?.DisplayName ?? "a target"}. Defend, move, or redirect it before end of round!");
        }

        public override void OnTurnEnd(BattleEventContext ctx)
        {
            BattleUnit target = ctx.MarkedUnit;
            if (target != null && target.IsAlive)
            {
                target.TakeDamage(turretDamage);
                ctx.Log($"The ballista fires! {target.DisplayName} takes {turretDamage} damage!");
            }
        }

        private static BattleUnit HighestDamageDealer(IReadOnlyList<BattleUnit> units)
        {
            // Without a damage-tracker, approximate by highest Strength+Magic.
            BattleUnit best = null;
            foreach (BattleUnit u in units)
                if (u.IsAlive && (best == null || (u.Strength + u.Magic) > (best.Strength + best.Magic)))
                    best = u;
            return best;
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Leyline Eruption")]
    public sealed class BattleEvent_LeylineEruption : BattleEventDefinition
    {
        public BattleEvent_LeylineEruption()
        {
            restrictions = BattleEventRestriction.AttackReplaced;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Runic tiles glow on the ground! Standing on one replaces your Attack with a devastating Ancient Magic spell — but roots you in place.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Spectral Audience")]
    public sealed class BattleEvent_SpectralAudience : BattleEventDefinition
    {
        [Min(1)] public int cheerHeal = 10;
        [Min(1)] public int booRockDamage = 8;

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The ghosts of the ruined city appear to watch! Critical hits earn healing cheers; missed attacks bring damaging boos!");
        }

        public void OnCritical(BattleUnit beneficiary, BattleEventContext ctx)
        {
            if (beneficiary == null || !beneficiary.IsAlive) return;
            beneficiary.Heal(cheerHeal);
            ctx.Log($"The crowd cheers! {beneficiary.DisplayName} healed for {cheerHeal} HP!");
        }

        public void OnMiss(BattleUnit punished, BattleEventContext ctx)
        {
            if (punished == null || !punished.IsAlive) return;
            punished.TakeDamage(booRockDamage);
            ctx.Log($"The crowd boos and throws rocks! {punished.DisplayName} takes {booRockDamage} damage!");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Magnetic Ore")]
    public sealed class BattleEvent_MagneticOre : BattleEventDefinition
    {
        public BattleEvent_MagneticOre()
        {
            restrictions = BattleEventRestriction.ForcedFrontRow;
            durationTurns = 2;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Magnetized ore activates! All heavily-armored units are pulled to the front row and cannot retreat for 2 turns.");
            // Pull high-defense units forward (defense >= 15 as threshold for 'heavy armor').
            foreach (BattleUnit u in ctx.Party)
                if (u.IsAlive && u.Defense >= 15) { u.FrontRow = true; ctx.Log($"{u.DisplayName} is yanked to the front row!"); }
            foreach (BattleUnit u in ctx.Enemies)
                if (u.IsAlive && u.Defense >= 15) { u.FrontRow = true; ctx.Log($"{u.DisplayName} is yanked to the front row!"); }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Fungal Tether")]
    public sealed class BattleEvent_FungalTether : BattleEventDefinition
    {
        public override void Apply(BattleEventContext ctx)
        {
            BattleUnit ally = ctx.RandomLiving(ctx.Party);
            BattleUnit enemy = ctx.RandomLiving(ctx.Enemies);
            if (ally == null || enemy == null) return;
            ctx.MarkedUnit = ally;
            ctx.SecondMarkedUnit = enemy;
            ctx.Log($"Glowing spores tether {ally.DisplayName} to {enemy.DisplayName}! Healing one heals the other; poisoning one poisons the other.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Unstable Relic")]
    public sealed class BattleEvent_UnstableRelic : BattleEventDefinition
    {
        [Min(1)] public int countdownStart = 6;

        private int _actionCount;

        public override void Apply(BattleEventContext ctx)
        {
            _actionCount = 0;
            ctx.Log($"A glowing relic drops onto the field! In {countdownStart} actions it will swap the HP percentages of both sides. Defuse it!");
        }

        /// <summary>Call each time any unit takes an action.</summary>
        public bool TickAction(BattleEventContext ctx)
        {
            _actionCount++;
            int remaining = countdownStart - _actionCount;
            ctx.Log($"Relic countdown: {remaining}");
            if (remaining <= 0)
            {
                Detonate(ctx);
                return true; // exploded
            }
            return false;
        }

        private void Detonate(BattleEventContext ctx)
        {
            ctx.Log("The Unstable Relic DETONATES! HP percentages swapped between both sides!");
            float partyPct = AverageHpPct(ctx.Party);
            float enemyPct = AverageHpPct(ctx.Enemies);
            ApplyPct(ctx.Party, enemyPct);
            ApplyPct(ctx.Enemies, partyPct);
        }

        private static float AverageHpPct(IReadOnlyList<BattleUnit> units)
        {
            float total = 0; int count = 0;
            foreach (BattleUnit u in units)
            {
                if (u.IsAlive && u.Definition != null) { total += (float)u.CurrentHP / u.Definition.maxHP; count++; }
            }
            return count > 0 ? total / count : 1f;
        }

        private static void ApplyPct(IReadOnlyList<BattleUnit> units, float pct)
        {
            foreach (BattleUnit u in units)
            {
                if (u.IsAlive && u.Definition != null)
                {
                    int newHp = Mathf.Max(1, Mathf.RoundToInt(u.Definition.maxHP * pct));
                    if (newHp < u.CurrentHP) u.TakeDamage(u.CurrentHP - newHp);
                    else u.Heal(newHp - u.CurrentHP);
                }
            }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level2/Smugglers Tunnels")]
    public sealed class BattleEvent_SmugglersTunnels : BattleEventDefinition
    {
        public BattleEvent_SmugglersTunnels()
        {
            restrictions = BattleEventRestriction.FlankedNextAttack;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Hidden trapdoors open! A character can jump through to emerge behind enemy lines — guaranteeing a critical backstab on their next attack.");
        }
    }

    // -------------------------------------------------------------------------
    //  Level 3 – Sprawling Ancient Civilization Events
    // -------------------------------------------------------------------------

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Crumbling Aqueduct")]
    public sealed class BattleEvent_CrumblingAqueduct : BattleEventDefinition
    {
        public BattleEvent_CrumblingAqueduct()
        {
            restrictions = BattleEventRestriction.NoGroundMovement;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The ancient dams fail! A tidal wave pushes everyone to the back rows. All active magical effects are extinguished.");
            foreach (BattleUnit u in ctx.Party) { u.FrontRow = false; StripMagicStatuses(u); }
            foreach (BattleUnit u in ctx.Enemies) { u.FrontRow = false; StripMagicStatuses(u); }
            ctx.Log("All units pushed to back rows. Active magical effects dispelled.");
        }

        private static void StripMagicStatuses(BattleUnit u)
        {
            // Expire all status effects immediately by ticking them past their duration.
            while (u.StatusEffects.Count > 0) u.TickStatuses();
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Judgment of the Forgotten Gods")]
    public sealed class BattleEvent_JudgmentOfTheForgottenGods : BattleEventDefinition
    {
        public override void Apply(BattleEventContext ctx)
        {
            int partyHp = TotalHp(ctx.Party);
            int enemyHp = TotalHp(ctx.Enemies);
            if (partyHp >= enemyHp)
            {
                ctx.Log("The scales judge: the party bears HUBRIS! Healing magic fails for the party this turn.");
                // Expressed via restriction on party faction — BattleController checks this.
                restrictions = BattleEventRestriction.NoHealingMagic;
                ctx.Log("Enemies receive DESPERATION: all skills cost 0 MP this turn.");
            }
            else
            {
                ctx.Log("The scales judge: the party earns DESPERATION! All party skills cost 0 MP this turn.");
                restrictions = BattleEventRestriction.ZeroMpCosts;
                ctx.Log("Enemies receive HUBRIS: healing magic fails for enemies.");
            }
        }

        private static int TotalHp(IReadOnlyList<BattleUnit> units)
        {
            int total = 0;
            foreach (BattleUnit u in units) if (u.IsAlive) total += u.CurrentHP;
            return total;
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Chronos Fracture")]
    public sealed class BattleEvent_ChronosFracture : BattleEventDefinition
    {
        public BattleEvent_ChronosFracture()
        {
            restrictions = BattleEventRestriction.PhantomRepeat;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Time fractures! Phantom clones will repeat the previous turn's actions automatically this turn.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Towering Colossus")]
    public sealed class BattleEvent_ToweringColossus : BattleEventDefinition
    {
        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("A dormant titan steps forward — the arena tilts! Units without Flying/Heavy traits slide to the bottom edge, clumped for AoE.");
            // Pull all non-heavy units to front row (simulating clumping at the tilted edge).
            foreach (BattleUnit u in ctx.Party)
                if (u.IsAlive && u.Defense < 15) { u.FrontRow = true; ctx.Log($"{u.DisplayName} slides to the front edge!"); }
            foreach (BattleUnit u in ctx.Enemies)
                if (u.IsAlive && u.Defense < 15) { u.FrontRow = true; ctx.Log($"{u.DisplayName} slides to the front edge!"); }
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Ancestral Possession")]
    public sealed class BattleEvent_AncestralPossession : BattleEventDefinition
    {
        [Min(1)] public int ancientPowerBonus = 20;
        public int durationDefault = 2;

        public override void Apply(BattleEventContext ctx)
        {
            BattleUnit victim = ctx.RandomLiving(ctx.Party);
            if (victim == null) return;
            ctx.MarkedUnit = victim;
            durationTurns = durationDefault;
            victim.AddStatus(new BattleStatusEffect
            {
                id = "AncientPossession",
                displayName = "Possessed",
                stat = BattleStat.Strength,
                amount = ancientPowerBonus,
                turnsRemaining = durationDefault
            });
            victim.AddStatus(new BattleStatusEffect
            {
                id = "AncientPossessionMag",
                displayName = "Possessed",
                stat = BattleStat.Magic,
                amount = ancientPowerBonus,
                turnsRemaining = durationDefault
            });
            ctx.Log($"An ancient king possesses {victim.DisplayName}! They gain massive power but act on auto-battle for {durationDefault} turns.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Architects Grid")]
    public sealed class BattleEvent_ArchitectsGrid : BattleEventDefinition
    {
        public BattleEvent_ArchitectsGrid()
        {
            restrictions = BattleEventRestriction.KnightMoveBonus;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("The battlefield becomes a glowing chessboard! L-shaped (Knight move) attacks deal 3× damage; straight attacks deal half.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Aetherial Hivemind")]
    public sealed class BattleEvent_AetherialHivemind : BattleEventDefinition
    {
        public BattleEvent_AetherialHivemind()
        {
            restrictions = BattleEventRestriction.SharedActionPoints;
            durationTurns = 2;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Ancient security tethers the party's souls! For 2 turns, Action Points and statuses are shared across the party.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Void Rift")]
    public sealed class BattleEvent_VoidRift : BattleEventDefinition
    {
        public BattleEvent_VoidRift()
        {
            restrictions = BattleEventRestriction.NoRangedAttacks;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("A black hole opens in the arena center! All ranged projectiles curve inward — archers are useless unless adjacent.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Holographic Scramble")]
    public sealed class BattleEvent_HolographicScramble : BattleEventDefinition
    {
        public BattleEvent_HolographicScramble()
        {
            restrictions = BattleEventRestriction.BlindedCombatants;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Illusion defenses activate! Every combatant becomes an identical silhouette for 1 turn. You must attack by memory alone!");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/Events/Level3/Pillars of the Heavens")]
    public sealed class BattleEvent_PillarsOfTheHeavens : BattleEventDefinition
    {
        public BattleEvent_PillarsOfTheHeavens()
        {
            restrictions = BattleEventRestriction.NoRangedAttacks;
        }

        public override void Apply(BattleEventContext ctx)
        {
            ctx.Log("Four massive marble pillars slam into the arena! Ranged attacks are impossible. The battle is now close-quarters.");
        }
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/VFX Definition")]
    public sealed class BattleVfxDefinition : ScriptableObject
    {
        public GameObject castPrefab;
        public GameObject impactPrefab;
        public Vector3 impactOffset;
        [Min(0f)] public float actionDelay = 0.35f;
    }

    [CreateAssetMenu(menuName = "Musketeers/Battle/UI Theme")]
    public sealed class BattleUiTheme : ScriptableObject
    {
        public Sprite fullTurnIcon;
        public Sprite halfTurnIcon;
        public Sprite emptyTurnIcon;
        public Sprite commandCursor;
        public Color partyColor = new Color(0.2f, 0.65f, 1f);
        public Color enemyColor = new Color(1f, 0.2f, 0.35f);
        public Color weaknessColor = new Color(1f, 0.92f, 0.25f);
        public Color resistColor = new Color(0.55f, 0.7f, 1f);
    }
}
