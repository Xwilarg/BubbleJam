using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BubbleJam.Bullet
{
    public class BulletManager : MonoBehaviour
    {
        public static BulletManager Instance { private set; get; }

        [SerializeField]
        private GameObject _bulletPrefab;

        private NativeList<BulletInfo> _data;
        private readonly List<GameObject> _instances = new();
        private readonly List<DamageInfo> _collData = new();

        private GameObject _bulletContainer;

        private int _enemyLayer;

        private void Awake()
        {
            Instance = this;

            _data = new NativeList<BulletInfo>(Allocator.Persistent);
            _bulletContainer = new GameObject("Bullets");

            _enemyLayer = LayerMask.GetMask("Enemy", "Attack");
        }

        public void Spawn(Vector2 pos, float angle, float speed, float lifetime, AttackShape shape)
        {
            _data.Add(new()
            {
                Angle = angle,
                Position = pos,
                Lifetime = lifetime,
                Speed = speed,
                Shape = shape
            });
            _collData.Add(new() {});
            var go = Instantiate(_bulletPrefab, pos, Quaternion.identity);
            _instances.Add(go);
            go.transform.SetParent(_bulletContainer.transform);
        }

        private void Update()
        {
            if (_data.Count == 0) return;

            var job = new BulletJob()
            {
                Bullets = _data.AsArray(),
                DeltaTime = Time.deltaTime
            };

            JobHandle handle = job.Schedule(_data.Count, 64);
            handle.Complete();

            for (int i = _data.Count - 1; i >= 0; i--)
            {
                var data = _data[i];
                var collData = _collData[i];

                var newPos = new Vector2(data.Position.x, data.Position.y);

                var pendingDestruction = data.Lifetime <= 0f;

                var coll = Physics2D.OverlapCircle(newPos, _instances[i].transform.localScale.x / 2f, _enemyLayer);
                if (coll != null)
                {
                    Destroy(_instances[i]);
                    _data.RemoveAt(i);
                    _instances.RemoveAt(i);
                    _collData.RemoveAt(i);
                }
                else
                {
                    _instances[i].transform.position = newPos;
                }
            }
        }
        private void OnDrawGizmos()
        {
            if (!_data.IsCreated) return;

            Gizmos.color = Color.red;
            for (int i = _data.Count - 1; i >= 0; i--)
            {
                var data = _data[i];
                var collData = _collData[i];

                var newPos = new Vector2(data.Position.x, data.Position.y);
                Gizmos.DrawWireSphere(newPos, _instances[i].transform.localScale.x / 2f);
            }
        }
    }
}
