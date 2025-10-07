using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlackHole.Runtime.Service;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VContainer;

namespace BlackHole.Runtime
{
    public interface IUserProfile
    {
        string Version { get; set; }
        int TotalScore { get; set; }
        int TotalStars { get; set; }
        
        void OnCreate();
    }
    
    public sealed class UserProfile : IUserProfile
    {
        public string Version { get; set; }
        public int TotalScore { get; set; }
        public int TotalStars { get; set; }
        
        public void OnCreate()
        {
            DoAsync().Forget();
            return;
            
            async UniTask DoAsync()
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var levels = await _assetService.LoadAsync<LevelsAsset>("LevelsCollectionAsset", cancellationTokenSource.Token);

                foreach (var level in levels.Items)
                {
                    _levelScoreService.UpdateLevelProgress(this, new LevelProgress
                    {
                        LevelId = level.AddressableAddress,
                    });
                }
            }
        }

        public UserSettings UserSettings { get; set; }
        
        public List<LevelProgress> LevelProgress { get; set; } = new();
        
        [Inject]
        private ILevelScoreService _levelScoreService;
        
        [Inject]
        private IAssetService _assetService;
    }

    public record UserProfileRecord
    {
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("totalScore")] public int TotalScore { get; set; }
        [JsonProperty("totalStars")] public int TotalStars { get; set; }
        
        [JsonProperty("userSettings")] public UserSettings UserSettings { get; set; } = new(); 
        [JsonProperty("levelProgress")] public List<LevelProgress> LevelProgress { get; set; } = new(); 
    }
    
    public sealed class UserSettings
    {
        public string Language { get; set; } = "en-US";
        public bool SoundEnabled { get; set; } = true;
        public float MusicVolume { get; set; } = 1.0f;
    }
}