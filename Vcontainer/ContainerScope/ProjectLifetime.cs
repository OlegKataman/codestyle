using BlackHole.Runtime;
using BlackHole.Runtime.Fsm;
using BlackHole.Runtime.Service;
using BlackHole.Runtime.Signals.Core;
using UnityEngine.LowLevel;
using VContainer;
using VContainer.Unity;

namespace BlackHole.ContainerScope
{
    public sealed class ProjectLifetime : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ProfileService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PersistentService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<ITimeTracker, TimeTracker>(Lifetime.Singleton);
            
            builder.Register<ScoreSource>(Lifetime.Transient).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelScoreService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            builder.Register<AsyncFsm>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SceneLoader>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlayerContainer>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            # region Signals
            
            builder.Register<SignalReceiverCollection>(Lifetime.Singleton);
            builder.Register<SignalBus>(Lifetime.Singleton);
            builder.Register<ISignalBus>(c => c.Resolve<SignalBus>(), Lifetime.Singleton);
            builder.Register<SignalHandlerRegistrar>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SignalHandlerInitializer>();
            
            # endregion

            base.Configure(builder);
        }
    }
}
