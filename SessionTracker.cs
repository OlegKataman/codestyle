using System;
using BlackHole.Common;
using UnityEngine;

namespace BlackHole.Runtime
{
    public static class SessionTracker
    {
        public static Guid CurrentSessionId { get; private set; } = Guid.NewGuid();
    
        //[RuntimeInitializeOnLoadMethod]
        public static void ResetSession()
        {
            CurrentSessionId = Guid.NewGuid();

            Debug.Log($"Session: {CurrentSessionId}".AddColor(nameof(Color.red)));
        }
    }
}