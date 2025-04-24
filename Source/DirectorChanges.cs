using HarmonyLib;

namespace Ardot.REPO.MapUpgrade;

public static class DirectorChanges
{
    public static void Init()
    {
        Plugin.Harmony.Patch(
            AccessTools.Method(typeof(LevelGenerator), "GenerateDone"),
            postfix: new HarmonyMethod(typeof(DirectorChanges), nameof(OnLevelGenerated))
        );
        Plugin.Harmony.Patch(
            AccessTools.Method(typeof(StatsManager), "Start"),
            postfix: new HarmonyMethod(typeof(DirectorChanges), nameof(OnStatsManagerStarted))
        );
    }

    private static void OnStatsManagerStarted()
    {
        StatsManager.instance.dictionaryOfDictionaries.Add("playerUpgradeMap", MapUpgrade.PlayerUpgrades);
    }

    private static void OnLevelGenerated()
    {
        MapUpgrade.UpdateUpgrade();
    }
}