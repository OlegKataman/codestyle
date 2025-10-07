using BlackHole.Runtime.Service;
using VContainer;

namespace BlackHole.Runtime
{
    public interface IScoreSource
    {
        IScoreRegistryService ScoreRegistryService { get; }
        
        IProfileService ProfileService { get; }
    }
    
    public sealed class ScoreSource : IScoreSource
    {
        public IScoreRegistryService ScoreRegistryService { get; }
        public IProfileService ProfileService { get; }
        
        [Inject]
        public ScoreSource (IObjectResolver resolver)
        {
            ScoreRegistryService = resolver.ResolveOrDefault<IScoreRegistryService>();
            
            ProfileService = resolver.Resolve<IProfileService>();
        }
    }
}