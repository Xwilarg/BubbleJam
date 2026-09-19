using Ink.UnityIntegration;
using Sketch.VN;
using Sketch.VN.InkleInk;
using UnityEngine;

namespace BubbleJam.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { private set; get; }

        [SerializeField]
        private InkFile _intro;

        [SerializeField]
        private bool _debug_skipStory;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (_debug_skipStory) return;
#endif
            VNManager.Instance.ShowStory(new InkStory(_intro));
        }
    }
}
