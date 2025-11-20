namespace ASFUtils.Data;

/// <summary>
/// 
/// </summary>
/// <param name="EULA"></param>
/// <param name="Statistic"></param>
/// <param name="DeleteWhenFarmed">挂完卡删除机器人</param>
/// <param name="DisableFarming">禁用挂卡</param>

public sealed record PluginConfig(
    bool EULA,
    bool Statistic = true,
    bool DeleteWhenFarmed = false,
    bool DisableFarming = false
);
