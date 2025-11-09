using UnityEngine;

static class TilePingService
{
    static float _lastPingTime;
    const float PingCooldown = 0.4f;
    const string PingSoundName = "ping_1";

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
        PlayPingSound();
        return true;
    }

    static void PlayPingSound()
    {
        if (SoundManager.instance == null || SoundManager.instance.unit_Source == null)
        {
            return;
        }

        AudioClip clip = SoundManager.GetAttackSound(PingSoundName);
        if (clip == null)
        {
            return;
        }

        SoundManager.instance.unit_Source.PlayOneShot(clip);
    }
}
