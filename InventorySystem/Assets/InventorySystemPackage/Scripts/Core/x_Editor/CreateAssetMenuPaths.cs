namespace InventorySystem
{
    public static class CreateAssetMenuPaths
    {
        private const string rootFolder = "InventorySystem/";

        private const string databases = "Databases/";

        private const string essentials = "Essentials/";

        // ---- CREATE ASSET MENU PATHS
        public const string itemsDatabase = rootFolder + databases  + "ItemDatabase";
        public const string effectsDatabase = rootFolder + databases + "EffectDatabase";
        public const string buildingsDatabase = rootFolder + databases + "BuildingsDatabase";

        public const string itemUseOption_Weapon = rootFolder + "ItemUse/Weapon";

        public const string skill = rootFolder + "Skill";
        public const string collectibleItem = rootFolder + "CollectibleItem";
        public const string craftingRecipe = rootFolder + "CraftingRecipe";
        public const string buildingRecipe = rootFolder + "BuildingRecipe";
        public const string item = rootFolder + "Item";

        public const string effect_poison = rootFolder + "Effects/PoisonEffect";

        public const string equipPosition = rootFolder + "EquipPosition";
        public const string equipPositionsHandler = rootFolder + essentials + "EquipPositionsHandler";

        public const string itemRarity = rootFolder + "itemRarity";
        public const string itemRaritiesHandler = rootFolder + essentials + "itemRaritiesHandler";

        public const string currenciesHandler = rootFolder + essentials + "currenciesHandler";
        public const string currency = rootFolder + "currency";

        public const string categoriesHandler = rootFolder + essentials + "categoriesHandler";
        public const string itemCategory = rootFolder + "itemCategory";

        // ---- ADD COMPONENT MENU
        private const string interactibleFolder = "Interactibles/";

        public const string pickupableItem = rootFolder + interactibleFolder + "PickupableItem";
        public const string collectibleItemAdd = rootFolder + interactibleFolder + "CollectibleItemHolder";

        public const string storage = rootFolder + interactibleFolder + "Storage";
        public const string crafting = rootFolder + interactibleFolder + "CraftingActionBuilding";
        public const string shop = rootFolder + interactibleFolder + "ShopActionBuilding";
        public const string building = rootFolder + interactibleFolder + "BuildingActionBuilding";

        // PAGE CONTENT
        private const string customPageContent = "CustomPageContent/";

        public const string buildingPageContent = rootFolder + customPageContent + "BuildingPageContent";
        public const string buildingRecipeDisplayer = rootFolder + customPageContent + "BuildingRecipeDisplayer";
        public const string categorySelector = rootFolder + customPageContent + "CategorySelector";
        public const string collectibleItemsDisplayer = rootFolder + customPageContent + "CollectibleItemsDisplayer";
        public const string craftingMenu = rootFolder + customPageContent + "CraftingMenu";
        public const string currenciesDisplayer = rootFolder + customPageContent + "CurrenciesDisplayer";
        public const string itemDisplayer = rootFolder + customPageContent + "ItemDisplayer";
        public const string listContentDisplayer = rootFolder + customPageContent + "Page_ListContentDisplayer";
        public const string shopMenu = rootFolder + customPageContent + "ShopMenu";
        public const string skillsMenu = rootFolder + customPageContent + "SkillsMenu";
        public const string slotsDisplayer = rootFolder + customPageContent + "SlotsDisplayer";
        public const string totalInventoryDisplayer = rootFolder + customPageContent + "TotalInventoryDisplayer";
    }
}
