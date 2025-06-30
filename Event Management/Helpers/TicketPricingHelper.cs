namespace Event_Management.Helpers
{
    public static class TicketPricingHelper
    {
        public static Dictionary<string, decimal> ParsePricing(string pricingText)
        {
            var pricingDict = new Dictionary<string, decimal>();

            if (string.IsNullOrWhiteSpace(pricingText))
                return pricingDict;

            var parts = pricingText.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var unlabelledPrices = new List<decimal>();

            foreach (var part in parts)
            {
                var trimmed = part.Trim();

                if (trimmed.Contains("-"))
                {
                    var split = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    var label = split[0].Trim();
                    if (decimal.TryParse(split[1].Trim(), out var price))
                    {
                        pricingDict[label] = price;
                    }
                }
                else
                {
                    if (decimal.TryParse(trimmed, out var price))
                    {
                        unlabelledPrices.Add(price);
                    }
                }
            }

            if (unlabelledPrices.Count == 1)
            {
                pricingDict["General"] = unlabelledPrices[0];
            }
            else if (unlabelledPrices.Count > 1)
            {
                for (int i = 0; i < unlabelledPrices.Count; i++)
                {
                    pricingDict[$"General {GetSuffix(i)}"] = unlabelledPrices[i];
                }
            }

            return pricingDict;
        }

        private static string GetSuffix(int index)
        {
            var result = "";
            while (index >= 0)
            {
                result = (char)('A' + (index % 26)) + result;
                index = (index / 26) - 1;
            }
            return result;
        }
    }

}
