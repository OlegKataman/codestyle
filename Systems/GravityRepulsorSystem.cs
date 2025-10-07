using BlackHole.Controllers.General;
using BlackHole.ECS.AnvelopCore.Components;
using BlackHole.Runtime.Service;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(AbsorbedCheckSystem))]
    public partial class GravityRepulsorSystem : SystemBase
    {
        private LayerMask _innerLayer;
        private float _nextJumpHeight = 6f;

        protected override void OnCreate()
        {
            _innerLayer = LayerMask.NameToLayer("InnerObject");
        }
        
        protected override void OnUpdate()
        {
            // ReSharper disable Unity.BurstLoadingManagedType
            
            Entities
                .WithoutBurst()
                .WithAll<EjectingComponent, Disabled, IdComponent>()
                .WithStructuralChanges()
                //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities)
                .ForEach((Entity entity, in IdComponent idComponent, in GameObject gameObject, in Transform transform) =>
                {
                    var spawnPosition = transform.position;
                        spawnPosition.y = -1f;
                    var rangeX = spawnPosition.x + Random.Range(-2f, 2f);
                    var rangeZ = spawnPosition.z + Random.Range(-3f, 3f);

                    if (_nextJumpHeight > 30f)
                        _nextJumpHeight = 6f;
                    _nextJumpHeight += 0.5f;

                    transform.position = spawnPosition;
                    gameObject.layer = _innerLayer.value;
                    gameObject.SetActive(true);
                    
                    if (!EntityManager.HasComponent<JumpAnimationData>(entity))
                    {
                        EntityManager.AddComponentData(entity, new JumpAnimationData
                        {
                            StartPosition = new float3(spawnPosition.x, spawnPosition.y, spawnPosition.z),
                            TargetPosition = new float3(rangeX, _nextJumpHeight, rangeZ),
                            JumpPower = 1f,
                            Duration = 1f,
                            ElapsedTime = 0,
                            DelayBeforeStart = 0.2f,
                            IsJumping = false
                        });
                    }
                    
                    ScoreRegistryService.OnRemoveScore(idComponent.Id);
                }).Run();
        }
    }
}