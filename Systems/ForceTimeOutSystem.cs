using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;
using UnityEngine;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(WorldPositionSystem))]
    [DisableAutoCreation]
    public partial class ForceTimeOutSystem : SystemBase
    {
        private LayerMask _defaultLayer;
        private LayerMask _baseLayer;
        
        protected override void OnCreate()
        {
            _defaultLayer = LayerMask.NameToLayer("Default");
            _baseLayer = LayerMask.NameToLayer("InteractiveObject");
        }
        
        protected override void OnUpdate()
        {
            Entities
                .WithoutBurst()
                .WithAll<GameObject, ForceTimeoutComponent>()
                .WithNone<DestroyTag>()
                .ForEach((Entity entity, ref ForceTimeoutComponent forceTimeoutComponent, 
                    in GameObject gameObject, in WorldPositionComponent worldPositionComponent) =>
                {
                    var timerTimeout = forceTimeoutComponent.TimerTimeout;
                    
                    if (timerTimeout >= 1.5f)
                    {
                        if (worldPositionComponent.Position.y <= -2.5f) return;
                        
                        gameObject.layer = _defaultLayer;
                        
                        EntityManager.SetComponentEnabled<TargetTag>(entity, false);
                    }

                    if (timerTimeout > 0 && gameObject.layer == _defaultLayer)
                        forceTimeoutComponent.TimerTimeout -= 0.02f;

                    if (timerTimeout <= 0)
                        gameObject.layer = _baseLayer;

                }).Run();
        }
    }
}