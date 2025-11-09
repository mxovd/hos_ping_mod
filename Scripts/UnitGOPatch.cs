using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

[HarmonyPatch(typeof(UnitGO), "OnMouseOver")]
static class UnitGOPatch
{
    // Prevent Alt+click on units from triggering selection or other interactions.
    [HarmonyPrefix]
    static bool Prefix(UnitGO __instance)
    {
        if (PingInputBlocker.ShouldBlockCurrentFrame())
        {
            return false;
        }

        if (!InputHelper.GetMouseButtonDown(0))
        {
            return true;
        }

        bool altHeld = InputHelper.GetKey(KeyCode.LeftAlt) || InputHelper.GetKey(KeyCode.RightAlt);
        if (!altHeld)
        {
            return true;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        if (!PingInputBlocker.TryBeginConsume())
        {
            return false;
        }

        if (__instance != null && __instance.tileGO != null)
        {
            TilePingCoordinator.RequestPing(__instance.tileGO);
        }

        return false;
    }
}
