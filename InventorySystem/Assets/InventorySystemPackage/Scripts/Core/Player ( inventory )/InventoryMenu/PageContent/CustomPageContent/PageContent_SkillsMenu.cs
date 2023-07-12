using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Skills_;

namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.skillsMenu)]
    public class PageContent_SkillsMenu : InventoryPageContent
    {
        [SerializeField] private PageContent_ListContentDisplayer scrollableContent;

        private Skills skills;

        protected override void GetComponents() { skills = GetComponentInParent<Skills>(); }

        public override void UpdateContent(bool viaButton) { skills.DisplaySkills(scrollableContent); }
    }
}
