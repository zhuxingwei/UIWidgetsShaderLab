using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public static class ShaderType
    {
        public const int Fill = 0;
        public const int GaussianBlur = 1;
        public const int BlitRT = 2;
        public const int ShadowBox = 3;
        public const int ShadowRBox = 4;
        public const int FillComputeBuffer = 5;
        public const int ClipAlpha = 6;
    }
    
    public abstract class FlushDrawer
    {
        private readonly RenderTexture _renderTexture;
        private readonly int _renderTextureHeight;
        private readonly int _renderTextureWidth;

        protected MonoBehaviour _owner;

        private readonly int _msaaSamples;
        private readonly CommandBuffer _cmdBuf;
        protected int windowHeight => _renderTextureHeight;
        protected int windowWidth => _renderTextureWidth;

        private readonly Dictionary<int, Material> _shaders = new Dictionary<int, Material>();

        private readonly List<int> tempRTs = new List<int>();
        
        private void BuildShaders()
        {
            _shaders[ShaderType.Fill] = new Material(Shader.Find("Custom/Fill"));
            _shaders[ShaderType.GaussianBlur] = new Material(Shader.Find("Custom/GaussianBlur"));
            _shaders[ShaderType.BlitRT] = new Material(Shader.Find("Custom/BlitRT"));
            _shaders[ShaderType.ShadowBox] = new Material(Shader.Find("Custom/ShadowBox"));
            _shaders[ShaderType.ShadowRBox] = new Material(Shader.Find("Custom/ShadowRBox"));
            _shaders[ShaderType.FillComputeBuffer] = new Material(Shader.Find("Custom/FillComputeBuffer"));
            _shaders[ShaderType.ClipAlpha] = new Material(Shader.Find("Custom/ClipAlpha"));
        }

        protected Material GetMaterial(int shaderType)
        {
            Debug.Assert(_shaders.ContainsKey(shaderType), "Invalid Shader Type: " + shaderType);
            return _shaders[shaderType];
        }

        protected void Blit(int? dstLayer, int srcLayer, int dstLayerWidth, int dstLayerHeight, float dstX, float dstY, float dstWidth, float dstHeight,
            float srcU = 0f, float srcV = 0f, float srcWidth = 1f, float srcHeight = 1f)
        {
            if (dstLayer == null)
            {
                cmdBuf.SetRenderTarget(_renderTexture);
            }
            else
            {
                cmdBuf.SetRenderTarget(dstLayer.Value);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
            }
            
            cmdBuf.SetGlobalTexture("_tex", srcLayer);
            
            Vector3[] vertices =
            {
                new Vector3(dstX, dstY, 0),
                new Vector3(dstWidth + dstX, dstY, 0),
                new Vector3(dstX, dstHeight + dstY, 0),
                new Vector3(dstWidth + dstX, dstHeight + dstY, 0)
            };

            Vector2[] uvs =
            {
                new Vector2(srcU, srcV),
                new Vector2(srcU + srcWidth, srcV),
                new Vector2(srcU, srcV + srcHeight),
                new Vector2(srcU + srcWidth, srcV + srcHeight)
            };
            
            int[] triangles = {0, 1, 2, 2, 1, 3};
            
            var material = GetMaterial(ShaderType.BlitRT);
            material.SetVector("_viewport", new Vector4(0, 0, dstLayerWidth, dstLayerHeight));
            var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
            var viewMatrix = Matrix4x4.identity;

            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0);
        }
        
        protected void DrawBox(float centerX, float centerY, float width, float height, Vector4 color, Material material)
        {
            Vector3[] vertices =
            {
                new Vector3(centerX - width / 2, centerY - height / 2, 0),
                new Vector3(centerX + width / 2, centerY - height / 2, 0),
                new Vector3(centerX - width / 2, centerY + height / 2, 0),
                new Vector3(centerX + width / 2, centerY + height / 2, 0)
            };

            Color[] colors =
            {
                color,
                color,
                color,
                color
            };

            int[] triangles = {0, 1, 2, 2, 1, 3};
            var mesh = new Mesh {vertices = vertices, triangles = triangles, colors = colors};

            var viewMatrix = Matrix4x4.identity;
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0);
        }

        protected int RequestTempRT(int width, int height)
        {
            var new_target_id = Shader.PropertyToID("tempRT_" + tempRTs.Count);
            tempRTs.Add(new_target_id);
            
            var desc = new RenderTextureDescriptor(
                width, height,
                RenderTextureFormat.Default, 0)
            {
                useMipMap = false,
                autoGenerateMips = false,
                msaaSamples = _msaaSamples,
            };
            
            cmdBuf.GetTemporaryRT(new_target_id, desc, FilterMode.Bilinear);
            return new_target_id;
        }

        private void ReleaseAllTempRTs()
        {
            foreach (var rtId in tempRTs)
            {
                cmdBuf.ReleaseTemporaryRT(rtId);
            }
            
            tempRTs.Clear();
        }

        public FlushDrawer(int height, int width, int msaaSamples, MonoBehaviour owner = null)
        {
            _renderTextureWidth = width;
            _renderTextureHeight = height;
            _msaaSamples = msaaSamples;
            _owner = owner;
            
            var desc = new RenderTextureDescriptor(
                _renderTextureWidth, _renderTextureHeight,
                RenderTextureFormat.Default, 0)
            {
                useMipMap = false,
                autoGenerateMips = false,
                msaaSamples = _msaaSamples
            };
            
            _renderTexture = new RenderTexture(desc);

            _cmdBuf = new CommandBuffer();
            
            BuildShaders();
        }

        protected CommandBuffer cmdBuf => _cmdBuf;

        void PreDraw()
        {
            _cmdBuf.SetRenderTarget(_renderTexture);
            _cmdBuf.ClearRenderTarget(true, true, Color.clear);
        }

        protected abstract void DoDraw();

        void PostDraw()
        {
            ReleaseAllTempRTs();
            Graphics.ExecuteCommandBuffer(_cmdBuf);
            _cmdBuf.Clear();
        }
        
        public RenderTexture Draw()
        {
            PreDraw();
            DoDraw();
            PostDraw();
            return _renderTexture;
        }
    }
}