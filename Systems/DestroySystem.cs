using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(DeathSystem))]
    public partial class DestroySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<DestroyTag>()
                .DestroyEntity();
        }
    }
}