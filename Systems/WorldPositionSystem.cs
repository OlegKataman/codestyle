using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [DisableAutoCreation]
    public partial class WorldPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithoutBurst()
                .WithAll<WorldPositionComponent, TargetTag>() // target tag hm.
                .WithNone<DestroyTag>()
                .ForEach((ref WorldPositionComponent worldPositionComponent, in Transform transform) =>
                {
                    
                    worldPositionComponent.Position = (float3)transform.position;

                }).Run();
        }
    }
}