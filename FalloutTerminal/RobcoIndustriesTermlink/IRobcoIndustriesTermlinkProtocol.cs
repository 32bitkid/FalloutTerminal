using FalloutTerminal.Communications;

namespace FalloutTerminal.RobcoIndustriesTermlink
{
    public interface IRobcoIndustriesTermlinkProtocol
    {
        ISerialConnection Connection { get;  }
        void Boot(V300.RunModes mode);
    }
}
