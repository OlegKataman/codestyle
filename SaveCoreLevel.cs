using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlackHole.Assets;
using BlackHole.Extensions;
using BlackHole.Levels.Requirement;
using BlackHole.Runtime.Score;
using BlackHole.Runtime.Service;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace BlackHole.Runtime.Lifecycle.Actions
{
    public sealed class SaveCoreLevel : IAction
    {
        [SerializeField] 
        private List<AssetReferenceT<ScoreGroup>> _trashGroup = new();
        
        [Inject]
        private IScoreSource _scoreSource;
        
        [Inject]
        private ILevelScoreService _levelScoreService;

        [Inject] 
        private ITimeTracker _timeTracker;

        [Inject] 
        private IAssetService _assetService;
        
        [Inject] 
        private ILevel _level;
        
        private const double GoalPercentageThreshold = 70.0;
        
        public async UniTask Execute(CancellationToken cancellationToken)
        {
            var profile = _scoreSource.ProfileService.GetProfile<UserProfile>();
            
            var stars = await CalculateStars(cancellationToken);
            
            _levelScoreService.UpdateLevelProgress(profile, new LevelProgress
            {
                LevelId = _level.AddressableAddress,
                Score = stars is 0 ? 0 : _level.ScorePerLevel / stars,
                StarsEarned = stars,
                AssetCollected = CalculateAssets(),
                CompletionTime = _timeTracker.WatchTime,
                IsCompleted = true,
            });
            
            Debug.Log("Saved");
            _scoreSource.ProfileService.Save();
        }

        private Dictionary<string, int> CalculateAssets()
        {
            using (ListScope<KeyValuePair<string,int>>.Create(out var assets))
            {
                foreach (var asset in _scoreSource.ScoreRegistryService.Scores)
                {
                    assets.Add(asset);
                }

                return new Dictionary<string, int>(assets);
            }
        }

        async UniTask<int> CalculateStars(CancellationToken cancellationToken)
        {
            var stars = 3;
            var requirements = _level.Requirements;
            
            var scores = await GetScoresWithoutTrash(cancellationToken);
            var strongScores = requirements.Where(x => x.Strong && x.Goal == false).ToList();
            var goalScores = requirements.Where(x => x.Goal).ToList();

            if (strongScores.Any())
            {
                if (!Earned(strongScores))
                {
                    stars--;

                    Debug.Log("Не собраны strong assets");
                }
            }

            if (goalScores.Any())
            {
                if (!Earned(goalScores))
                {
                    stars--;

                    Debug.Log("Не собраны goal assets");
                }
            }

            Debug.Log(scores.Count);

            foreach (var score in scores)
            {
                Debug.Log(score.Score.AssetGUID);
            }
            
            if (!EarnedCheckWithPercent(scores))
            {
                stars--;
                
                Debug.Log("Не собраны percentage check assets");
            }
            
            return Mathf.Clamp(stars, 0, 3);
        }

        async UniTask<List<RequiredScores>> GetScoresWithoutTrash(CancellationToken cancellationToken)
        {
            var requirements = _level.Requirements;
            var list = requirements.Where(x => x.Strong == false && x.Goal == false).ToList();

            foreach (var required in list.ToList())
            {
                var score = await _assetService.LoadAsync<InteractiveAsset>(required.Score.AssetGUID, cancellationToken);
                var group = await _assetService.LoadAsync<ScoreGroup>(score.GroupReference.AssetGUID, cancellationToken);

                foreach (var trash in _trashGroup)
                {
                    Debug.Log(group.Key + " / " + trash.AssetGUID);
                    
                    if (group.Key == trash.AssetGUID)
                    {
                        Debug.Log("++++++++++++++++++++++++++++++++++++++");
                        list.Remove(required);
                    }
                }
            }
            
            return list;
        }

        private bool Earned(List<RequiredScores> requiredScores)
        {
            var allRequirementsMet = true;
            
            foreach (var required in requiredScores)
            {
                var sum = _scoreSource.ScoreRegistryService.Scores
                    .Where(x => x.Key == required.Score.AssetGUID)
                    .Sum(x => x.Value);

                if (required.Value >= sum) continue;
                    
                allRequirementsMet = false;
                break;
            }

            return allRequirementsMet;
        }
        
        private bool EarnedCheckWithPercent(List<RequiredScores> requiredScores)
        {
            var allRequirementsMet = true;

            foreach (var required in requiredScores)
            {
                var sum = _scoreSource.ScoreRegistryService.Scores
                    .Where(x => x.Key == required.Score.AssetGUID)
                    .Sum(x => x.Value);
                    
                var percentage = required.Value != 0 ? ((double)sum / required.Value) * 100 : 0;
                
                if (percentage > GoalPercentageThreshold) continue;

                allRequirementsMet = false;
                break;
            }
            
            return allRequirementsMet;
        }
    }
}