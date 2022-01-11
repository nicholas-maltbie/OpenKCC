using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Change the fullscreen mode of the game.
    /// </summary>
    public class ToggleFullScreenAction : Button
    {
        public string whenFullScreen = "Minimize";
        public string whenWindowed = "Maximize";

        private Button button;
        private UnityEngine.UI.Text buttonText;

        protected override void Awake()
        {
            base.Awake();
            this.button = GetComponent<Button>();
            this.buttonText = this.button.GetComponentInChildren<UnityEngine.UI.Text>();
        }

        public void Update()
        {
            buttonText.text = Screen.fullScreen ? whenFullScreen : whenWindowed;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}
