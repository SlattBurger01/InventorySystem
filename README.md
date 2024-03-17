**Multiplayer Ready Inventory System**
- Using Unity & Photon

***Inventory***
- Minecraft-like style with easily customizable page count, items interactions and so on
- Each item can have custom ```stack count```, appearance, can be used as equitable (armor and stuff) and can have custom variables such as ```Damage```, effects (on consume, hold, ...), and so on

***Inventory Menu***
- Each menu page can have custom ```open Key``` and ```close Key```, even open on interaction (such as clicking on some GameObject in scene)
- Custom page content can be created simply by creating new class that inherits from ```InventoryPageContent.cs```

***Building system***
- Satisfactory-like building system (objects are built in grid based on adjanced objects)

***Collectible Items***
- If collection is completed, some stats can be improved, etc.
- Can be synchronized with other players

***Combat system***
- Damage can be customized separately on each item
- effects can be added on hit (on target player)
- player can be respawned after his death, items can be dropped

***Crafting system***
- Crafting recipes can be locked under skill, collection, etc.
- Properties such as ```crafting time``` can be adjusted based on skills, collection, etc.

***Effects***
- Effects can be added on any interaction (on item used, on GameObject clicked, etc.)

***Storages***
- Storages (such as chests, furnaces, etc.) can be opened by multiple players at the same time 
  
***Shop***
- Shop menu can be directly in inventory menu or in shop building (IInteractable object) can be used

***Skills***
  - Skills can be increased by any interaction you can imagine
  - Each skill has custom amount of xp required to level up and required xp multiplayer on level up 

***Save and load system***
  - Handles saving and loading: on scene loaded, on player joined, on player disconnected, on scene unloaded

Basic documentation: https://github.com/SlattBurger01/InventorySystem/blob/main/InventorySystem/Assets/InventorySystemPackage/Documentation/Multiplayer%20ready%20inventory%20%26%20build%20system%20Documentation.pdf
