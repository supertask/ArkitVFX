// Ref. https://edom18.hateblo.jp/entry/2019/08/11/223803

using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace HumanVFX
{

    public static class ShaderID
    {
        public static readonly int TextureY = Shader.PropertyToID("_textureY");
        public static readonly int TextureCbCr = Shader.PropertyToID("_textureCbCr");
        public static readonly int HumanStencil = Shader.PropertyToID("_HumanStencil");
        public static readonly int EnvironmentDepth = Shader.PropertyToID("_EnvironmentDepth");
        public static readonly int DepthRange = Shader.PropertyToID("_DepthRange");
        public static readonly int AspectFix = Shader.PropertyToID("_AspectFix");
    }

    public class PeopleOcclusion : MonoBehaviour
    {
        [SerializeField, Tooltip("The ARHumanBodyManager which will produce frame events.")]
        private ARHumanBodyManager _humanBodyManager;

        [SerializeField]
        private Material _material = null;

        //[SerializeField] ARCameraManager _cameraManager = null;
        private ARCameraBackground _cameraBackground = null;
        private AROcclusionManager _occlusionManager = null;

        private RenderTexture _captureTexture = null;

        public ARHumanBodyManager HumanBodyManager
        {
            get { return _humanBodyManager; }
            set { _humanBodyManager = value; }
        }

        #region ### MonoBehaviour ###

        private void Awake()
        {
            Camera camera = this.GetComponent<Camera>();
            this._cameraBackground = GetComponent<ARCameraBackground>();
            this._occlusionManager = GetComponent<AROcclusionManager>();
        }
        
        void OnEnable()
        {
            this.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            this._captureTexture = new RenderTexture(Screen.width, Screen.height, 0);

            // Camera callback setup
            //_cameraManager.frameReceived += OnCameraFrameReceived;
            this._occlusionManager.frameReceived += OnOcclusionFrameReceived;
        }

        void OnDisable()
        {
            // Camera callback termination
            //_cameraManager.frameReceived -= OnCameraFrameReceived;
            _occlusionManager.frameReceived -= OnOcclusionFrameReceived;
            if (_captureTexture != null) {
                _captureTexture.Release();
                _captureTexture = null;
            }
        }
        #endregion ### MonoBehaviour ###

        /*
        void OnCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            // We expect there is at least one texture.
            if (args.textures.Count == 0) return;

            // Try receiving Y/CbCr textures.
            for (var i = 0; i < args.textures.Count; i++)
            {
                var id = args.propertyNameIds[i];
                var tex = args.textures[i];
                if (id == ShaderID.TextureY)
                    _material.SetTexture(ShaderID.TextureY, tex);
                else if (id == ShaderID.TextureCbCr)
                    _material.SetTexture(ShaderID.TextureCbCr, tex);
            }
        }
        */

        void OnOcclusionFrameReceived(AROcclusionFrameEventArgs args)
        {
            // Try receiving stencil/depth textures.
            for (var i = 0; i < args.textures.Count; i++)
            {
                var id = args.propertyNameIds[i];
                var tex = args.textures[i];
                if (id == ShaderID.HumanStencil)
                    _material.SetTexture(ShaderID.HumanStencil, tex);
                else if (id == ShaderID.EnvironmentDepth)
                    _material.SetTexture(ShaderID.EnvironmentDepth, tex);
            }
        }

        private void LateUpdate()
        {
            if (_cameraBackground.material != null)
            {
                Graphics.Blit(null, _captureTexture, _cameraBackground.material);
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, _material);
        }
    }        

} // namespace HumanVFX