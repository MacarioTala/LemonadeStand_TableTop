using UnityEngine;

namespace Sandbox
{
    public class EconomyInitializer
    {
        private TheEconomy lemonadeEconomy;
        private ITradeLogger tradeLogger;
        
        public TheEconomy InitializeEconomy()
        {
            // Create and initialize the economy with a trade logger
            tradeLogger = new TradeLoggerV1();
            TheEconomy.SetupForTests(tradeLogger);
            lemonadeEconomy = TheEconomy.Instance;
            Debug.Log("Economy initialized successfully.");
            return lemonadeEconomy;
        }
        
        public TheEconomy GetEconomy() => lemonadeEconomy;
    }
}
