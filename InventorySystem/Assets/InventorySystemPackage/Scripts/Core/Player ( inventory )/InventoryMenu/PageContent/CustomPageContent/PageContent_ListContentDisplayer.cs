using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// YOU CAN'T ADD NEW DISPLAYED ITEM WITHOUT RELOADING PAGE
namespace InventorySystem.PageContent
{
    [AddComponentMenu(CreateAssetMenuPaths.listContentDisplayer)]
    public class PageContent_ListContentDisplayer : InventoryPageContent
    {
        [SerializeField] private ListContentDisplayer displayer;

        public Transform ContentParent => displayer.contentParent;
        public bool InteractOnScrollwheel => displayer.interactOnScrollwheel;

        public override void SetUpContent() { displayer.SetUp(); }

        public void SetDisplayedContent_(List<GameObject> objs) => displayer.SetDisplayedContent_(objs);

        public void InvokeScroll(bool up) => displayer.InvokeScroll(up);
    }
}
