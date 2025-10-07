using BlackHole.Tools.Joystick_Pack.Scripts.Base;

namespace BlackHole.Runtime
{
    interface IJoystickProvider
    {
        Joystick CurrentJoystick { get; set; }
    }
    
    public sealed class JoystickProvider : IJoystickProvider
    {
        public Joystick CurrentJoystick { get; set; }

        public JoystickProvider()
        {
            CurrentJoystick = null;
        }
    }
}