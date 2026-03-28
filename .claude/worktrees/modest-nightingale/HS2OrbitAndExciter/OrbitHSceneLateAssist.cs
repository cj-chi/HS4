using UnityEngine;

namespace HS2OrbitAndExciter
{
    /// <summary>
    /// Applies H-scene flags and checkpoint assist after game proc runs (same frame), without breaking camera orbit math.
    /// OrbitController runs at -100 and only writes CamDat.Rot; CameraControl_Ver2 applies transBase in LateUpdate(0).
    /// </summary>
    [DefaultExecutionOrder(32000)]
    public class OrbitHSceneLateAssist : MonoBehaviour
    {
        private OrbitController? _orbit;

        private void Awake()
        {
            _orbit = GetComponent<OrbitController>();
        }

        private void LateUpdate()
        {
            if (_orbit == null)
                _orbit = GetComponent<OrbitController>();
            if (_orbit == null || !OrbitController.IsOrbitActive())
                return;
            var hScene = OrbitController.TryGetHScene();
            if (hScene == null)
                return;
            _orbit.RunLateHSceneAssist(hScene);
        }
    }
}
