using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketMessaging.Services.SMTPClient
{
    public enum SmtpPriority : byte
    {
        High = 1,
        Normal = 3,
        Row = 5
    }
}
