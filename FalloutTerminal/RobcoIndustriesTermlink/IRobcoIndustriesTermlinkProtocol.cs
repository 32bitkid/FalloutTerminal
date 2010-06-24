using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FalloutTerminal
{
    public interface IRobcoIndustriesTermlinkProtocol
    {
        //StreamWriter OutputStream { get; }
        //StreamReader InputStream { get; }
        void Boot();
    }
}
