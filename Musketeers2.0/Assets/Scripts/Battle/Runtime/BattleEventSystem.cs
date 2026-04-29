using System;
using System.Collections.Generic;
using UnityEngine;

namespace Musketeers.Battle
{
    /// <summary>
    /// Manages the lifecycle of randomized combat events inside a battle.
    /// Owned by <see cref="BattleController"/> and ticked each faction turn.
    /// </summary>
    public sealed class BattleEventSystem
    {
        private readonly List<BattleEventDefinition> pool;
        private readonly float chance;
        private BattleEventDefinition activeEvent;
        private int turnsRemaining;
        private BattleEventContext lastContext;

        public event Action<BattleEventDefinition, BattleEventContext> EventTriggered;
        public event Action<BattleEventDefinition, BattleEventContext> EventExpired;
        public event Action<BattleEventContext> TurnEnded;

        public BattleEventDefinition ActiveEvent => activeEvent;
        public bool HasActiveEvent => activeEvent != null;

        /// <summary>
        /// Aggregate restrictions from the currently active event (empty flags if none).
        /// </summary>
        public BattleEventRestriction CurrentRestrictions =>
            activeEvent != null ? activeEvent.restrictions : BattleEventRestriction.None;

        public BattleEventSystem(List<BattleEventDefinition> eventPool, float eventChance)
        {
            pool = eventPool ?? new List<BattleEventDefinition>();
            chance = Mathf.Clamp01(eventChance);
        }

        /// <summary>
        /// Called at the start of each faction turn.
        /// May roll a new event if none is active and the pool is non-empty.
        /// </summary>
        public BattleEventContext OnTurnStart(
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);

            if (!HasActiveEvent && pool.Count > 0 && UnityEngine.Random.value <= chance)
            {
                RollEvent(ctx);
            }

            return ctx;
        }

        /// <summary>
        /// Called at the end of each faction turn to tick the active event.
        /// </summary>
        public void OnTurnEnd(
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            if (!HasActiveEvent) return;

            BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);
            lastContext = ctx;

            activeEvent.OnTurnEnd(ctx);
            TurnEnded?.Invoke(ctx);

            turnsRemaining--;
            if (turnsRemaining <= 0)
            {
                activeEvent.OnExpire(ctx);
                EventExpired?.Invoke(activeEvent, ctx);
                activeEvent = null;
            }
        }

        /// <summary>
        /// Notifies the event system that a unit died (for events like Cursed Miasma).
        /// </summary>
        public void OnUnitDied(BattleUnit deceased,
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            if (activeEvent is BattleEvent_CursedMiasma miasma)
            {
                BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);
                miasma.OnUnitDied(deceased, ctx);
                TurnEnded?.Invoke(ctx);
            }
        }

        /// <summary>
        /// Notifies the event system that an action was taken (for events like Unstable Relic).
        /// </summary>
        public void OnActionTaken(
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            if (activeEvent is BattleEvent_UnstableRelic relic)
            {
                BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);
                bool detonated = relic.TickAction(ctx);
                TurnEnded?.Invoke(ctx);
                if (detonated)
                {
                    activeEvent = null;
                }
            }
        }

        /// <summary>
        /// Notifies the event system that a Fire spell was just cast (for Volatile Gas Pocket).
        /// </summary>
        public void OnFireSpellCast(
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            if (activeEvent is BattleEvent_VolatileGasPocket gas)
            {
                BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);
                gas.Ignite(ctx);
                TurnEnded?.Invoke(ctx);
                // Gas is consumed after ignition.
                activeEvent = null;
            }
        }

        /// <summary>
        /// Notifies the event system that a critical hit or miss occurred (for Spectral Audience).
        /// </summary>
        public void OnCriticalHit(BattleUnit beneficiary,
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            if (activeEvent is BattleEvent_SpectralAudience audience)
            {
                BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);
                audience.OnCritical(beneficiary, ctx);
                TurnEnded?.Invoke(ctx);
            }
        }

        public void OnMissedAttack(BattleUnit punished,
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            if (activeEvent is BattleEvent_SpectralAudience audience)
            {
                BattleEventContext ctx = BuildContext(party, enemies, turnIcons, activeFaction);
                audience.OnMiss(punished, ctx);
                TurnEnded?.Invoke(ctx);
            }
        }

        // ------------------------------------------------------------------

        private void RollEvent(BattleEventContext ctx)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            BattleEventDefinition picked = pool[idx];
            if (picked == null) return;

            activeEvent = picked;
            turnsRemaining = Mathf.Max(1, picked.durationTurns + 1); // +1: includes current turn
            lastContext = ctx;

            picked.Apply(ctx);
            EventTriggered?.Invoke(picked, ctx);
        }

        private static BattleEventContext BuildContext(
            IReadOnlyList<BattleUnit> party,
            IReadOnlyList<BattleUnit> enemies,
            BattleTurnIconPool turnIcons,
            BattleFaction activeFaction)
        {
            return new BattleEventContext
            {
                Party = party,
                Enemies = enemies,
                TurnIcons = turnIcons,
                ActiveFaction = activeFaction
            };
        }
    }
}
