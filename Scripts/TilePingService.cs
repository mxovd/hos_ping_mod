using UnityEngine;

static class TilePingService
{
    static float _lastPingTime;
    const float PingCooldown = 0.4f;

    public static bool TryCreatePing(TileGO tileGO, bool bypassCooldown = false)
    {
        if (tileGO == null)
        {
            return false;
        }

        if (!bypassCooldown)
        {
            if (Time.unscaledTime - _lastPingTime < PingCooldown)
            {
                return false;
            }

            _lastPingTime = Time.unscaledTime;
        }

        tileGO.HighlightTile();
        return true;
    }
}
