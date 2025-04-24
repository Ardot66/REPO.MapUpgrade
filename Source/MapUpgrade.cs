using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Ardot.REPO.MapUpgrade;

public class MapUpgrade : MonoBehaviour
{
    public static Dictionary<string, int> PlayerUpgrades = new ();
    public static Value Value;
    public static ConfigEntry<bool> MapRequiresUpgrade;
    public static ConfigEntry<float>
        MinCost,
        MaxCost,
        MapSizeIncrease;

    private ItemToggle ItemToggle;
    private PhotonView PhotonView;

    public static void Init()
    {
        MapRequiresUpgrade = Plugin.Config.Bind(
            "Upgrade",
            "MapRequiresUpgrade",
            true,
            "If true, one map upgrade is required before the map can be used"
        );
        MinCost = Plugin.Config.Bind(
            "Upgrade",
            "MinCost",
            7000f,
            "The minimum price of a MapUpgrade in the shop"
        );
        MaxCost = Plugin.Config.Bind(
            "Upgrade",
            "MaxCost",
            10000f,
            "The maximum price of a MapUpgrade in the shop"
        );
        MapSizeIncrease = Plugin.Config.Bind(
            "Upgrade",
            "MapSizeIncrease",
            0.5f,
            "The amount that the map increases in size for every map upgrade"
        );

        Plugin.Config.SettingChanged += (object caller, SettingChangedEventArgs arg) => {
            if(PlayerAvatar.instance == null)
                return;

            if(arg.ChangedSetting == MapRequiresUpgrade || arg.ChangedSetting == MapToolChanges.DefaultSize || arg.ChangedSetting == MapSizeIncrease)
                UpdateUpgrade();  
        };

        Value = ScriptableObject.CreateInstance<Value>();
        UpdateValue();
    }

    public static void UpdateValue()
    {
        Value.valueMin = MinCost.Value / 4f;
        Value.valueMax = MaxCost.Value / 4f;
    }

    public void Awake()
    {
        ItemToggle = GetComponent<ItemToggle>();
        PhotonView = GetComponent<PhotonView>();

        UpdateValue();
        ItemAttributes attributes = GetComponent<ItemAttributes>();
        attributes.item.value = Value;
    }

    [PunRPC]
    public void Upgrade()
    {
        string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID((int)ItemToggle.Get("playerTogglePhotonID")));

        if(!PlayerUpgrades.ContainsKey(steamID))
            PlayerUpgrades.Add(steamID, 0);
        PlayerUpgrades[steamID]++;

        if(SemiFunc.IsMultiplayer() && Utils.IsHost())
            PhotonView.RPC(nameof(Upgrade), RpcTarget.Others);

        UpdateUpgrade();
    }

    public static void UpdateUpgrade()
    {
        for(int x = 0; x < GameDirector.instance.PlayerList.Count; x++)
        {
            PlayerAvatar player = GameDirector.instance.PlayerList[x];

            string steamID = (string)player.Get("steamID");
            if(!PlayerUpgrades.TryGetValue(steamID, out int upgrades))
                upgrades = 0;

            if(!MapRequiresUpgrade.Value)
                upgrades++;

            if(PlayerAvatar.instance == null || PlayerAvatar.instance.mapToolController == null)
                continue;

            MapToolChanges mapTool = PlayerAvatar.instance.mapToolController.GetComponent<MapToolChanges>();

            if(mapTool == null)
                continue;

            mapTool.SetMapEnabled(upgrades > 0);
            mapTool.SetMaxZoom(MapToolChanges.DefaultSize.Value + (upgrades - 1) * MapSizeIncrease.Value);
        }
    }
}