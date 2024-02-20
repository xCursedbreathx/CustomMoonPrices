using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace CustomMoonPrices
{
    public static class Extensions
    {

        public static void SendMessage(this FastBufferWriter stream, string label, ulong clientId = 0uL)
        {
            bool fragment = stream.Capacity > 1300;
            NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;

            if (fragment) CustomMoonPricesMain.CMPLogger.LogError(
                $"Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
                "Config instance will be sent in fragments to avoid overflowing the buffer."
            );

            var msgManager = NetworkManager.Singleton.CustomMessagingManager;
            msgManager.SendNamedMessage(label, clientId, stream, delivery);
        }

    }
}
