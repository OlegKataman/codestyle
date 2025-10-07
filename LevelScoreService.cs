using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackHole.Runtime.Service
{
    public interface ILevelScoreService
    {
        void UpdateLevelProgress(UserProfile profile, LevelProgress newProgress);
        LevelProgress GetLevelProgress(UserProfile profile, string levelId);
    }
    
    public sealed class LevelScoreService : ILevelScoreService
    {
        public void UpdateLevelProgress(UserProfile profile, LevelProgress newProgress)
        {
            var existingProgress = profile.LevelProgress.FirstOrDefault(lp => lp.LevelId == newProgress.LevelId);
            if (existingProgress != null)
            {
                existingProgress.Score = newProgress.Score;
                existingProgress.StarsEarned = newProgress.StarsEarned;
                existingProgress.CompletionTime = newProgress.CompletionTime;
                existingProgress.IsCompleted = newProgress.IsCompleted;
                existingProgress.UnlockedAchievements = newProgress.UnlockedAchievements;
                existingProgress.AssetCollected = newProgress.AssetCollected;
            }
            else
            {
                profile.LevelProgress.Add(newProgress);
            }
        }

        public LevelProgress GetLevelProgress(UserProfile profile, string levelId)
        {
            return profile.LevelProgress.FirstOrDefault(lp => lp.LevelId == levelId);
        }
    }
    
    public record LevelProgress
    {
        public string LevelId { get; set; }
        public bool IsCompleted { get; set; }
        public int Score { get; set; }
        public int StarsEarned { get; set; } // 0-3
        public float CompletionTime { get; set; }
        public List<string> UnlockedAchievements { get; set; } = new();
        public Dictionary<string, int> AssetCollected { get; set; } = new();
    }
}