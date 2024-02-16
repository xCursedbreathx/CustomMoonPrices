using LCCustomMoonPrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;

namespace CustomMoonPrices
{
    public class Config : SyncedInstance<Config>
    {

        public static Dictionary<string, bool> moonPriceEnabled = new Dictionary<string, bool>();

        public static Dictionary<string, int> moonPrice = new Dictionary<string, int>();

        public Config()
        {
            InitInstance(this);
        }

        public void updateMoon(string moonname, bool enabled, int price)
        {

            CustomMoonPricesMain.CMPLogger.LogMessage("Updating Moon: " + moonname + " Enabled: " + enabled + " Price: " + price);
            if (!IsHost) return;
            CustomMoonPricesMain.CMPLogger.LogMessage("Moon Update from Host syncing Config to Players again.");
            moonPriceEnabled[moonname] = enabled;
            moonPrice[moonname] = price;

            CustomMoonPricesMain.CMPLogger.LogMessage("Moon Updated Data: " + moonname + " a: " + moonPriceEnabled[moonname] + " p: " + moonPrice[moonname]);

            byte[] bytes = SerializeToBytes(SyncedInstance<Config>.Instance);
            int length = bytes.Length;

            using (FastBufferWriter messageStream = new FastBufferWriter(IntSize + length, Allocator.Temp))

                MessageManager.SendNamedMessageToAll("CustomMoonPrices_ResyncConfigOnChange", messageStream);

        }

    }
}
