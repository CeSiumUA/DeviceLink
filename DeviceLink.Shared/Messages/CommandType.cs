using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceLink.Shared.Messages
{
    public enum CommandType : byte
    {
        Handshake = 1,
        SetMousePosition,
        TransferAudio,
    }
}
