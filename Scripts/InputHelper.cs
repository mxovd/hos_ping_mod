using System;
using HarmonyLib;
using UnityEngine;

static class InputHelper
{
    static readonly Type InputType = AccessTools.TypeByName("UnityEngine.Input");
    static readonly Type[] MouseButtonSignature = { typeof(int) };
    static readonly Type[] KeySignature = { typeof(KeyCode) };

    public static bool GetMouseButtonDown(int button)
    {
        if (InputType == null)
        {
            return false;
        }

        var method = AccessTools.Method(InputType, "GetMouseButtonDown", MouseButtonSignature);
        if (method == null)
        {
            return false;
        }

        return method.Invoke(null, new object[] { button }) is bool result && result;
    }

    public static bool GetKey(KeyCode key)
    {
        if (InputType == null)
        {
            return false;
        }

        var method = AccessTools.Method(InputType, "GetKey", KeySignature);
        if (method == null)
        {
            return false;
        }

        return method.Invoke(null, new object[] { key }) is bool result && result;
    }
}
