using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CustomDiscoverStateLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger;
    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private void Awake() {
        Logger = base.Logger;
        harmony.PatchAll(typeof(Patches));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}

public static class CustomDiscoverState {
    public class CustomDiscoverGraphic {
        public Color ColorMiddle;
        public Color ColorCorner;
    }
        
    internal static Dictionary<ValuableDiscoverGraphic.State, CustomDiscoverGraphic> customStates = new Dictionary<ValuableDiscoverGraphic.State, CustomDiscoverGraphic> {};
    private static int lastAddedIndex = 260; // Last index in ValuableDiscoverGraphic.State enum
    public static ValuableDiscoverGraphic.State AddNewDiscoverGraphic(Color middle, Color corner) {
        ValuableDiscoverGraphic.State state = (ValuableDiscoverGraphic.State)lastAddedIndex;
        lastAddedIndex++;
        customStates[state] = new CustomDiscoverGraphic {
            ColorMiddle = middle,
            ColorCorner = corner
        };
        return state;
    }
}
[HarmonyPatch]
internal class Patches {
    [HarmonyPatch(typeof(ValuableDiscover), "New")]
    [HarmonyPrefix]
    public static bool ValuableDiscoverPatch(ValuableDiscover __instance, PhysGrabObject _target, ValuableDiscoverGraphic.State _state) {
        if (!CustomDiscoverState.customStates.TryGetValue(_state, out CustomDiscoverState.CustomDiscoverGraphic customState)) {
            return true;
        }
        ValuableDiscoverGraphic component = Object.Instantiate<GameObject>(__instance.graphicPrefab, __instance.transform).GetComponent<ValuableDiscoverGraphic>();
        component.target = _target;
        component.state = _state;
        component.middle.GetComponent<Image>().color = customState.ColorMiddle;
        component.topLeft.GetComponent<Image>().color = customState.ColorCorner;
        component.topRight.GetComponent<Image>().color = customState.ColorCorner;
        component.botLeft.GetComponent<Image>().color = customState.ColorCorner;
        component.botRight.GetComponent<Image>().color = customState.ColorCorner;
        return false;
    }
}