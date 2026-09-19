using BubbleJam.Bullet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BubbleJam.Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _mouseMarker;

        private float _timer;
        private float _angle;

        private Camera _cam;

        public void Play()
        {
            SceneManager.LoadScene("Main");
        }

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            var mouse = Mouse.current.position.ReadValue();
            var mouseWorldPos = _cam.ScreenToWorldPoint(mouse);
            _mouseMarker.position = mouseWorldPos;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _angle = Mathf.MoveTowardsAngle(_angle, _angle + Mathf.PI / 10f, Mathf.PI / 10f);
                BulletManager.Instance.Spawn(Vector2.zero, _angle, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(Vector2.zero, _angle + Mathf.PI, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(Vector2.zero, _angle - Mathf.PI / 2f, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(Vector2.zero, _angle + Mathf.PI / 2f, 10f, 10f, AttackShape.Straight);
                _timer = .05f;
            }
        }
    }
}
