using UnityEngine;

// Legacy shim so old prefabs that referenced PlayerRowUI keep working.
// Requires the new binder so rows still populate.
[RequireComponent(typeof(PlayerRowBinder))]
public class PlayerRowUI : MonoBehaviour { }
