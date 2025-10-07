using System;
using BlackHole.ContainerScope;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace BlackHole.Common
{
    public sealed class AutoResolver : MonoBehaviour
    {
        [SerializeField]
        private Scope _scope;
        
        private void Awake()
        {
            var resolver = GetResolver();

            if (resolver == null)
                throw new NullReferenceException("resolver is null");
            
            resolver.InjectGameObject(this.gameObject);
        }

        private IObjectResolver GetResolver()
        {
            switch (_scope)
            {
                case Scope.Scene:
                {
                    var context = Object.FindAnyObjectByType<SceneLifetime>();
                    var container = context.Container;

                    return container;
                }
                case Scope.Project:
                {
                    var context = Object.FindAnyObjectByType<ProjectLifetime>();
                    var container = context.Container;

                    return container;
                }
                case Scope.Hierarchy:
                default:
                {
                    var keeper = GetComponentInParent<ResolverKeeper>();
                    var container = keeper.ObjectResolver;
                    
                    return container;
                }
            }
        }
        
        private enum Scope
        {
            Hierarchy,
            Scene,
            Project
        }
    }
}
