using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace InventorySystem.PageContent
{
    // DON'T FORGE TO USE "InventoryContent_PageContent" NAMESPACE TO USE THIS CLASS
    public abstract class InventoryPageContent : MonoBehaviour
    {
        protected InventoryPage parentPage;

        private void Awake() { GetComponentsInternal(); GetComponents(); }

        private void GetComponentsInternal() { parentPage = GetComponentInParent<InventoryPage>(); }

        // VIRTUAL VOIDS
        protected virtual void GetComponents() { }

        public virtual void SetUpContent() { }

        /// <summary> CALLED BEFORE PAGE IS UPDATED </summary>
        /// <param name="viaButton"></param>
        public virtual void BeforePageUpdated(bool viaButton) { }

        /// <param name="viaButton"> IF PAGE WAS UPDATED BY PRESSING INVENTORY PAGE BUTTON </param>
        public virtual void UpdateContent(bool viaButton) { }

        /// <summary> CALLED AFTER PAGE IS UPDATED </summary>
        public virtual void OnParentPageOpened() { }
    }
}
