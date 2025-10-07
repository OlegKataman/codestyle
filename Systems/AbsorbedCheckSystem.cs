using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateAfter(typeof(DeathSystem))]
    public partial class AbsorbedCheckSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<AbsorbedTag, Disabled>()
                .WithNone<EjectingComponent>()
                //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .ForEach((Entity entity) =>
                {
                    EntityManager.AddComponent<EjectingComponent>(entity);
                }).Run();
        }
    }
}