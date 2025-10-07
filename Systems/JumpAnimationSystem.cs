using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    public struct JumpAnimationData : IComponentData
    {
        public float3 StartPosition;
        public float3 TargetPosition;
        public float JumpPower;
        public float Duration;
        public float ElapsedTime;
        public float DelayBeforeStart;
        public bool IsJumping;
    }
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(GravityRepulsorSystem))]
    public partial class JumpAnimationSystem : SystemBase
    {
        // ReSharper disable Unity.BurstLoadingManagedType
        
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity entity, ref JumpAnimationData jumpData, in GameObject gameObject) =>
                {
                    if (jumpData.DelayBeforeStart > 0)
                    {
                        jumpData.DelayBeforeStart -= deltaTime;
                        if (jumpData.DelayBeforeStart <= 0)
                        {
                            jumpData.IsJumping = true;
                            jumpData.DelayBeforeStart = 0;
                        }
                        return;
                    }
                    
                    if (!jumpData.IsJumping)
                        return;
                    
                    jumpData.ElapsedTime += deltaTime;
                    var normalizedTime = jumpData.ElapsedTime / jumpData.Duration;
                    
                    if (normalizedTime >= 1.0f)
                    {
                        gameObject.transform.position = new Vector3(
                            jumpData.TargetPosition.x,
                            jumpData.TargetPosition.y,
                            jumpData.TargetPosition.z);
                        
                        jumpData.IsJumping = false;
                        EntityManager.AddComponent<JumpCleanupSystem.JumpCompletedTag>(entity);
                    }
                    else
                    {
                        // Горизонтальное движение
                        var currentPos = math.lerp(jumpData.StartPosition, jumpData.TargetPosition, normalizedTime);

                        // Вертикальное движение: базовая высота + парабола прыжка
                        var baseHeight = math.lerp(jumpData.StartPosition.y, jumpData.TargetPosition.y, normalizedTime);
                        var jumpHeight = jumpData.JumpPower * 4f * normalizedTime * (1f - normalizedTime);
                        currentPos.y = baseHeight + jumpHeight;

                        gameObject.transform.position = new Vector3(currentPos.x, currentPos.y, currentPos.z);
                    }
                }).Run();
        }
    }
}