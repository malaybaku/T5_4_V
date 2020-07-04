using UnityEngine;

namespace T5_4_V
{
    public class ArmDownPose : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private void Start()
        {
            var r = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var l = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            
            r.localRotation = Quaternion.AngleAxis(-70f, Vector3.forward);
            l.localRotation = Quaternion.AngleAxis(70f, Vector3.forward);
        }
    }
}
