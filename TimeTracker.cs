using System;
using System.Threading;
using BlackHole.Common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace BlackHole.Runtime
{
    public interface ITimeTracker
    {
        int WatchTime { get; }
        int WatchTimerTime { get; }
        
        UniTask StartWatch(int watchSeconds);
        void StartWatch();
        void StopWatch();
    }
    
    public sealed class TimeTracker : ITimeTracker
    {
        public int WatchTime { get; private set; }
        public int WatchTimerTime { get; private set; }

        private const float TimeBuffer = 0.5f;
        private CancellationTokenSource _watchTokenSource;
        
        async UniTask ITimeTracker.StartWatch(int watchSeconds)
        {
            Debug.Log($"@ StartWatch {watchSeconds}");
            
            WatchTimerTime = watchSeconds;
            WatchTime = 0;
            
            _watchTokenSource?.CancelAndDispose();
            _watchTokenSource = new CancellationTokenSource();
            
            var token = _watchTokenSource.Token;
            
            UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(1), PlayerLoopTiming.Update, ignoreTimeScale: false)
                .ForEachAsync(_ =>
                {
                    if (token.IsCancellationRequested) return;

                    WatchTime++;
                    WatchTimerTime--;
                }, token).Forget();

            if (watchSeconds > 0)
            {
                await UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(watchSeconds + TimeBuffer), PlayerLoopTiming.Update, ignoreTimeScale: false)
                    .ForEachAsync(_ =>
                    {
                        Debug.Log("Timer End!");
                    }, token);
            }
        }

        void ITimeTracker.StartWatch()
        {
            Debug.Log("@ StartWatch");
            
            WatchTime = 0;
            
            _watchTokenSource?.CancelAndDispose();
            _watchTokenSource = new CancellationTokenSource();
            
            UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(1), PlayerLoopTiming.Update, ignoreTimeScale: false)
                .ForEachAsync(_ =>
                {
                    WatchTime++;
                }, _watchTokenSource.Token).Forget();
        }

        void ITimeTracker.StopWatch()
        {
            _watchTokenSource?.CancelAndDispose();
            _watchTokenSource = null;
        }
    }
}