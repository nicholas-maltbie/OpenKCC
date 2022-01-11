using UnityEngine;

namespace nickmaltbie.OpenKCC.UI
{
    /// <summary>
    /// Simple class to abstract commands to change UI for a menu screen
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        /// <summary>
        /// Can this menu controller allow for input to change screens
        /// </summary>
        public bool allowInputChanges = true;

        /// <summary>
        /// Short action delay (how fast can the screen change)
        /// </summary>
        public float actionDelay = 0.01f;

        /// <summary>
        /// Time of previous screen change
        /// </summary>
        private float previousTime;

        /// <summary>
        /// Can the screne be changed right now?
        /// </summary>
        private bool CanAct => allowInputChanges && (Time.time - previousTime > actionDelay);

        /// <summary>
        /// Requests go to the previous screen
        /// </summary>
        public void PreviousScreen()
        {
            if (!CanAct)
            {
                return;
            }

            previousTime = Time.time;
            UIManager.PreviousScreen(this);
        }

        /// <summary>
        /// Request a new screen using a prefab name
        /// </summary>
        /// <param name="screenPrefab">Screen prefab to switch to</param>
        public void SetScreen(GameObject screenPrefab)
        {
            this.SetScreen(screenPrefab.name);
        }

        /// <summary>
        /// Request a new screen directly through a name
        /// </summary>
        /// <param name="name">Name of new screen to display</param>
        public void SetScreen(string name)
        {
            if (!CanAct)
            {
                return;
            }

            previousTime = Time.time;
            UIManager.RequestNewScreen(this, name);
        }
    }
}
