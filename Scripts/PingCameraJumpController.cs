using HarmonyLib;
using UnityEngine;

static class PingCameraJumpController
{
    static Vector3? _lastReceivedWorldPosition;
    static PingCameraJumpBehaviour _behaviour;

    public static void RecordReceivedPing(TileGO tileGO)
    {
        if (tileGO == null)
        {
            return;
        }

        Transform tileTransform = tileGO.transform;
        if (tileTransform == null)
        {
            return;
        }

        Vector3 position = tileTransform.position;
        _lastReceivedWorldPosition = new Vector3(position.x, position.y, position.z);

        if (UIManager.instance != null)
        {
            EnsureListener(UIManager.instance);
        }
    }

    public static bool TryGetLastReceivedPing(out Vector3 worldPosition)
    {
        if (_lastReceivedWorldPosition.HasValue)
        {
            worldPosition = _lastReceivedWorldPosition.Value;
            return true;
        }

        worldPosition = default;
        return false;
    }

    public static void EnsureListener(UIManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (_behaviour != null && _behaviour.gameObject == manager.gameObject)
        {
            return;
        }

        var existing = manager.GetComponent<PingCameraJumpBehaviour>();
        if (existing == null)
        {
            existing = manager.gameObject.AddComponent<PingCameraJumpBehaviour>();
        }

        _behaviour = existing;
    }

    sealed class PingCameraJumpBehaviour : MonoBehaviour
    {
        float _lastJumpTime;

        void OnDestroy()
        {
            if (_behaviour == this)
            {
                _behaviour = null;
            }
        }

        void Update()
        {
            if (!PingCameraJumpController.TryGetLastReceivedPing(out var targetPosition))
            {
                return;
            }

            bool altHeld = InputHelper.GetKey(KeyCode.LeftAlt) || InputHelper.GetKey(KeyCode.RightAlt);
            if (!altHeld)
            {
                return;
            }

            bool shiftHeld = InputHelper.GetKey(KeyCode.LeftShift) || InputHelper.GetKey(KeyCode.RightShift);
            if (!shiftHeld)
            {
                return;
            }

            bool altPressedThisFrame = InputHelper.GetKeyDown(KeyCode.LeftAlt) || InputHelper.GetKeyDown(KeyCode.RightAlt);
            bool shiftPressedThisFrame = InputHelper.GetKeyDown(KeyCode.LeftShift) || InputHelper.GetKeyDown(KeyCode.RightShift);
            if (!altPressedThisFrame && !shiftPressedThisFrame)
            {
                return;
            }

            if (Time.unscaledTime - _lastJumpTime < 0.15f)
            {
                return;
            }

            var camera = Camera.main;
            var manager = UIManager.instance;
            if (camera == null || manager == null)
            {
                return;
            }

            _lastJumpTime = Time.unscaledTime;
            Vector3 start = camera.transform.position;
            Vector3 destination = new Vector3(targetPosition.x, targetPosition.y, start.z);
            StartCoroutine(manager.CenterCameraOnUnitCoroutine(start, destination, 0.2f));
            StartCoroutine(manager.HighlighTile(targetPosition.x, targetPosition.y));
        }
    }

    [HarmonyPatch(typeof(UIManager), "Awake")]
    static class UIManagerAwakePatch
    {
        static void Postfix(UIManager __instance)
        {
            EnsureListener(__instance);
        }
    }
}
