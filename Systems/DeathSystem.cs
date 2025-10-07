using System;
using BlackHole.ECS.AnvelopCore.Components;
using BlackHole.Runtime.Service;
using Unity.Entities;
using UnityEngine;
using VContainer;

namespace BlackHole.ECS.AnvelopCore.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ForceModeSystem))]
    public partial class DeathSystem : SystemBase
    {
        [Inject]
        private IScoreRegistryService _scoreService;
        
        protected override void OnUpdate()
        {
            // ReSharper disable Unity.BurstLoadingManagedType
            
            if (!SystemAPI.TryGetSingletonEntity<HoleComponent>(out var hole)) return;
            var holeComponent = SystemAPI.GetComponent<HoleComponent>(hole);
            
            Entities
                .WithoutBurst()
                .WithAll<TargetTag, IdComponent>()
                .WithNone<AbsorbedTag>()
                .WithChangeFilter<WorldPositionComponent>() // Фильтр изменений позиции
                .WithStructuralChanges()
                .ForEach((Entity entity, in WorldPositionComponent worldPositionComponent, 
                    in IdComponent idComponent, in GameObject gameObject, in Rigidbody rigidbody) =>
                {
                    if (worldPositionComponent.Position.y <= -holeComponent.RadiusForce)
                    {
                        Debug.Log($"AddScore: {idComponent.Id}");
                        
                        Vibration.VibratePop();
                        ScoreRegistryService.OnAddScore(idComponent.Id);
                        //_audioManager.Play("pop");
                        
                        gameObject.SetActive(false);
                        rigidbody.isKinematic = true;
                        
                        EntityManager.SetComponentEnabled<TargetTag>(entity, false);
                        //EntityManager.AddComponent<AbsorbedTag>(entity); // test offffff
                    }
                }).Run();
        }
    }
}