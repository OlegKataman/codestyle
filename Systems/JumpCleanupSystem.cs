using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;
using UnityEngine;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(JumpAnimationSystem))]
    public partial class JumpCleanupSystem : SystemBase
    {
        private LayerMask _baseLayer;
        public struct JumpCompletedTag : IComponentData { }

        protected override void OnCreate()
        {
            _baseLayer = LayerMask.NameToLayer("InteractiveObject");
        }
        
        protected override void OnUpdate()
        {
            // ReSharper disable Unity.BurstLoadingManagedType
            
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<JumpCompletedTag>()
                .ForEach((Entity entity, in GameObject gameObject, in Rigidbody rigidbody) =>
                {
                    gameObject.layer = _baseLayer.value;
                    rigidbody.isKinematic = false;
                    Physics.SyncTransforms();
                    
                    EntityManager.RemoveComponent<JumpCompletedTag>(entity);
                    EntityManager.RemoveComponent<JumpAnimationData>(entity);
                    EntityManager.RemoveComponent<EjectingComponent>(entity);
                    EntityManager.RemoveComponent<AbsorbedTag>(entity);
                    //EntityManager.SetComponentEnabled<ForceModeComponent>(entity, true);
                }).Run();
        }
    }
}