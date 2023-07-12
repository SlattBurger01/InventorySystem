using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.PageContent
{
    // IS NOT DIRECTLY IN PAGECONTENT, BECAUSE SOME OF THEM ARE NOT MEANT TO BE USED FOR HOTBAR
    // CHECKED IN 'Inventory.cs'
    public interface IUseableForHotbar
    {
        public bool useForHotbar { get; set; }
    }
}
