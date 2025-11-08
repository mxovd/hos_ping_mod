using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

[HarmonyPatch(typeof(TileGO), "OnMouseOver")]
static class TileGOPatch
{
    // Runs after the game handles TileGO.OnMouseOver.
    [HarmonyPostfix]
    static void Postfix(TileGO __instance)
    {
        if (!InputHelper.GetMouseButtonDown(0))
        {
            return;
        }

        bool altHeld = InputHelper.GetKey(KeyCode.LeftAlt) || InputHelper.GetKey(KeyCode.RightAlt);
        if (!altHeld)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        TilePingCoordinator.RequestPing(__instance);
    }
}