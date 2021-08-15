using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using UnityEngine.XR.ARFoundation.Samples;

namespace HumanVFX
{
    public class RibbonBinder : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The ARHumanBodyManager which will produce body tracking events.")]
        ARHumanBodyManager m_HumanBodyManager;

        [SerializeField] public HumanBodyTracker humanbodyTracker;

        public static List<Vector2Int> SELECTED_JOINTS = new List<Vector2Int>() {
            new Vector2Int((int)Util.JointIndices.Spine1, (int)Util.JointIndices.Spine7), //背中
            new Vector2Int((int)Util.JointIndices.RightArm, (int)Util.JointIndices.RightForearm), //右肩
            new Vector2Int((int)Util.JointIndices.RightForearm, (int)Util.JointIndices.RightHandMidEnd ) //右腕
        };

        void OnEnable()
        {
            Debug.Assert(m_HumanBodyManager != null, "Human body manager is required.");
            m_HumanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
        }

        void OnDisable()
        {
            if (m_HumanBodyManager != null)
                m_HumanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
        }

        void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            BoneController boneController;
            foreach (var humanBody in eventArgs.updated)
            {
                //Debug.Log($"Adding a new skeleton [{humanBody.trackableId}].");
                if (humanbodyTracker.m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
                {
                    int numOfJoints = humanBody.joints.Length;
                    for (int i = 0; i < numOfJoints; ++i) {
                        XRHumanBodyJoint joint = humanBody.joints[i];
                        //Debug.LogFormat("joint index: i = {0}, joint.index = {1}", i, joint.index);
                        //Debug.Log(boneController.m_BoneMapping[i]);
                        //Debug.Log("pos: " + joint.localPose.position);

                        //if (index >= 0 && index < k_NumSkeletonJoints) {
                        //}
                    }
                }
                break; //Only one human
            }
        }
        
        /*
        // Returns the integer value corresponding to the JointIndices enum value
        // passed in as a string.
        int GetJointIndex(string jointName)
        {
            Uitl.JointIndices val;
            if (Enum.TryParse(jointName, out val))
            {
                return (int)val;
            }
            return -1;
        }
        */
        

    }
}