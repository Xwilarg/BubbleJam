using Ink.UnityIntegration;
using Sketch.VN;
using Sketch.VN.InkleInk;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleJam.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { private set; get; }

        private static bool _skipIntro;

        [SerializeField]
        private InkFile _intro, _badEnding, _goodEnding;

        [SerializeField]
        private bool _debug_skipStory;

        [SerializeField]
        private GameObject _gameUI;

        [SerializeField]
        private GameObject _youAreHereHint;

        private void Awake()
        {
            Instance = this;

            _gameUI.SetActive(false);
            _youAreHereHint.SetActive(false);
        }

        private bool OnTags(string name, string content)
        {
            if (name == "end")
            {
                SceneManager.LoadScene("Menu");
                return true;
            }
            if (name == "hint")
            {
                _youAreHereHint.SetActive(content == "show");
                return true;
            }
            if (name == "retry")
            {
                _skipIntro = content == "skip";
                SceneManager.LoadScene("Main");
                return true;
            }

            return false;
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (_debug_skipStory)
            {
                _gameUI.SetActive(true);
                return;
            }
#endif
            if (_skipIntro)
            {
                _gameUI.SetActive(true);
                return;
            }

            VNManager.Instance.ShowStory(new InkStory(_intro), onDone: () => { _gameUI.SetActive(true); }, onTags: OnTags);
        }

        public void PlayBadEnding()
        {
            VNManager.Instance.ShowStory(new InkStory(_badEnding), onTags: OnTags);
        }

        public void PlayGoodEnding()
        {
            VNManager.Instance.ShowStory(new InkStory(_goodEnding), onTags: OnTags);
        }
    }
}
