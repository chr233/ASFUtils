using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;

using ASFUtils.Core;
using ASFUtils.Data;

using SteamKit2;

using System.ComponentModel;
using System.Composition;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ASFUtils;

[Export(typeof(IPlugin))]
internal sealed class ASFUtils : IASF, IPlugin, IBotCommand2, IBotCardsFarmerInfo, IBotConnection
{
    private bool ASFEBridge;

    private Timer? StatisticTimer;

    /// <summary>
    /// 获取插件信息
    /// </summary>
    private string PluginInfo => $"{Name} {Version}";

    public string Name => "ASF Utils";

    public Version Version => MyVersion;

    /// <summary>
    /// ASF启动事件
    /// </summary>
    /// <param name="additionalConfigProperties"></param>
    /// <returns></returns>
    public async Task OnASFInit(IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties = null)
    {
        PluginConfig? config = null;

        if (additionalConfigProperties != null)
        {
            foreach (var (configProperty, configValue) in additionalConfigProperties)
            {
                if (configProperty != "ASFEnhance" || configValue.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                try
                {
                    config = configValue.ToJsonObject<PluginConfig>();
                    if (config != null)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ASFLogger.LogGenericException(ex);
                }
            }
        }

        Config = config ?? new PluginConfig(false, false);

        var sb = new StringBuilder();

        //使用协议
        if (!Config.EULA)
        {
            sb.AppendLine();
            sb.AppendLine(Langs.Line);
            sb.AppendLineFormat(Langs.EulaWarning, Name);
            sb.AppendLine(Langs.Line);
        }


    }

    /// <summary>
    /// 插件加载事件
    /// </summary>
    /// <returns></returns>
    public Task OnLoaded()
    {
        ASFLogger.LogGenericInfo(Langs.PluginContact);
        ASFLogger.LogGenericInfo(Langs.PluginInfo);

        const BindingFlags flag = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var handler = typeof(ASFUtils).GetMethod(nameof(ResponseCommand), flag);

        const string pluginId = nameof(ASFUtils);
        const string cmdPrefix = "ASFU";
        const string repoName = "ASFUtils";

        ASFEBridge = AdapterBridge.InitAdapter(Name, pluginId, cmdPrefix, repoName, handler);

        if (ASFEBridge)
        {
            ASFLogger.LogGenericDebug(Langs.ASFEnhanceRegisterSuccess);
        }
        else
        {
            ASFLogger.LogGenericInfo(Langs.ASFEnhanceRegisterFailed);
            ASFLogger.LogGenericWarning(Langs.PluginStandalongMode);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 处理命令事件
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="access"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <param name="steamId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidEnumArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamId = 0)
    {
        if (ASFEBridge)
        {
            return null;
        }

        if (!Enum.IsDefined(access))
        {
            throw new InvalidEnumArgumentException(nameof(access), (int)access, typeof(EAccess));
        }

        try
        {
            var cmd = args[0].ToUpperInvariant();

            if (cmd.StartsWith("ASFU."))
            {
                cmd = cmd[5..];
            }

            var task = ResponseCommand(bot, access, cmd, args);
            if (task != null)
            {
                return await task.ConfigureAwait(false);
            }

            return null;
        }
        catch (Exception ex)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                ASFLogger.LogGenericException(ex);
            }).ConfigureAwait(false);

            return ex.StackTrace;
        }
    }

    /// <summary>
    /// 处理命令
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="access"></param>
    /// <param name="cmd"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private Task<string>? ResponseCommand(Bot bot, EAccess access, string cmd, string[] args)
    {
        var argLength = args.Length;

        return argLength switch
        {
            0 => throw new InvalidOperationException(nameof(args)),
            1 => cmd switch //不带参数
            {
                //插件信息
                "ASFUTILS" or
                "ASFU" when access >= EAccess.FamilySharing => Task.FromResult(PluginInfo),

#if DEBUG
                "TEST" when access >= EAccess.Master => Command.ResponseTest(bot),
#endif

                _ => null
            },
            _ => cmd switch //带参数
            {
#if DEBUG
                "TEST" when access >= EAccess.Master => Command.ResponseTest(Utilities.GetArgsAsText(args, 1, ",")),
#endif

                _ => null
            }
        };
    }

    /// <inheritdoc/>
    public Task OnBotFarmingFinished(Bot bot, bool farmedSomething)
    {
        if (Config.DeleteWhenFarmed && !farmedSomething)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000).ConfigureAwait(false);
                Utils.DeleteBot(bot);
                ASFLogger.LogGenericWarning($"已删除挂完卡的机器人 {bot.BotName}");
            });
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnBotFarmingStarted(Bot bot)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnBotFarmingStopped(Bot bot)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnBotLoggedOn(Bot bot)
    {
        if (Config.DisableFarming)
        {
            return bot.Actions.Pause(true);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnBotDisconnected(Bot bot, EResult reason)
    {
        return Task.CompletedTask;
    }
}