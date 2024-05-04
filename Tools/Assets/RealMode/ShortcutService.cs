using UnityEngine;

namespace RealMode
{
    public class ShortcutService : MonoBehaviour
    {
        public static bool CanUseShortcuts { get; private set; }

        private void Awake()
        {
            CanUseShortcuts = true;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
                {
                    CanUseShortcuts = !CanUseShortcuts;
                }
            }
        }
    }
}