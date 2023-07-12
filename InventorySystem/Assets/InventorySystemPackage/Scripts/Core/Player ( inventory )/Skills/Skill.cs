using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Skills_
{
    [CreateAssetMenu(menuName = CreateAssetMenuPaths.skill)]
    public class Skill : ScriptableObject
    {
        public string skillName = "newSkill";
        public float firstLevelReqXps = 1000;
        public float nextLevelMultiplayer = 1.2f;

        [Tooltip("0 MEANS UNLIMITED")]
        public int maxLevel = 20;
    }
}
