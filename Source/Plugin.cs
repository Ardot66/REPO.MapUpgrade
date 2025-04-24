using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using REPOLib;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;


namespace Ardot.REPO.MapUpgrade;

[BepInPlugin(PluginGUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
#pragma warning disable BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
public class Plugin : BaseUnityPlugin
#pragma warning restore BepInEx002 // Classes with BepInPlugin attribute must inherit from BaseUnityPlugin
{
    public const string PluginGUID = "Ardot.REPO.MapUpgrade";

    public new static ConfigFile Config;
    public static Harmony Harmony;
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Config = base.Config;
        Harmony = new (PluginGUID);

        DirectorChanges.Init();
        MapUpgrade.Init();
        MapToolChanges.Init();
    }
}