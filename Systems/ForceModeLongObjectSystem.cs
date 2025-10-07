using BlackHole.ECS.AnvelopCore.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(WorldPositionSystem))]
    [DisableAutoCreation]
    public partial class ForceModeLongObjectSystem : SystemBase
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
            if (!SystemAPI.TryGetSingletonEntity<HoleComponent>(out var hole)) return;
            
            var holePosition = SystemAPI.GetComponent<WorldPositionComponent>(hole).Position;
            var holeComponent = SystemAPI.GetComponent<HoleComponent>(hole);
            var holePosFlat = new float3(holePosition.x, 0, holePosition.z);

            Entities
                .WithoutBurst()
                .WithAll<TargetTag, LongObjectComponent>()
                .WithChangeFilter<WorldPositionComponent>() // Фильтр изменений позиции
                .ForEach((Entity entity, ref ForceTimeoutComponent forceTimeoutComponent, 
                    in Rigidbody rigidBody, in GameObject gameObject, in Transform transform,
                    in WorldPositionComponent worldPositionComponent, in LongObjectComponent longObjectComponent) =>
                {
                    var direction = holePosFlat - worldPositionComponent.Position;
                        direction.y = 0;
                    //var direction2 = new float3(holePosition.x, -2f, holePosition.y) - worldPositionComponent.Position;

                    var radiusForce = holeComponent.RadiusForce;
                    var radiusMinDistanceForce = longObjectComponent.MinDistanceToForce + (radiusForce / 4);
                    var radiusForceSq = radiusMinDistanceForce * radiusMinDistanceForce;
                    
                    var distanceSq = math.distancesq(new float3(holePosFlat.x, 0, holePosFlat.z), 
                        new float3(worldPositionComponent.Position.x, 0, worldPositionComponent.Position.z));
                    
                    if (distanceSq < radiusForceSq)
                    {
                        forceTimeoutComponent.TimerTimeout += 0.01f;
                        
                        var dotVectors = math.dot(transform.forward, math.normalize(direction));
                        var dotValue = (1 - (radiusForce / 55));
                        
                        if (math.abs(dotVectors) < dotValue) 
                            return;
                        
                        var pointLocalCenterMass = transform.InverseTransformPoint(new Vector3(holePosFlat.x, 0, holePosFlat.z));
                        rigidBody.centerOfMass = pointLocalCenterMass;
                        
                        //if (math.abs(dotVectors) > 0.9)
                            //rigidBody.AddForce((direction2 * dotVectors) * 20);
                        
                        gameObject.layer = _forceLayer.value;
                    }
                    else
                    {
                        if (worldPositionComponent.Position.y >= 0f)
                        {
                            gameObject.layer = _baseLayer.value;
                            rigidBody.ResetCenterOfMass();
                            rigidBody.ResetInertiaTensor();
                        }
                        
                        EntityManager.SetComponentEnabled<TargetTag>(entity, false);
                    }

                }).Run();
        }
    }
}

//rigidBody.AddRelativeTorque((Vector3.up * dotVectors) * forceModeComponent.Force * 20, ForceMode.Impulse);