using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public class TestAlphaClip : FlushDrawer
    {
        public TestAlphaClip(int height, int width, int msaaSamples, MonoBehaviour owner) : base(height, width, msaaSamples, owner)
        {
            
        }


        void DrawCachePicture()
        {
            var size = 30;
            var delta = 5;
            var desc = new RenderTextureDescriptor(
                size,
                size,
                RenderTextureFormat.Default,
                24
            )
            {
                useMipMap = false,
                autoGenerateMips = false
            };
            
            maskTex = new RenderTexture(desc);

            var cacheCmd = new CommandBuffer();
            
            cacheCmd.SetRenderTarget(maskTex);
            cacheCmd.ClearRenderTarget(false, true, Color.clear);
            
            var material = GetMaterial(ShaderType.ShadowRBox);
            
            Vector3[] vertices =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            int[] triangles =
            {
                0, 1, 2, 2, 1, 3
            };
            
            var mesh = new Mesh {vertices = vertices, triangles = triangles};
            material.SetVector("box", new Vector4(delta, delta, size - delta, size - delta));
            material.SetVector("window", new Vector2(size, size));
            material.SetFloat("sigma", 3f);
            material.SetVector("color", new Vector4(1, 1, 1, 1));
            material.SetFloat("corner", (size - delta - delta) / 2);
            var viewMatrix = Matrix4x4.identity;
            cacheCmd.DrawMesh(mesh, viewMatrix, material);
            
            Graphics.ExecuteCommandBuffer(cacheCmd);
        }

        private Texture2D imageTex;

        private RenderTexture maskTex;

        private int step = 0;

        IEnumerator _LoadImage(string file)
        {
            var uri = "file://" + Application.dataPath + "/Resources/" + file;
            
            using (var www = UnityWebRequestTexture.GetTexture(uri)) {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    throw new Exception($"Failed to get file \"{uri}\": {www.error}");
                }

                var data = ((DownloadHandlerTexture) www.downloadHandler).texture;
                imageTex = data;
                yield return data;
            }
        }

        protected override void DoDraw()
        {
            //draw call == 1
            //Step 1: Check whether the target image exists (A Texture2D), if not create it
            if (step == 0)
            {
                step = 1;
                _owner.StartCoroutine(_LoadImage("Images/china-flag-badge.png"));
                return;
            }
            else if (step == 1)
            {
                if (imageTex == null)
                {
                    return;
                }

                step = 2;
            }
            else if (step == 2)
            {
                //Step 2: Check whether the CacheTexture exists, if not, draw it
                if (this.maskTex == null)
                {
                    DrawCachePicture();
                }

                //Step 3: Bind two textures and draw the final thing
                cmdBuf.SetGlobalTexture("_tex", imageTex);
                cmdBuf.SetGlobalTexture("_maskTex", maskTex);

                var width = maskTex.width;
                var height = maskTex.height;

                var imageWidthRatio = 1.0f * width / imageTex.width;
                var imageHeightRatio = 1.0f * height / imageTex.height;
            
                Vector3[] vertices =
                {
                    new Vector3(200, 200, 0),
                    new Vector3(200 + width, 200, 0),
                    new Vector3(200, 200 + height, 0),
                    new Vector3(200 + width, 200 + height, 0)
                };

                Vector2[] uvs =
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };

                var left = 0.5f - imageWidthRatio / 2f;
                var right = 0.5f + imageWidthRatio / 2f;
                var up = 0.5f - imageHeightRatio / 2f;
                var down = 0.5f + imageHeightRatio / 2f;

                Vector2[] uv2 =
                {
                    new Vector2(left, up),
                    new Vector2(right, up),
                    new Vector2(left, down), 
                    new Vector2(right, down)
                };
            
                int[] triangles = {0, 1, 2, 2, 1, 3};
            
                var material = GetMaterial(ShaderType.ClipAlpha);
                material.SetVector("_viewport", new Vector4(0, 0, windowWidth, windowHeight));
                var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
                mesh.uv2 = uv2;
                var viewMatrix = Matrix4x4.identity;

                cmdBuf.DrawMesh(mesh, viewMatrix, material, 0);
            }
        }
    }
}