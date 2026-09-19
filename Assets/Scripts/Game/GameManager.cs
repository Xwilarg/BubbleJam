using UnityEngine;

namespace BubbleJam.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
