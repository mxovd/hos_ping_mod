using System;
using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

static class TilePingCoordinator
{
    public const byte EventCode = 94;
    const float AllyCacheRefreshInterval = 2f;

    static readonly HashSet<string> _cachedAlliedNicknames = new HashSet<string>();
    static float _lastCacheRefresh;

    public static void RequestPing(TileGO tileGO)
    {
        if (tileGO == null)
        {
            return;
        }

        string senderNickname = GetLocalNickname();

        if (!TilePingService.TryCreatePing(tileGO, false, senderNickname))
        {
            return;
        }

        if (string.IsNullOrEmpty(senderNickname))
        {
            return;
        }

        if (!ShouldBroadcast())
        {
            return;
        }

        short posX = (short)Mathf.Clamp(tileGO.tile.PosX, short.MinValue, short.MaxValue);
        short posY = (short)Mathf.Clamp(tileGO.tile.PosY, short.MinValue, short.MaxValue);

        object[] payload = new object[] { senderNickname, posX, posY };
        global::MultiplayerManager.RaiseEvent(EventCode, payload);
    }

    public static void ReceivePing(object payload)
    {
        object[] data = payload as object[];
        if (data == null || data.Length < 3)
        {
            return;
        }

        string senderNickname = data[0] as string;
        if (string.IsNullOrEmpty(senderNickname))
        {
            return;
        }

        int posX;
        int posY;
        if (!TryReadCoordinate(data[1], out posX) || !TryReadCoordinate(data[2], out posY))
        {
            return;
        }

        if (!CanDisplayPingFrom(senderNickname))
        {
            return;
        }

        var gameData = global::GameData.Instance;
        if (gameData == null || gameData.map == null)
        {
            return;
        }

        if (posX < 0 || posY < 0 || posX >= gameData.map.SizeX || posY >= gameData.map.SizeY)
        {
            return;
        }

        var tile = gameData.map.TilesTable[posX, posY];
        if (tile == null || tile.tileGO == null)
        {
            return;
        }

        TilePingService.TryCreatePing(tile.tileGO, true, senderNickname);
    }

    static bool ShouldBroadcast()
    {
        if (global::MultiplayerManager.Instance == null)
        {
            return false;
        }

        if (!PhotonNetwork.IsConnected)
        {
            return false;
        }

        if (!global::Utils.IsRealtimeMultiplayer())
        {
            return false;
        }

        RefreshAllyCache();
        return _cachedAlliedNicknames.Count > 0;
    }

    static bool CanDisplayPingFrom(string senderNickname)
    {
        var gameData = global::GameData.Instance;
        if (gameData == null || gameData.listOfPlayers == null)
        {
            return false;
        }

        if (!gameData.TryFindLocalPlayer(out var localPlayer))
        {
            return false;
        }

        if (string.Equals(localPlayer.Nickname, senderNickname, StringComparison.Ordinal))
        {
            return true;
        }

        if (!gameData.TryFindPlayerByNickname(senderNickname, out var senderPlayer))
        {
            return false;
        }

        if (senderPlayer.IsComputer)
        {
            return false;
        }

        return localPlayer.IsAlliedWith(senderPlayer);
    }

    static string GetLocalNickname()
    {
        var settings = global::PlayerSettings.Instance;
        if (settings != null && !string.IsNullOrEmpty(settings.Username))
        {
            return settings.Username;
        }

        return null;
    }

    static bool TryReadCoordinate(object value, out int coordinate)
    {
        if (value is short)
        {
            coordinate = (short)value;
            return true;
        }
        if (value is int)
        {
            coordinate = (int)value;
            return true;
        }
        if (value is byte)
        {
            coordinate = (byte)value;
            return true;
        }
        coordinate = 0;
        return false;
    }

    static void RefreshAllyCache()
    {
        if (Time.unscaledTime - _lastCacheRefresh < AllyCacheRefreshInterval)
        {
            return;
        }

        _lastCacheRefresh = Time.unscaledTime;
        _cachedAlliedNicknames.Clear();

        var gameData = global::GameData.Instance;
        if (gameData == null || gameData.listOfPlayers == null)
        {
            return;
        }

        if (!gameData.TryFindLocalPlayer(out var localPlayer))
        {
            return;
        }

        foreach (var player in gameData.listOfPlayers)
        {
            if (player == null || ReferenceEquals(player, localPlayer))
            {
                continue;
            }

            if (player.IsComputer)
            {
                continue;
            }

            if (!localPlayer.IsAlliedWith(player))
            {
                continue;
            }

            if (string.IsNullOrEmpty(player.Nickname))
            {
                continue;
            }

            _cachedAlliedNicknames.Add(player.Nickname);
        }
    }

    [HarmonyPatch(typeof(MultiplayerManager), "ProcessEvent", new[] { typeof(byte), typeof(object) })]
    static class MultiplayerManagerProcessEventPatch
    {
        static void Postfix(byte p_eventCode, object p_object)
        {
            if (p_eventCode == EventCode)
            {
                ReceivePing(p_object);
            }
        }
    }
}
