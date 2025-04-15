using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using State = ValuableDiscoverGraphic.State;

namespace CustomDiscoverStateLib;
using CDS = CustomDiscoverState;

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
    private static int lastAddedIndex = 260; // Last index in ValuableDiscoverGraphic.State enum
    /// <summary>Custom discover graphic state.</summary>
    /// <param name="valuableDiscover">The valuable discover instance.</param>
    /// <param name="physGrabObject">The physGrabObject instance.</param>
    /// <param name="middle">The color of the middle part of the graphic.</param>
    /// <param name="corner">The color of the corner part of the graphic.</param>
    /// <returns>True if the state is applicable, false otherwise.</returns>
    public delegate bool DynamicStateDelegate(ValuableDiscover valuableDiscover, PhysGrabObject physGrabObject, out Color? middle, out Color? corner);
    /// <summary>Custom discover graphic state.</summary>
    /// <param name="valuableDiscover">The valuable discover instance.</param>
    /// <param name="physGrabObject">The physGrabObject instance.</param>
    /// <returns>True if the state is applicable, false otherwise.</returns>
    public delegate bool ConditionalStateDelegate(ValuableDiscover valuableDiscover, PhysGrabObject physGrabObject);
    
    internal static Dictionary<State, ConditionalStateDelegate> conditionalStates = new();
    internal static Dictionary<State, CustomDiscoverGraphic> customStates = new();
    internal static Dictionary<State,DynamicStateDelegate> dynamicStates = new();
    
    /// <summary>Custom discover graphic state.</summary>
    public class CustomDiscoverGraphic {
        public Color ColorMiddle;
        public Color ColorCorner;
    }
    
    /// <summary>Creates a new discover graphic state for manual applying.</summary>
    /// <param name="middle">The color of the middle part of the graphic.</param>
    /// <param name="corner">The color of the corner part of the graphic.</param>
    /// <returns>The state of the new discover graphic.</returns>
    public static State AddNewDiscoverGraphic(Color middle, Color corner) {
        State state = (State)lastAddedIndex;
        lastAddedIndex++;
        customStates[state] = new CustomDiscoverGraphic {
            ColorMiddle = middle,
            ColorCorner = corner
        };
        return state;
    }
    
    /// <summary>Creates a new discover graphic state that is triggered by a condition.</summary>
    /// <param name="condition">The condition that triggers the state.</param>
    /// <param name="middle">The color of the middle part of the graphic.</param>
    /// <param name="corner">The color of the corner part of the graphic.</param>
    /// <returns>The state of the new discover graphic.</returns>
    public static State AddNewConditionalDiscoverGraphic(ConditionalStateDelegate condition, Color middle, Color corner) {
        State state = AddNewDiscoverGraphic(middle, corner);
        conditionalStates[state] = condition;
        return state;
    }
    
    /// <summary>Creates a new discover graphic state that is determined dynamically at runtime.</summary>
    /// <param name="delegate">The delegate that determines the state's colors when evaluated.
    /// This delegate should return true if it can handle the valuable discover, and set the out parameters
    /// with appropriate colors. If it returns false, the delegate is considered not applicable.</param>
    /// <returns>The state of the new dynamic discover graphic. You generally don't want to apply it manually.</returns>
    /// <remarks>
    /// The <see cref="DynamicStateDelegate"/> is evaluated when a valuable is discovered, and allows
    /// for dynamic color determination based on the current game state or valuable properties.
    /// </remarks>
    public static State AddNewDynamicDiscoverGraphic(DynamicStateDelegate @delegate) {
        State state = AddNewDiscoverGraphic(new Color(0,0,0,0), Color.white);
        dynamicStates[state] = @delegate;
        return state;
    }
}
[HarmonyPatch]
internal class Patches {
    [HarmonyPatch(typeof(ValuableDiscover), "New")]
    [HarmonyPrefix]
    public static bool ValuableDiscoverPatch(ValuableDiscover __instance, PhysGrabObject _target, State _state) {
        Color? middle = null;
        Color? corner = null;
        // Check for custom states
        if (!CDS.customStates.TryGetValue(_state, out CDS.CustomDiscoverGraphic customState)) {
            // Check for conditional states
            bool foundDynamic = false;
            foreach (var conditionalState in CDS.conditionalStates) {
                if (conditionalState.Value.Invoke(__instance, _target)) {
                    customState = CDS.customStates[conditionalState.Key];                        
                    foundDynamic = true;
                    middle = customState.ColorMiddle;
                    corner = customState.ColorCorner;
                    break;
                }
            }
            // Check for dynamic states if previous checks failed
            if (!foundDynamic || customState == null) {
                foreach (var dynamicState in CDS.dynamicStates) {
                    if (dynamicState.Value.Invoke(__instance, _target, out middle, out corner)) {
                        _state = dynamicState.Key;
                        foundDynamic = true;
                        break;
                    }
                }
                // If no dynamic state was found, return true to skip the default behavior
                if (!foundDynamic) {
                    return true;
                }
            }
        } else {
            middle = customState.ColorMiddle;
            corner = customState.ColorCorner;
        }
        if (middle == null || corner == null) {
            return true;
        }
        ValuableDiscoverGraphic component = Object.Instantiate<GameObject>(__instance.graphicPrefab, __instance.transform).GetComponent<ValuableDiscoverGraphic>();
        component.target = _target;
        component.state = _state;
        component.middle.GetComponent<Image>().color = (Color)middle;
        component.topLeft.GetComponent<Image>().color = (Color)corner;
        component.topRight.GetComponent<Image>().color = (Color)corner;
        component.botLeft.GetComponent<Image>().color = (Color)corner;
        component.botRight.GetComponent<Image>().color = (Color)corner;
        return false;
    }
}