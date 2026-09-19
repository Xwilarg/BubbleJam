using System.Collections;
using UnityEngine;

namespace BubbleJam
{
    public class Skill
    {
        public Skill(float refTimer, MonoBehaviour parent)
        {
            _wait = new WaitForSeconds(refTimer);
            _parent = parent;
        }

        public bool CanUse { private set; get; } = true;
        private MonoBehaviour _parent;

        private WaitForSeconds _wait;

        public void Use()
        {
            CanUse = false;
            _parent.StartCoroutine(WaitCoroutine());
        }

        private IEnumerator WaitCoroutine()
        {
            yield return _wait;
            CanUse = true;
        }
    }
}
