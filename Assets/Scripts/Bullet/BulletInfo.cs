using Unity.Mathematics;

namespace BubbleJam.Bullet
{
    public struct BulletInfo
    {
        public float2 Position;
        public float Angle;
        public float Speed;
        public float Lifetime;

        public AttackShape Shape;
    }

    public struct DamageInfo
    { }
}
