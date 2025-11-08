using UnityEngine;

static class TilePingService
{
    static float _lastPingTime;
    const float PingCooldown = 0.4f;

    public static void TryCreatePing(TileGO tileGO)
    {
        if (tileGO == null)
        {
            return;
        }

        if (Time.unscaledTime - _lastPingTime < PingCooldown)
        {
            return;
        }

        _lastPingTime = Time.unscaledTime;
        tileGO.HighlightTile();
    }
}
