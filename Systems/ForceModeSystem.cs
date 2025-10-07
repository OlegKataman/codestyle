using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(WorldPositionSystem))]
    [DisableAutoCreation]
    public partial class ForceModeSystem : SystemBase
    {
        private LayerMask _baseLayer;
        private LayerMask _forceLayer;
        
        protected override void OnCreate()
        {
            _baseLayer = LayerMask.NameToLayer("InteractiveObject");
            _forceLayer = LayerMask.NameToLayer("ForceObject");
        }
        
        protected override void OnUpdate()
        {
            // ReSharper disable Unity.BurstLoadingManagedType
            
            if (!SystemAPI.TryGetSingletonEntity<HoleComponent>(out var hole)) return;
            
            var holePosition = SystemAPI.GetComponent<WorldPositionComponent>(hole).Position;
            var holeComponent = SystemAPI.GetComponent<HoleComponent>(hole);
            var holePosFlat = new float3(holePosition.x, 0, holePosition.z);
            
            Entities
                .WithoutBurst()
                .WithAll<TargetTag, ShortObjectComponent>()
                .WithNone<DestroyTag>() //// ?
                .WithChangeFilter<WorldPositionComponent>() // Фильтр изменений позиции
                .ForEach((ref ForceTimeoutComponent forceTimeoutComponent, in Rigidbody rigidBody, in GameObject gameObject, 
                    in WorldPositionComponent worldPositionComponent, in ShortObjectComponent forceModeShortObject) =>
                {
                    var forceDirection = Vector3.zero;
                    
                    var direction = holePosFlat - worldPositionComponent.Position;
                        direction.y = 0;
                    
                    var radiusForceSq = holeComponent.RadiusForce * holeComponent.RadiusForce;
                    var radiusHoleSq = (holeComponent.RadiusForce / 2) * (holeComponent.RadiusForce / 2);

                    var distanceSq = math.distancesq(new float3(holePosFlat.x, 0, holePosFlat.z), 
                        new float3(worldPositionComponent.Position.x, 0, worldPositionComponent.Position.z));
                    
                    if (distanceSq < radiusHoleSq)
                    {
                        gameObject.layer = _forceLayer.value;
                        
                        var pointLocalCenterMass = gameObject.transform.InverseTransformPoint(new Vector3(holePosFlat.x, 0, holePosFlat.z));

                        rigidBody.centerOfMass = pointLocalCenterMass;
                        
                        forceTimeoutComponent.TimerTimeout += 0.01f;
                    }
                    else
                    {
                        gameObject.layer = _baseLayer.value;
                        
                        if (!forceModeShortObject.ForceWithoutMove && distanceSq < radiusForceSq)
                        {
                            if (worldPositionComponent.Position.y < 0) return;
                            
                            forceDirection = (direction) * forceModeShortObject.MoveSpeed / math.length(direction);
                        }
                    }
                    
                    rigidBody.AddForce(forceDirection);
                }).Run();
        }
    }
}

//var holePosition = EntityManager.GetComponentData<WorldPositionComponent>(holeEntity).Position;
//Debug.Log(direction.magnitude.ToString());

//forceDirection = math.down() * (forceModeComponent.Force / math.length(direction));
//forceDirection = direction * forceModeComponent.Force;