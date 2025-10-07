using BlackHole.Levels.Requirement;

namespace BlackHole.Runtime
{
    interface ILevelProvider
    {
        ILevel CurrentLevel { get; set; }
    }
    
    public sealed class LevelProvider : ILevelProvider
    {
        public ILevel CurrentLevel { get; set; }
    }
}