using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace HumanVFX {
    [ExecuteInEditMode]
    public class TransformUpdater : MonoBehaviour
    {

        public static Vector3 VFX_ROTATION_DIR = Vector3.up;
        public static class ShaderID
        {
            public static int BoneCenter = Shader.PropertyToID("boneCenter");
            public static int BoneNormDirection = Shader.PropertyToID("boneNormDirection");
            public static int BoneLength = Shader.PropertyToID("boneLength");
        }
        

        [SerializeField] public Transform boneBegin;
        [SerializeField] public Transform boneEnd;

        // ReadOnly properties from outside
        public Vector3 boneCenter { get; private set; }
        public Vector3 boneNormDirection { get; private set; }
        public Quaternion boneRotation { get; private set; }
        public float boneLength { get; private set; }

        private VisualEffect vfx;

        // Start is called before the first frame update
        void Start()
        {
            this.vfx = this.transform.GetComponent<VisualEffect>();
        }

        // Update is called once per frame
        void Update()
        {
            if (this.vfx == null) {
                this.vfx = this.transform.GetComponent<VisualEffect>();
            }
            Vector3 boneDirection = boneEnd.position - boneBegin.position;
            this.boneCenter = Vector3.Lerp(boneBegin.position, boneEnd.position, 0.5f);
            this.boneNormDirection = Vector3.Normalize(boneDirection);
            this.boneRotation = Quaternion.FromToRotation(VFX_ROTATION_DIR, boneNormDirection);
            this.boneLength = Vector3.Distance(boneBegin.position, boneEnd.position);

            this.transform.position = boneCenter;
            this.transform.rotation = boneRotation;
            this.vfx.SetFloat(ShaderID.BoneLength, boneLength);
        }
    }
}