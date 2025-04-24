using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;

namespace Ardot.REPO.MapUpgrade;

public class MapToolChanges : MonoBehaviour
{
    public static MapToolChanges Instance;
    public static ConfigEntry<float> 
        DefaultSize,
        ZoomRate;

    public static void Init()
    {
        ZoomRate = Plugin.Config.Bind(
            "Map",
            "ZoomSpeed",
            1f,
            "Controls the speed that the map is zoomed"
        );
        DefaultSize = Plugin.Config.Bind(
            "Map",
            "DefaultSize",
            1f,
            "The starting size of the map"
        );

        Plugin.Harmony.Patch(
            AccessTools.Method(typeof(PlayerAvatar), "Awake"),
            postfix: new HarmonyMethod(typeof(MapToolChanges), nameof(PlayerAwakePostfix))
        );
    }

    public static void PlayerAwakePostfix(PlayerAvatar __instance)
    {
        __instance.mapToolController.gameObject.AddComponent<MapToolChanges>();
    }

    public MapToolController MapTool;
    public Camera MapCamera;
    public record struct BackgroundObject(Transform Object, Vector3 OriginalScale);
    public BackgroundObject[] BackgroundObjects = new BackgroundObject[3];

    public const float ZoomScale = 2.25f;
    public float MaxZoom = 1f;
    public bool MapEnabled = true;

    public void Awake()
    {
        MapTool = GetComponent<MapToolController>();

        int backgroundObjectIndex = 0;
        Utils.ForObjectsInTree(Map.Instance.transform.parent, transform => {
            Camera camera = transform.GetComponent<Camera>();

            if(camera != null)
                MapCamera = camera;

            switch(transform.name)
            {
                case "Fog":
                case "Scanlines":
                case "Background":
                {
                    BackgroundObjects[backgroundObjectIndex] = new BackgroundObject(transform, transform.localScale);
                    backgroundObjectIndex++;
                    break;
                }
                case "Completed":
                    return false;
            }

            return true;
        });

        SetZoom(DefaultSize.Value);
    }

    public void Update()
    {
        if(!MapEnabled || !(bool)MapTool.Get("Active"))
            return;

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        SetZoom(Mathf.Clamp(MapCamera.orthographicSize / ZoomScale + scroll * ZoomRate.Value, 0.5f, MaxZoom));
    }

    public void SetMapEnabled(bool enabled)
    {
        MapTool.DisplayMesh.enabled = enabled;
        MapTool.DisplayMesh.transform.parent.GetComponentInChildren<Light>().enabled = enabled;
        MapEnabled = enabled;
    }

    public void SetMaxZoom(float zoom)
    {
        if(Mathf.Abs(zoom - MaxZoom) > 0.1f)
            SetZoom(zoom);

        MaxZoom = zoom;
    }

    public void SetZoom(float zoom)
    {
        MapCamera.orthographicSize = zoom * ZoomScale;

        for(int x = 0; x < BackgroundObjects.Length; x++)
        {
            Vector3 originalScale = BackgroundObjects[x].OriginalScale;
            BackgroundObjects[x].Object.localScale = new Vector3(originalScale.x * zoom, originalScale.y, originalScale.z * zoom);
        }
    }
}