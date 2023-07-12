namespace InventorySystem.Skills_
{
    public class SkillInHandler
    {
        public int currentXps;
        public int currentLevel;
        public Skill skill;

        public SkillInHandler(Skill skill_) { skill = skill_; }

        // ---
        public bool onMaxLevel { get { return currentLevel >= skill.maxLevel; } }
    }
}
