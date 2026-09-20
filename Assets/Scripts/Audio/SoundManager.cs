using UnityEngine;

namespace BubbleJam.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { private set; get; }

        [SerializeField]
        private AudioSource _source;

        [SerializeField]
        private AudioClip _shootSfx, _slashSfx;

        private void Awake()
        {
            Instance = this;
        }

        public void PlayShoot()
        {
            _source.PlayOneShot(_shootSfx, .25f);
        }

        public void PlaySlash()
        {
            _source.PlayOneShot(_slashSfx, .5f);
        }
    }
}
