using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using UnityEngine.XR.ARFoundation.Samples;

using System;

using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace HumanVFX
{
    public class Skeleton : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The ARHumanBodyManager which will produce body tracking events.")]
        ARHumanBodyManager m_HumanBodyManager;
        
        [SerializeField] public HumanBodyTracker humanbodyTracker;
        [SerializeField] public GameObject bonePrefab;

        [System.Serializable] struct Bone
        {
            public Util.JointIndices JointFrom;
            public Util.JointIndices JointTo;
            public float Radius;

            public Bone(Util.JointIndices from, Util.JointIndices to, float radius)
            {
                JointFrom = from;
                JointTo = to;
                Radius = radius;
            }
        }

        [SerializeField] Bone[] boneArray = new []
        {
            new Bone(Util.JointIndices.Hips,          Util.JointIndices.LeftUpLeg,  1),
            new Bone(Util.JointIndices.LeftUpLeg,  Util.JointIndices.LeftLeg,  1),
            new Bone(Util.JointIndices.LeftLeg,  Util.JointIndices.LeftFoot,      1),
            new Bone(Util.JointIndices.LeftFoot,      Util.JointIndices.LeftToes,      1),

            new Bone(Util.JointIndices.Hips,          Util.JointIndices.RightUpLeg, 1),
            new Bone(Util.JointIndices.RightUpLeg, Util.JointIndices.RightLeg, 1),
            new Bone(Util.JointIndices.RightLeg, Util.JointIndices.RightFoot,     1),
            new Bone(Util.JointIndices.RightFoot,     Util.JointIndices.RightToes,     1),

            new Bone(Util.JointIndices.Hips,          Util.JointIndices.Spine7,         1),
            new Bone(Util.JointIndices.Spine7,         Util.JointIndices.Neck1,          1),
            new Bone(Util.JointIndices.Neck1,          Util.JointIndices.Head,          1),

            new Bone(Util.JointIndices.Neck1,          Util.JointIndices.LeftArm,  1),
            new Bone(Util.JointIndices.LeftArm,  Util.JointIndices.LeftForearm,  1),
            new Bone(Util.JointIndices.LeftForearm,  Util.JointIndices.LeftHand,      1),

            new Bone(Util.JointIndices.Neck1,          Util.JointIndices.RightArm, 1),
            new Bone(Util.JointIndices.RightArm, Util.JointIndices.RightForearm, 1),
            new Bone(Util.JointIndices.RightForearm, Util.JointIndices.RightHand,     1)
        };

        private List<KeyValuePair<Transform, Transform>> boneVFXList =
            new List<KeyValuePair<Transform, Transform>>();
        
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

        void Update()
        {
            //foreach (var bone in boneArray)
            //{
                //var joint1 = _sourceAnimator.GetBoneTransform(bone.JointFrom);
                //var joint2 = _sourceAnimator.GetBoneTransform(bone.JointTo);

                //_vertices.Add(joint1.position);
                //_vertices.Add(joint2.position);
                //_normals.Add(joint1.up);
                //_normals.Add(joint2.up);
                //_texcoords.Add(Vector2.one * bone.Radius);
                //_texcoords.Add(Vector2.one * bone.Radius);
            //}
        }


        void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            //if (_animator == null) { return; }

            foreach (var humanBody in eventArgs.added)
            {
                OnAddHumanBody(humanBody);
                break;
            }

            foreach (var humanBody in eventArgs.updated)
            {
                OnUpdateHumanBody(humanBody);
                break;
            }
        }
        

        void OnAddHumanBody(ARHumanBody humanBody)
        {
            BoneController boneController;

            if (humanbodyTracker.m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                for (int i = 0; i < boneArray.Length; i++)
                {
                    Bone bone = boneArray[i];
                    int jointFromIndex = (int)bone.JointFrom;
                    int jointToIndex = (int)bone.JointTo;
                    Transform jointFromTransform = boneController.m_BoneMapping[jointFromIndex];
                    Transform jointToTransform = boneController.m_BoneMapping[jointToIndex];

                    GameObject boneVFX;
                    if (i == 0) {
                        boneVFX = bonePrefab;
                    } else {
                        boneVFX = Instantiate(bonePrefab) as GameObject;
                    }
                    //boneVFX.parent = bonePrefab.parent;
                    KeyValuePair<Transform, Transform> jointPair = new KeyValuePair<Transform, Transform>(
                        boneVFX.transform.Find("p0"),
                        boneVFX.transform.Find("p1")
                    );
                    jointPair.Key.position = jointFromTransform.position;
                    jointPair.Value.position = jointToTransform.position;
                    
                    boneVFXList.Add(jointPair);
                }
            }
        }

        void OnUpdateHumanBody(ARHumanBody humanBody)
        {
            //var bones = Enum.GetValues(typeof(Util.JointIndices)) as Util.JointIndices[];
            BoneController boneController;

            if (humanbodyTracker.m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                for (int i = 0; i < boneArray.Length; i++)
                {
                    Bone bone = boneArray[i];
                    int jointFromIndex = (int)bone.JointFrom;
                    int jointToIndex = (int)bone.JointTo;
                    Transform jointFromTransform = boneController.m_BoneMapping[jointFromIndex];
                    Transform jointToTransform = boneController.m_BoneMapping[jointToIndex];

                    KeyValuePair<Transform, Transform>  jointPair = boneVFXList[i];
                    jointPair.Key.position = jointFromTransform.position;
                    jointPair.Value.position = jointToTransform.position;

                    //Debug.LogFormat("jointFrom transform = {0}, jointTo transform = {1}", jointFromTransform, jointToTransform);
                    //Debug.LogFormat("jointFrom = {0}, jointTo = {1}", jointFromTransform.position, jointToTransform.position);
                }
            }
        }
    }
}
