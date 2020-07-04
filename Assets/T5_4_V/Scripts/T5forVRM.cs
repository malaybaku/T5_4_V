using UnityEngine;
using Tobii.Gaming;
using VRM;

namespace T5_4_V
{
    /// <summary>
    /// Move character face pose and eyes with Tobii Eye Tracker
    /// </summary>
    public class T5forVRM : MonoBehaviour
    {
        [SerializeField] private bool isMirrored = false;
        [SerializeField] private float responsiveness = 10f;
        [SerializeField] private Animator animator;
        [SerializeField] private VRMBlendShapeProxy blendShape;
        [Range(0, 1)] [SerializeField] private float eyeRotationApplyRate = 0.2f;
        [SerializeField] private float eyeOpenRate = 5f;
        [SerializeField] private float eyeCloseJudgeLimitTime = 0.1f;
        
        private static readonly BlendShapeKey lBlinkKey = new BlendShapeKey(BlendShapePreset.Blink_L);
        private static readonly BlendShapeKey rBlinkKey = new BlendShapeKey(BlendShapePreset.Blink_R);
        
        private Camera _cam;
        private Transform _root;
        private Transform _head;
        private Transform _leftEye;
        private Transform _rightEye;
        
        private bool _eyeClosed;
        private float _blinkValue;
        
        private void Start()
        {
            _cam = Camera.main;
            _root = animator.transform;
            _head = animator.GetBoneTransform(HumanBodyBones.Head);
            _leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            _rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            var headPose = TobiiAPI.GetHeadPose();
            if (headPose.IsRecent())
            {
                var rot = headPose.Rotation;
                if (isMirrored)
                {
                    rot.y *= -1;
                    rot.z *= -1;
                }
                _head.localRotation = Quaternion.Lerp(
                    _head.localRotation, 
                    rot,
                    Time.deltaTime * responsiveness
                    );
                
                var pos = headPose.Position;
                // Debug.Log($"Head Pos = {pos.x:0.00},{pos.y:0.00},{pos.z:0.00}");
                _root.localPosition = new Vector3(
                    pos.x * (-0.001f),
                    pos.y * 0.001f,
                    0
                    );
            }

            var gazePoint = TobiiAPI.GetGazePoint();
            if (gazePoint.IsRecent())
            {
                var fov = _cam.fieldOfView;
                var mirrorFactor = isMirrored ? -1f : 1f;
                var eyeRotation = Quaternion.Euler(
                    (gazePoint.Viewport.y - 0.5f) * fov * eyeRotationApplyRate * (-1),
                    (gazePoint.Viewport.x - 0.5f) * fov * _cam.aspect * eyeRotationApplyRate * mirrorFactor,
                    0
                    );

                _leftEye.localRotation = eyeRotation;
                _rightEye.localRotation = eyeRotation;
            }

            _eyeClosed = 
                TobiiAPI.GetUserPresence().IsUserPresent() && (Time.unscaledTime - gazePoint.Timestamp) > eyeCloseJudgeLimitTime ||
                !gazePoint.IsRecent();

            if (_eyeClosed)
            {
                _blinkValue = 1.0f;
            }
            else
            {
                _blinkValue = Mathf.Max(_blinkValue - eyeOpenRate * Time.deltaTime, 0f);
            }
            
            blendShape.AccumulateValue(lBlinkKey, _blinkValue);
            blendShape.AccumulateValue(rBlinkKey, _blinkValue);
            blendShape.Apply();
        }
    }
}
