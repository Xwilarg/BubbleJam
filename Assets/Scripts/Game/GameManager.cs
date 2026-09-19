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

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            VNManager.Instance.ShowStory(new InkStory(_intro));
        }
    }
}
