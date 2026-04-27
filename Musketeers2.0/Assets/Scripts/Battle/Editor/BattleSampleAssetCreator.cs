#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Musketeers.Battle.Editor
{
    public static class BattleSampleAssetCreator
    {
        private const string SamplePath = "Assets/BattleSamples";

        [MenuItem("Musketeers/Battle/Create Starter Battle Assets")]
        public static void CreateStarterAssets()
        {
            Directory.CreateDirectory(SamplePath);

            BattleSkillDefinition attack = CreateAsset<BattleSkillDefinition>("Basic Slash");
            attack.displayName = "Slash";
            attack.kind = BattleSkillKind.Damage;
            attack.element = BattleElement.Slash;
            attack.targeting = BattleTargeting.SingleEnemy;
            attack.power = 12;
            attack.mpCost = 0;

            BattleSkillDefinition fire = CreateAsset<BattleSkillDefinition>("Fire Shot");
            fire.displayName = "Fire Shot";
            fire.kind = BattleSkillKind.Damage;
            fire.element = BattleElement.Fire;
            fire.targeting = BattleTargeting.SingleEnemy;
            fire.power = 18;
            fire.mpCost = 4;

            BattleSkillDefinition heal = CreateAsset<BattleSkillDefinition>("First Aid");
            heal.displayName = "First Aid";
            heal.kind = BattleSkillKind.Heal;
            heal.element = BattleElement.Heal;
            heal.targeting = BattleTargeting.SingleAlly;
            heal.power = 18;
            heal.mpCost = 5;

            BattleUnitDefinition musketeer = CreateAsset<BattleUnitDefinition>("Sample Musketeer");
            musketeer.displayName = "Musketeer";
            musketeer.defaultFaction = BattleFaction.Party;
            musketeer.playerControlled = true;
            musketeer.maxHP = 120;
            musketeer.maxMP = 28;
            musketeer.strength = 14;
            musketeer.magic = 9;
            musketeer.defense = 8;
            musketeer.agility = 10;
            musketeer.basicAttack = attack;
            musketeer.skills = new List<BattleSkillDefinition> { fire, heal };

            BattleUnitDefinition mageCaptain = CreateAsset<BattleUnitDefinition>("Sample Mage Captain");
            mageCaptain.displayName = "Mage Captain";
            mageCaptain.defaultFaction = BattleFaction.Enemy;
            mageCaptain.playerControlled = false;
            mageCaptain.maxHP = 180;
            mageCaptain.maxMP = 40;
            mageCaptain.strength = 10;
            mageCaptain.magic = 16;
            mageCaptain.defense = 7;
            mageCaptain.agility = 8;
            mageCaptain.basicAttack = attack;
            mageCaptain.skills = new List<BattleSkillDefinition> { fire };
            mageCaptain.affinities = new List<BattleAffinityEntry>
            {
                new BattleAffinityEntry { element = BattleElement.Fire, response = BattleAffinityResponse.Resist },
                new BattleAffinityEntry { element = BattleElement.Pierce, response = BattleAffinityResponse.Weak }
            };

            BattleEncounterDefinition encounter = CreateAsset<BattleEncounterDefinition>("Starter Encounter");
            encounter.encounterName = "Starter Encounter";
            encounter.party = new List<BattleFormationSlot>
            {
                new BattleFormationSlot { unit = musketeer, scenePosition = new Vector3(-2f, 0f, 0f), frontRow = true }
            };
            encounter.enemies = new List<BattleFormationSlot>
            {
                new BattleFormationSlot { unit = mageCaptain, scenePosition = new Vector3(2f, 0f, 0f), frontRow = true }
            };

            EditorUtility.SetDirty(attack);
            EditorUtility.SetDirty(fire);
            EditorUtility.SetDirty(heal);
            EditorUtility.SetDirty(musketeer);
            EditorUtility.SetDirty(mageCaptain);
            EditorUtility.SetDirty(encounter);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = encounter;
        }

        private static T CreateAsset<T>(string assetName) where T : ScriptableObject
        {
            string path = AssetDatabase.GenerateUniqueAssetPath($"{SamplePath}/{assetName}.asset");
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
