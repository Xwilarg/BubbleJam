using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace BubbleJam.Bullet
{
    [BurstCompile]
    public struct BulletJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<BulletInfo> Bullets;
        public float DeltaTime;


        const float Frequency = 15f;
        const float Amplitude = 20f;

        public void Execute(int index)
        {
            var elem = Bullets[index];

            float sin = math.sin(elem.Angle);
            float cos = math.cos(elem.Angle);

            elem.Position = new float2(
                x: elem.Position.x + cos * elem.Speed * DeltaTime,
                y: elem.Position.y + sin * elem.Speed * DeltaTime
            );

            if (elem.Shape == AttackShape.Sin)
            {
                var wave = math.sin(elem.Lifetime * Frequency) * Amplitude;
                elem.Position.x += -sin * wave * DeltaTime;
                elem.Position.y += cos * wave * DeltaTime;
            }
            else if (elem.Shape == AttackShape.Cos)
            {
                var wave = math.sin(elem.Lifetime * Frequency) * Amplitude;
                elem.Position.x += sin * wave * DeltaTime;
                elem.Position.y += -cos * wave * DeltaTime;
            }

            elem.Lifetime -= DeltaTime;

            Bullets[index] = elem;
        }
    }
}