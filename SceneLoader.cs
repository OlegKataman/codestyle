using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlackHole.Common;
using BlackHole.Extensions;
using BlackHole.Levels.Requirement;
using BlackHole.Runtime.Fsm;
using BlackHole.Runtime.Service;
using BlackHole.UI.Common;
using BlackHole.UI.Fragments.LoadingFragment;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace BlackHole.Runtime
{
    public enum Scene
    {
        Home,
        Game,
    }
    
    public enum ReloadAdditive
    {
        Inactive,
        Active,
    }
    
    public interface ISceneLoader
    {
        UniTask BeginTransition(CancellationToken cancellationToken);
        UniTask CompleteTransition(CancellationToken cancellationToken);
        UniTask LoadHomeAsync(CancellationToken cancellationToken);
        UniTask LoadLevelAsync(ILevel nextLevel, LoadSceneMode mode, CancellationToken cancellationToken, ReloadAdditive reloadMode = ReloadAdditive.Inactive);
        UniTask LoadNextLevelAsync(CancellationToken cancellationToken);
    }
    
    public sealed class SceneLoader : ISceneLoader
    {
        [Inject] 
        private IUIService _uiService;
        
        [Inject]
        private IAsyncFsm _fsm;
        
        [Inject]
        private ILevelProvider _levelProvider;

        [Inject] 
        private IAssetService _assetService;

        async UniTask ISceneLoader.BeginTransition(CancellationToken cancellationToken)
        {
            await _uiService.Open<LoadingFragment>(new LoadingFragmentModel());
        }

        UniTask ISceneLoader.CompleteTransition(CancellationToken cancellationToken)
        {
            return _uiService.Close<LoadingFragment>();
        }
        
        async UniTask ISceneLoader.LoadHomeAsync(CancellationToken cancellationToken)
        {
            await LoadSceneAsync(Scene.Home, LoadSceneMode.Single, _ => { }, cancellationToken);
            
            var homeState = new HomeState();
                homeState.InjectIntoSceneLifetime();
                
            await _fsm.To(homeState, cancellationToken);
        }

        async UniTask ISceneLoader.LoadLevelAsync(ILevel nextLevel, LoadSceneMode mode, 
            CancellationToken cancellationToken, ReloadAdditive reloadMode)
        {
            await LoadSceneAsync(Scene.Game, mode, builder =>
            {
                //builder.RegisterInstance(nextLevel).As<ILevel>();

            }, cancellationToken);
            
            _levelProvider.CurrentLevel = nextLevel;
            
            var scenes = nextLevel.AdditiveScenes.Select(scene => scene.name).ToList();
            await LoadAdditiveSceneAsync(scenes, reloadMode, cancellationToken);

            Lightmapping.lightingDataAsset = nextLevel.LightingData;
            
            var gameState = new GameState();
                gameState.InjectIntoSceneLifetime();
                
            Debug.Log(nextLevel.AddressableAddress.AddColor(nameof(Color.red)));
            
            await _fsm.To(gameState, cancellationToken);
        }

        async UniTask ISceneLoader.LoadNextLevelAsync(CancellationToken cancellationToken)
        {
            var levels = await _assetService.LoadAsync<LevelsAsset>("LevelsCollectionAsset", cancellationToken);

            var currentLevel = levels.Items.First(x => x.AddressableAddress == _levelProvider.CurrentLevel.AddressableAddress);
            var index = levels.Items.IndexOf(currentLevel);
            var nextLevel = levels.Items.ElementAt(index + 1);

            await ((ISceneLoader)this).LoadLevelAsync(nextLevel, LoadSceneMode.Single, cancellationToken);
        }
        
        private async UniTask LoadSceneAsync(Scene scene , LoadSceneMode mode,
            Action<IContainerBuilder> action, CancellationToken cancellationToken)
        {
            await _fsm.From(cancellationToken); // черный экран
            
            using (LifetimeScope.Enqueue(action))
            {
                if (!_uiService.TryGet<LoadingFragment>(out _))
                {
                    await _uiService.Open<LoadingFragment>(new LoadingFragmentModel());
                }
            }
            {
                if (SceneManager.GetActiveScene().name == scene.ToString())
                    return;

                Debug.Log("Load " + scene);
                await SceneManager.LoadSceneAsync(scene.ToString(), mode).WithCancellation(cancellationToken);
            }
        }

        private async UniTask LoadAdditiveSceneAsync(List<string> scenes, ReloadAdditive reloadMode, 
            CancellationToken cancellationToken)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                
                if (scene.isLoaded)
                {
                    if (scenes.Any(x => x == scene.name) && reloadMode is ReloadAdditive.Inactive)
                        continue;
                    
                    if (scene.name is "Game" or "Home")
                        continue;
                    
                    SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.None).WithCancellation(cancellationToken);
                }
            }
            
            foreach (var scene in scenes)
            {
                if (SceneManager.GetSceneByName(scene).isLoaded && reloadMode is ReloadAdditive.Inactive)
                    continue;
                
                Debug.Log("Additive Load " + scene);
                await SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive).WithCancellation(cancellationToken);
            }
        }
    }
}