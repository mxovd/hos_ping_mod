using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class TilePingService
{
    static float _lastPingTime;
    const float PingCooldown = 0.4f;
    const string PingSoundName = "ping_1";

    static readonly Color[] PlayerPingColors =
    {
        new Color(0.90f, 0.25f, 0.22f), // red
        new Color(0.27f, 0.63f, 0.94f), // blue
        new Color(0.35f, 0.83f, 0.33f), // green
        new Color(0.96f, 0.75f, 0.21f), // yellow
        new Color(0.93f, 0.38f, 0.88f), // magenta
        new Color(0.29f, 0.92f, 0.82f), // teal
        new Color(0.96f, 0.48f, 0.19f), // orange
        new Color(0.60f, 0.45f, 0.89f)  // purple
    };

    static readonly Dictionary<string, Color> PingColorAssignments = new Dictionary<string, Color>(StringComparer.Ordinal);
    static int _nextPlayerColorIndex;
    static readonly Color DefaultPingColor = new Color(0.73f, 0.79f, 0.94f);
    static GameObject _highlightPrefab;

    public static bool TryCreatePing(TileGO tileGO, bool bypassCooldown = false, string senderName = null)
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

        Color highlightColor = ResolvePingColor(tileGO, senderName);
        SpawnPingHighlight(tileGO, highlightColor);
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

    static void SpawnPingHighlight(TileGO tileGO, Color color)
    {
        if (tileGO == null)
        {
            return;
        }

        tileGO.StartCoroutine(HighlightRoutine(tileGO, color));
    }

    static IEnumerator HighlightRoutine(TileGO tileGO, Color color)
    {
        if (tileGO == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.3f);

        if (tileGO == null)
        {
            yield break;
        }

        GameObject prefab = GetHighlightPrefab();
        if (prefab == null)
        {
            yield break;
        }

        GameObject highlightObj = UnityEngine.Object.Instantiate(prefab);
        if (highlightObj == null)
        {
            yield break;
        }

        Transform tileTransform = tileGO.transform;
        if (tileTransform == null)
        {
            UnityEngine.Object.Destroy(highlightObj);
            yield break;
        }

        highlightObj.transform.position = new Vector3(tileTransform.position.x, tileTransform.position.y, -0.48f);

        var spriteRenderer = highlightObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            UnityEngine.Object.Destroy(highlightObj);
            yield break;
        }

        Color transparent = new Color(color.r, color.g, color.b, 0f);
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            spriteRenderer.color = Color.Lerp(transparent, color, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = color;

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            spriteRenderer.color = Color.Lerp(color, transparent, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = transparent;
        UnityEngine.Object.Destroy(highlightObj);
    }

    static GameObject GetHighlightPrefab()
    {
        if (_highlightPrefab == null)
        {
            _highlightPrefab = Resources.Load<GameObject>("Prefabs/Highlight ring");
        }
        return _highlightPrefab;
    }

    static Color ResolvePingColor(TileGO tileGO, string senderName)
    {
        if (!string.IsNullOrEmpty(senderName))
        {
            return AssignColorForIdentifier(senderName);
        }

        var ownerName = tileGO?.tile?.ownerPlayer?.Name;
        if (!string.IsNullOrEmpty(ownerName))
        {
            return AssignColorForIdentifier(ownerName);
        }

        return DefaultPingColor;
    }

    static Color AssignColorForIdentifier(string identifier)
    {
        if (PingColorAssignments.TryGetValue(identifier, out var color))
        {
            return color;
        }

        color = SelectColorForIdentifier(identifier);
        PingColorAssignments[identifier] = color;
        return color;
    }

    static Color SelectColorForIdentifier(string identifier)
    {
        var gameData = GameData.Instance;
        if (gameData != null && gameData.listOfPlayers != null)
        {
            for (int i = 0; i < gameData.listOfPlayers.Count; i++)
            {
                Player player = gameData.listOfPlayers[i];
                if (player != null && !string.IsNullOrEmpty(player.Name) && string.Equals(player.Name, identifier, StringComparison.Ordinal))
                {
                    if (PlayerPingColors.Length > 0)
                    {
                        return PlayerPingColors[i % PlayerPingColors.Length];
                    }
                }
            }
        }

        if (PlayerPingColors.Length == 0)
        {
            return DefaultPingColor;
        }

        Color color = PlayerPingColors[_nextPlayerColorIndex % PlayerPingColors.Length];
        _nextPlayerColorIndex = (_nextPlayerColorIndex + 1) % PlayerPingColors.Length;
        return color;
    }

}
