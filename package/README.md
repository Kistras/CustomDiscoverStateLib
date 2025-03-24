## What does it do
This library allows you to create custom colored highlights for ValuableDiscoverGraphic (the thing that appears when you see a valuable or a dead friend for a first time). Doesn't do anything by itself, other mods are meant to use it.

GUID: `Kistras-CustomDiscoverStateLib`

## Usage
```cs
using CustomDiscoverStateLib;
...
// Better store it somewhere and create once to not clutter the state list
ValuableDiscoverGraphic.State customState = CustomDiscoverState.AddNewDiscoverGraphic(
    middle: new Color(0.8f, 0.0f, 0.5f, 0.075f), 
    corner: new Color(0.9f, 0.1f, 0.6f, 0.75f));

// Display that custom DiscoverGraphic on some valuable object
ValuableObject someValuable = FindObjectsOfType<ValuableObject>().First();
if (someValuable) {
    ValuableDiscover.instance.New(someValuable.physGrabObject, customState);
}
```
<details>
  <summary>Expand to see what happens</summary>
  <img src="https://i.imgur.com/Vh39siZ.png" alt="A thingum hath been enlumined"/>
</details>
Don't forget to list this library as dependency:

```cs
[BepInDependency("Kistras-CustomDiscoverStateLib")]
public class Plugin : BaseUnityPlugin {
    ...
}
```

## Base game colors (for reference)
```cs
ValuableDiscoverGraphic.State baseGameReminder = CustomDiscoverState.AddNewDiscoverGraphic(
    middle: new Color(0.642f, 0.619f, 0.481f, 0.039f), 
    corner: new Color(0.642f, 0.619f, 0.481f, 0.039f));
ValuableDiscoverGraphic.State baseGameBad = CustomDiscoverState.AddNewDiscoverGraphic(
    middle: new Color(1f, 0.0f, 0.067f, 0.059f), 
    corner: new Color(1f, 0.1f, 0.067f, 0.059f));
ValuableDiscoverGraphic.State baseGameDiscover = CustomDiscoverState.AddNewDiscoverGraphic(
    middle: new Color(1f, 0.863f, 0f, 0.118f), 
    corner: new Color(1f, 0.863f, 0f, 1f));
```

## Why make this?
Because I've seen two mods already that implement this functionality and [I'm in the process of making the third one](https://thunderstore.io/c/repo/p/Kistras/Valuables_Scanner/). So making a library seemed like a right choice