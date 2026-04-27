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
