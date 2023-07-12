using InventorySystem.PageContent;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Prefabs;

namespace InventorySystem.Skills_ // USED IN PLAYERDATA AND SAVEANDLOADSYSTEM AND SKILLSMENU, InventoryEventSystem, InventoryPrefabsSpawner
{
    public class Skills : MonoBehaviour
    {
        [SerializeField] private Skill[] skills;

        [SerializeField] private GameObject skillPrefab;

        public SkillInHandler[] skillsInHandler;

        private InventoryCore core;

        private void Awake()
        {
            core = GetComponent<InventoryCore>();

            skillsInHandler = new SkillInHandler[skills.Length];
            for (int i = 0; i < skills.Length; i++) skillsInHandler[i] = new SkillInHandler(skills[i]);
        }

        public void AddXps(string skillName, int xpAmount) => AddXps(GetSkill(skillName), xpAmount);

        private void AddXps(SkillInHandler skill, int amount)
        {
            if (skill.onMaxLevel) return;

            skill.currentXps += amount;
            CheckForNewLevel(skill);
            core.inventoryEventSystem.InventoryMenu_UpdateOpenedPage();

            if (skill.onMaxLevel) skill.currentXps = 0;
        }

        private void CheckForNewLevel(SkillInHandler SkillInHandler)
        {
            while (SkillInHandler.currentXps >= GetRequiedXps(GetSkillId(SkillInHandler.skill.skillName)))
            {
                SkillInHandler.currentXps -= GetRequiedXps(GetSkillId(SkillInHandler.skill.skillName));
                SkillInHandler.currentLevel++;
            }
        }

        public int GetLevel(string skillName) => GetSkill(skillName).currentLevel;

        private int GetSkillId(string skillName)
        {
            int targetSkill = -1;

            for (int i = 0; i < skillsInHandler.Length; i++)
            {
                if (skillsInHandler[i].skill.skillName == skillName) { targetSkill = i; break; }
            }

            if (targetSkill == -1) Debug.LogError($"SkillInHandler NOT FOUND ({skillName})");

            return targetSkill;
        }

        private SkillInHandler GetSkill(string skillName) { return skillsInHandler[GetSkillId(skillName)]; }

        public void DisplaySkills(PageContent_ListContentDisplayer scrollableContentDisplayer)
        {
            List<GameObject> spawnedObjs = new List<GameObject>();

            for (int i = 0; i < skillsInHandler.Length; i++)
            {
                GameObject newSkill = InventoryPrefabsSpawner.spawner.SpawnSkillPrefab(skillPrefab, scrollableContentDisplayer.ContentParent, skillsInHandler[i], GetRequiedXps(i), skillsInHandler[i].onMaxLevel);

                spawnedObjs.Add(newSkill);
            }

            scrollableContentDisplayer.SetDisplayedContent_(spawnedObjs);
        }

        /// <summary> HOW MUCH XP IS NECESSARY FOR NEXT LEVEL </summary> 
        private int GetRequiedXps(int skillId)
        {
            float fXps = skillsInHandler[skillId].skill.firstLevelReqXps * skillsInHandler[skillId].skill.nextLevelMultiplayer * (skillsInHandler[skillId].currentLevel + 1);
            return Mathf.RoundToInt(fXps);
        }

        public void SetLevelAndXps(string skillName, int level, int xps) // SAVE AND LOAD SYSTEM
        {
            SkillInHandler SkillInHandler = GetSkill(skillName);

            SkillInHandler.currentLevel = level;
            SkillInHandler.currentXps = xps;
        }
    }
}
