using System;
using BlackHole.Runtime.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BlackHole.Runtime.Score
{
    [CreateAssetMenu(fileName = "Group", menuName = "Game/ScoreGroup")]
    [Serializable]
    public sealed class ScoreGroup : AddressableScriptable
    {
        public string Name => $"{name}";
        
        #region UIView
        [field: FoldoutGroup("UIView")]
        [field: SerializeField]
        public AssetReferenceGameObject UIViewReference { get; set; }
        
        [field: FoldoutGroup("UIView")]
        [field: SerializeField]
        public Sprite UIViewSpriteIcon { get; set; }
        #endregion
    }
}
