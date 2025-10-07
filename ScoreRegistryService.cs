using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace BlackHole.Runtime.Service
{
    public interface IScoreRegistryService
    {
        public Action ScoreAdded { get; set; }
        public Action ScoreRemoved { get; set; }
        
        public Dictionary<string, int> Scores { get; protected set; }

        public void Clear();
    }
    
    public sealed class ScoreRegistryService : IDisposable, IScoreRegistryService
    {
        public static Action<FixedString128Bytes> OnAddScore;
        public static Action<FixedString128Bytes> OnRemoveScore;

        Action IScoreRegistryService.ScoreAdded { get; set; }
        Action IScoreRegistryService.ScoreRemoved { get; set; }
        
        Dictionary<string, int> IScoreRegistryService.Scores { get; set; } = new();

        private ScoreRegistryService()
        {
            OnAddScore += AddScore;
            OnRemoveScore += RemoveScore;
        }

        ~ScoreRegistryService()
        {
            ((IDisposable)this).Dispose();
        }
        
        void IDisposable.Dispose()
        {
            OnAddScore -= AddScore;
            OnRemoveScore -= RemoveScore;
        }
        
        void IScoreRegistryService.Clear()
        {
            ((IScoreRegistryService)this).Scores.Clear();
        }

        private void AddScore(FixedString128Bytes id)
        {
            var key = id.ToString();
            
            if (((IScoreRegistryService)this).Scores.TryGetValue(key, out var score))
            {
                ((IScoreRegistryService)this).Scores[key] = score + 1;
            }
            else
            {
                ((IScoreRegistryService)this).Scores.Add(key, 1);
            }
            
            ((IScoreRegistryService)this).ScoreAdded?.Invoke();
        }

        private void RemoveScore(FixedString128Bytes id)
        {
            var key = id.ToString();
            
            ((IScoreRegistryService)this).Scores.Remove(key, out _);
            
            ((IScoreRegistryService)this).ScoreRemoved?.Invoke();
        }
    }
}