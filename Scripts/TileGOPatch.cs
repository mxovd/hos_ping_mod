using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

[HarmonyPatch(typeof(TileGO), "OnMouseOver")]
static class TileGOPatch
{
    // Intercept Alt+clicks so the game does not process them as regular tile interactions.
    [HarmonyPrefix]
    static bool Prefix(TileGO __instance)
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

        TilePingCoordinator.RequestPing(__instance);
        return false;
    }
}

static class PingInputBlocker
{
    static bool _isConsumingClick;
    static bool _blockReleaseFrame;

    public static bool ShouldBlockCurrentFrame()
    {
        if (_blockReleaseFrame)
        {
            _blockReleaseFrame = false;
            return true;
        }

        if (!_isConsumingClick)
        {
            return false;
        }

        if (!InputHelper.GetMouseButton(0))
        {
            _isConsumingClick = false;
            _blockReleaseFrame = true;
            return true;
        }

        return true;
    }

    public static bool TryBeginConsume()
    {
        if (_isConsumingClick)
        {
            return false;
        }

        _isConsumingClick = true;
        return true;
    }
}