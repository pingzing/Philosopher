using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philosopher.Multiplat.Services
{
    public interface IMagicPacketService
    {
        Task SendMagicPacket(string hostName, int destPort, string targetMac);        
    }

    public class MagicPacketService : IMagicPacketService
    {
        bool _initialized = false;
        private UdpSocketClient _client;        

        public async Task SendMagicPacket(string hostName, int destPort, string targetMac)
        {
            if(!_initialized)
            {
                _client = new UdpSocketClient();
            }

            //turn MAC into an array of bytes
            targetMac = targetMac.Replace("-", "").Replace(":", "");
            byte[] macBytes = Enumerable.Range(0, targetMac.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(targetMac.Substring(x, 2), 16)) //16 == hexadecimal
                .ToArray();
            //Magic packet header
            List<byte> magicPacket = new List<byte> { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            //Magic packet is the header + the target MACx16
            for(int i = 0; i < 16; i++)
            {
                magicPacket = magicPacket.Concat(macBytes).ToList();
            }

            await _client.SendToAsync(magicPacket.ToArray(), hostName, destPort);            
        }
    }
}
