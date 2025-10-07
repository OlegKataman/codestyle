using System;
using System.Threading;
using BlackHole.Common;
using BlackHole.Extensions;
using BlackHole.Levels.Requirement;
using BlackHole.Playable.Hole;
using BlackHole.Runtime.Service;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace BlackHole.Runtime.Lifecycle.Loop
{
    public sealed partial class Game
    { 
        [Inject] 
        private IObjectResolver _objectResolver;

        [Inject] 
        private AssetService _assetService;

        [Inject] 
        private IPlayerContainer _playerContainer;

        [Inject] 
        private ILevel _level;
        
        public ILifecycle Lifecycle { get; private set; }
        
        public async UniTask CreateLifecycle(CancellationToken cancellationToken)
        {
            var level = _level.GetType();
            var instance = await _assetService.LoadAsync<LifecycleAsset>($"lifecycle/Lifecycle-{level.Name}", cancellationToken);
            
            _objectResolver.Inject(instance);
            Lifecycle = instance;
            
            Debug.Log($"Created lifecycle {Lifecycle}");
        }

        public async UniTask<Hole> CreateHole(CancellationToken cancellationToken)
        {
            var presentHole = Object.FindAnyObjectByType<Hole>(FindObjectsInactive.Include);
            var spawnPosition = _playerContainer.StartPosition = presentHole.transform.position;
            
            var prefab = await _assetService.LoadAsync<GameObject>("Hole", cancellationToken);
            var instance = prefab.InstantiateIntoSceneLifetime(spawnPosition, Quaternion.identity);

            return instance.GetComponent<Hole>();
        }
    }

    public sealed class GameLoop : IInitializable, ITickable, IDisposable
    {
        [Inject] 
        private Game _game;
        
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private Action _tickAction = () => { };
        
        void IInitializable.Initialize()
        {
            Debug.Log("Crate Lifecycle".AddItalic());

            var random = Random.Range(0, 1000);
            
            _ = _game.CreateLifecycle(_cancellationTokenSource.Token).ContinueWith(() =>
            {
                _ = _game.Lifecycle.GetTriggerAction(LifecycleTrigger.OnInitialize)
                    .Execute(_cancellationTokenSource.Token).ContinueWith(() =>
                    {
                        _game.Lifecycle.GetTriggerAction(LifecycleTrigger.OnGameStart)
                            .Execute(_cancellationTokenSource.Token);
                    });

                //_tickAction = () => Debug.Log(random.ToString().AddColor(nameof(Color.red)));
                
                _tickAction = () =>
                    _game.Lifecycle.GetTriggerAction(LifecycleTrigger.OnUpdate)
                        .Execute(_cancellationTokenSource.Token);
            });
        }

        void ITickable.Tick()
        {
            _tickAction();
        }
        
        void IDisposable.Dispose()
        {
            _cancellationTokenSource.CancelAndDispose();
        }
    }
}