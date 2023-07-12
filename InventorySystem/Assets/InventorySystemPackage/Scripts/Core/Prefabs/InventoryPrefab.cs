using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InventorySystem.Prefabs
{
    public class InventoryPrefab : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] texts;
        [SerializeField] private RawImage[] rawImages;
        [SerializeField] private Slider[] sliders;
        [SerializeField] private Button[] buttons;

        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private TextMeshProUGUI lockedOverlayText;

        [SerializeField] private RectTransform[] adjustableScales;

        public void UpdateText(int textId, string content) => UpdateText(textId, content, texts[textId].color);

        public void UpdateText(int textId, string content, Color color)
        {
            TextMeshProUGUI targetText = texts[textId];

            targetText.text = content;
            targetText.color = color;
        }

        public void UpdateTextSpriteAsset(int textId, TMP_SpriteAsset spriteAsset) { texts[textId].spriteAsset = spriteAsset; }

        public void UpdateOpacity(float opacity)
        {
            foreach (RawImage image in GetComponentsInChildren<RawImage>(true))
            {
                Color c = image.color;
                image.color = new Color(c.r, c.g, c.b, opacity);
            }

            foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Color c = text.color;
                text.color = new Color(c.r, c.g, c.b, opacity);
            }
        }

        public void UpdateRawImage(int imageId, Texture2D texture) { rawImages[imageId].texture = texture; }

        public void UpdateRawImage(int imageId, Color color) { rawImages[imageId].color = color; }

        public void UpdateSlider(int sliderId, float maxValue, float value)
        {
            sliders[sliderId].maxValue = maxValue;
            sliders[sliderId].value = value;
        }

        public void UpdateLockedOverlay(bool enable, string textContent)
        {
            lockedOverlay.SetActive(enable);
            lockedOverlayText.text = textContent;
        }

        public void SetActiveText(int id, bool enable) => SetObjActive<TextMeshProUGUI>(texts, id, enable);
        public void SetActiveImage(int id, bool enable) => SetObjActive<RawImage>(rawImages, id, enable);
        public void SetActiveSlider(int id, bool enable) => SetObjActive<Slider>(sliders, id, enable);

        private void SetObjActive<T>(T[] a, int id, bool e) where T: MonoBehaviour
        {
            a[id].gameObject.SetActive(e);
        }

        public TextMeshProUGUI GetText(int textId) { return texts[textId]; }

        public void UpdateAdjustableScale(int id, Vector2 newScale) { adjustableScales[id].sizeDelta = newScale; }

        // BUTTONS
        public void RemoveAllListeners(int buttonId) { buttons[buttonId].onClick.RemoveAllListeners(); }
        public void AddListener(int buttonId, UnityAction action) { buttons[buttonId].onClick.AddListener(action); }

        /// <summary> ONE OF THESE WILL BE UPDATED AND SET ACTIVE ( IMAGE HAS PRIORITY ) </summary>
        public void UpdateImageOrText(int imageId, int textId, string text, Texture2D texture)
        {
            this.SetActiveText(textId, !texture);
            this.SetActiveImage(imageId, texture);

            if (texture) this.UpdateRawImage(imageId, texture);
            else this.UpdateText(textId, text);
        }
    }
}