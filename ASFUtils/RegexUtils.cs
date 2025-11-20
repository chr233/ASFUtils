using System.Text.RegularExpressions;

namespace ASFCsgoDelivery;
internal static partial class RegexUtils
{
    [GeneratedRegex(@"在 (\d+) (\d+)月 (\d+) \((\d+:\d+:\d+)\)")]
    public static partial Regex MatchTradingTime();

    [GeneratedRegex(@"tradeofferid_(\d+)")]
    public static partial Regex MatchTradeOfferId();

    [GeneratedRegex(@"(?:https?:\/\/steamcommunity\.com\/tradeoffer\/new\/\?)?partner=(\d+)&token=(\S+)")]
    public static partial Regex GenMatchTradeLink();
}
