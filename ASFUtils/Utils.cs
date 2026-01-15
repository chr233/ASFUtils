using ArchiSteamFarm.Core;
using ArchiSteamFarm.NLog;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;

using ASFUtils.Data;

using System.Reflection;
using System.Text;

namespace ASFUtils;

internal static class Utils
{
    internal const StringSplitOptions SplitOptions =
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    /// <summary>
    ///     插件配置
    /// </summary>
    internal static PluginConfig Config { get; set; } = null!;

    /// <summary>
    ///     获取版本号
    /// </summary>
    internal static Version MyVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version("0.0.0.0");

    /// <summary>
    ///     获取ASF版本
    /// </summary>
    internal static Version ASFVersion => typeof(ASF).Assembly.GetName().Version ?? new Version("0.0.0.0");

    /// <summary>
    ///     获取插件所在路径
    /// </summary>
    internal static string MyLocation => Assembly.GetExecutingAssembly().Location;

    /// <summary>
    ///     获取插件所在文件夹路径
    /// </summary>
    internal static string MyDirectory => Path.GetDirectoryName(MyLocation) ?? ".";

    /// <summary>
    ///     Steam商店链接
    /// </summary>
    internal static Uri SteamStoreURL => ArchiWebHandler.SteamStoreURL;

    /// <summary>
    ///     Steam社区链接
    /// </summary>
    internal static Uri SteamCommunityURL => ArchiWebHandler.SteamCommunityURL;

    /// <summary>
    ///     SteamAPI链接
    /// </summary>
    internal static Uri SteamApiURL => new("https://api.steampowered.com");

    /// <summary>
    ///     Steam结算链接
    /// </summary>
    internal static Uri SteamCheckoutURL => ArchiWebHandler.SteamCheckoutURL;

    internal static Uri SteamHelpURL => ArchiWebHandler.SteamHelpURL;

    /// <summary>
    ///     日志
    /// </summary>
    internal static ArchiLogger ASFLogger => ASF.ArchiLogger;

    /// <summary>
    /// 转换SteamId
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    internal static ulong SteamId2Steam32(ulong steamId)
    {
        return steamId & 0x001111011111111;
    }

    /// <summary>
    /// 转换SteamId
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    internal static ulong Steam322SteamId(ulong steamId)
    {
        return steamId | 0x110000100000000;
    }

    /// <summary>
    ///     格式化返回文本
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static string FormatStaticResponse(string message) => $"<ASFE> {message}";

    /// <summary>
    ///     格式化返回文本
    /// </summary>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatStaticResponse(string message, params object?[] args) =>
        FormatStaticResponse(string.Format(message, args));

    /// <summary>
    ///     格式化返回文本
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static string FormatBotResponse(this Bot bot, string message) => $"<{bot.BotName}> {message}";

    /// <summary>
    ///     格式化返回文本
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static string FormatBotResponse(this Bot bot, string message, params object?[] args) =>
        bot.FormatBotResponse(string.Format(message, args));

    internal static StringBuilder AppendLineFormat(this StringBuilder sb, string format, params object?[] args) =>
        sb.AppendLine(string.Format(format, args));

#if DEBUG
    internal static bool IsDebug = true;
#else
    internal static bool IsDebug = false;
#endif

    private static readonly List<Bot.EFileType> FileTypes = [Bot.EFileType.Config, Bot.EFileType.Database, Bot.EFileType.KeysToRedeem, Bot.EFileType.KeysToRedeemUnused, Bot.EFileType.KeysToRedeemUsed, Bot.EFileType.MobileAuthenticator];

    internal static void DeleteBot(Bot bot)
    {
        var botName = bot.BotName;
        foreach (var type in FileTypes)
        {
            try
            {
                var filePath = Bot.GetFilePath(botName, type);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                filePath += ".new";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                ASFLogger.LogGenericException(ex);
            }
        }
    }
}