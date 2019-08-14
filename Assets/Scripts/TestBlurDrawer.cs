using UnityEngine;

namespace DefaultNamespace
{
    enum BlurTypes
    {
        None,
        TwoPass,
        TwoLayer,
        Pingpong,
        TwoLayerFragCal
    };
    
    //draw Cnt:
    public class TestBlurDrawer : FlushDrawer
    {
        private const int msaaSampleOverride = 1;
        private const BlurTypes blurShaderId = BlurTypes.Pingpong;
        private const int drawCnt = 5;
        
        public TestBlurDrawer(int height, int width, int msaaSamples) : base(height, width, msaaSampleOverride)
        {
           
        }

        //RT performance check
        
        //draw shadow box by:
        //using only two temporary rt in a ping-pong manner to draw all the shadows
        private void DrawShadowBox2(int width, int height)
        {
            var shadowLayer = RequestTempRT(width + 40, height + 40);
            var tempBlurLayer = RequestTempRT(width + 40, height + 40);
            
            for (var i = 0; i < 5; i++)
            {
                var offsetY = (i / 10) * 120 + (i % 10) * 10;
                var offsetX = (i % 10) * 50;

                var red = i % 3 == 0 ? 1.0f : 0.0f;
                var green = i % 3 == 1 ? 1.0f : 0.0f;
                var blue = i % 3 == 2 ? 1.0f : 0.0f;
                
                //draw box to shadowLayer
                cmdBuf.SetRenderTarget(shadowLayer);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
                var material = GetMaterial(ShaderType.Fill);
                material.SetVector("_viewport", new Vector4(0, 0, width + 40, height + 40));
                DrawBox(width / 2 + 20, height / 2 + 20, width, height, new Vector4(red, green, blue, 1f), material);

                //draw shadow to blurlayer
                cmdBuf.SetRenderTarget(tempBlurLayer);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
                cmdBuf.SetGlobalTexture("_tex", shadowLayer);
                Vector3[] vertices =
                {
                    new Vector3(0, 0, 0),
                    new Vector3(width + 40, 0, 0),
                    new Vector3(0, height + 40, 0),
                    new Vector3(width + 40, height + 40, 0)
                };

                Vector2[] uvs =
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            
                int[] triangles = {0, 1, 2, 2, 1, 3};
                var material2 = GetMaterial(ShaderType.GaussianBlur);
                material2.SetVector("_viewport", new Vector4(0, 0, width + 40, height + 40));
                var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
                var viewMatrix = Matrix4x4.identity;

                cmdBuf.DrawMesh(mesh, viewMatrix, material2, 0, 1);
            
                //draw blur shadow back to shadowLayer
                cmdBuf.SetRenderTarget(shadowLayer);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
                cmdBuf.SetGlobalTexture("_tex", tempBlurLayer);
            
                cmdBuf.DrawMesh(mesh, viewMatrix, material2, 0, 0);
                
                
                //Blit to canvas
                Blit(null, shadowLayer, windowWidth, windowHeight, offsetX, offsetY, width + 40, height + 40);
            }
        }
        
        //draw shadow box by:
        //request new trt every time but release it immediately just after the execution done
        private void DrawShadowBox3(int width, int height)
        {
            for (var i = 0; i < 5; i++)
            {
                var shadowLayer = RequestTempRT(width + 40, height + 40);
                var tempBlurLayer = RequestTempRT(width + 40, height + 40);

                var offsetY = (i / 10) * 120 + (i % 10) * 10;
                var offsetX = (i % 10) * 50;

                var red = i % 3 == 0 ? 1.0f : 0.0f;
                var green = i % 3 == 1 ? 1.0f : 0.0f;
                var blue = i % 3 == 2 ? 1.0f : 0.0f;
                
                //draw box to shadowLayer
                cmdBuf.SetRenderTarget(shadowLayer);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
                var material = GetMaterial(ShaderType.Fill);
                material.SetVector("_viewport", new Vector4(0, 0, width + 40, height + 40));
                DrawBox(width / 2 + 20, height / 2 + 20, width, height, new Vector4(red, green, blue, 1f), material);

                
                //draw shadow to blurlayer
                cmdBuf.SetRenderTarget(tempBlurLayer);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
                cmdBuf.SetGlobalTexture("_tex", shadowLayer);
                Vector3[] vertices =
                {
                    new Vector3(0, 0, 0),
                    new Vector3(width + 40, 0, 0),
                    new Vector3(0, height + 40, 0),
                    new Vector3(width + 40, height + 40, 0)
                };

                Vector2[] uvs =
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 1)
                };
            
                int[] triangles = {0, 1, 2, 2, 1, 3};
                var material2 = GetMaterial(ShaderType.GaussianBlur);
                material2.SetVector("_viewport", new Vector4(0, 0, width + 40, height + 40));
                var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
                var viewMatrix = Matrix4x4.identity;

                cmdBuf.DrawMesh(mesh, viewMatrix, material2, 0, 1);
            
                //draw blur shadow back to shadowLayer
                cmdBuf.SetRenderTarget(shadowLayer);
                cmdBuf.ClearRenderTarget(true, true, Color.clear);
                cmdBuf.SetGlobalTexture("_tex", tempBlurLayer);
            
                cmdBuf.DrawMesh(mesh, viewMatrix, material2, 0, 0);
                
                cmdBuf.ReleaseTemporaryRT(shadowLayer);
                cmdBuf.ReleaseTemporaryRT(tempBlurLayer);
                //Blit to canvas
                Blit(null, shadowLayer, windowWidth, windowHeight, offsetX, offsetY, width + 40, height + 40);
            }
        }

        private int DrawBlurBox(int width, int height, Vector4 color, BlurTypes blurType)
        {
            //draw box on the middle of a larger canvas (+40 pixels on each side)
            var shadowLayer = RequestTempRT(width + 40, height + 40);
            cmdBuf.SetRenderTarget(shadowLayer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            var material = GetMaterial(ShaderType.Fill);
            material.SetVector("_viewport", new Vector4(0, 0, width + 40, height + 40));
            DrawBox(width / 2 + 20, height / 2 + 20, width, height, color, material);

            var blurLayer = -1;
            switch (blurType)
            {
                case BlurTypes.None:
                {
                    blurLayer = shadowLayer;
                    break;
                }
                case BlurTypes.TwoPass:
                {
                    blurLayer = Blur(shadowLayer, 140, 140);
                    break;
                }
                case BlurTypes.TwoLayer:
                {
                    blurLayer = Blur2(shadowLayer, 140, 140);
                    break;
                }
                case BlurTypes.Pingpong:
                {
                    blurLayer = Blur3(shadowLayer, 140, 140);
                    break;
                }
                case BlurTypes.TwoLayerFragCal:
                {
                    blurLayer = Blur4(shadowLayer, 140, 140);
                    break;
                }
            }
            return blurLayer;
        }

        int Blur4(int srcLayer, int dstLayerWidth, int dstLayerHeight)
        {
            var blurLayer = RequestTempRT(dstLayerWidth, dstLayerHeight);
            cmdBuf.SetRenderTarget(blurLayer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", srcLayer);
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0),
                new Vector3(dstLayerWidth, 0, 0),
                new Vector3(0, dstLayerHeight, 0),
                new Vector3(dstLayerWidth, dstLayerHeight, 0)
            };

            Vector2[] uvs =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            int[] triangles = {0, 1, 2, 2, 1, 3};
            var material = GetMaterial(ShaderType.GaussianBlur);
            material.SetVector("_viewport", new Vector4(0, 0, dstLayerWidth, dstLayerHeight));
            var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
            var viewMatrix = Matrix4x4.identity;

            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 2);
            
            var blur2Layer = RequestTempRT(dstLayerWidth, dstLayerHeight);
            cmdBuf.SetRenderTarget(blur2Layer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", blurLayer);
            
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 3);
            return blur2Layer;
        }
        
        /*
         * Render to the same renderTexture that is used for sampling
         *
         * Result: bad practice, the outcome is wrong
         */
        int Blur5(int srcLayer, int dstLayerWidth, int dstLayerHeight)
        {
            cmdBuf.SetGlobalTexture("_tex", srcLayer);
            cmdBuf.SetRenderTarget(srcLayer);
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0),
                new Vector3(dstLayerWidth, 0, 0),
                new Vector3(0, dstLayerHeight, 0),
                new Vector3(dstLayerWidth, dstLayerHeight, 0)
            };

            Vector2[] uvs =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            int[] triangles = {0, 1, 2, 2, 1, 3};
            var material = GetMaterial(ShaderType.GaussianBlur);
            material.SetVector("_viewport", new Vector4(0, 0, dstLayerWidth, dstLayerHeight));
            var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
            var viewMatrix = Matrix4x4.identity;

            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 1);
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 0);
            return srcLayer;
        }

        int Blur3(int srcLayer, int dstLayerWidth, int dstLayerHeight)
        {
            var blurLayer = RequestTempRT(dstLayerWidth, dstLayerHeight);
            cmdBuf.SetRenderTarget(blurLayer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", srcLayer);
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0),
                new Vector3(dstLayerWidth, 0, 0),
                new Vector3(0, dstLayerHeight, 0),
                new Vector3(dstLayerWidth, dstLayerHeight, 0)
            };

            Vector2[] uvs =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            int[] triangles = {0, 1, 2, 2, 1, 3};
            var material = GetMaterial(ShaderType.GaussianBlur);
            material.SetVector("_viewport", new Vector4(0, 0, dstLayerWidth, dstLayerHeight));
            var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
            var viewMatrix = Matrix4x4.identity;

            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 1);
            
            
            cmdBuf.SetRenderTarget(srcLayer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", blurLayer);
            
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 0);
            return srcLayer;
        }

        int Blur2(int srcLayer, int dstLayerWidth, int dstLayerHeight)
        {
            var blurLayer = RequestTempRT(dstLayerWidth, dstLayerHeight);
            cmdBuf.SetRenderTarget(blurLayer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", srcLayer);
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0),
                new Vector3(dstLayerWidth, 0, 0),
                new Vector3(0, dstLayerHeight, 0),
                new Vector3(dstLayerWidth, dstLayerHeight, 0)
            };

            Vector2[] uvs =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            int[] triangles = {0, 1, 2, 2, 1, 3};
            var material = GetMaterial(ShaderType.GaussianBlur);
            material.SetVector("_viewport", new Vector4(0, 0, dstLayerWidth, dstLayerHeight));
            var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
            var viewMatrix = Matrix4x4.identity;

            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 1);
            
            var blur2Layer = RequestTempRT(dstLayerWidth, dstLayerHeight);
            cmdBuf.SetRenderTarget(blur2Layer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", blurLayer);
            
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 0);
            return blur2Layer;
        }
        
        int Blur(int srcLayer, int dstLayerWidth, int dstLayerHeight)
        {
            var blurLayer = RequestTempRT(dstLayerWidth, dstLayerHeight);
            cmdBuf.SetRenderTarget(blurLayer);
            cmdBuf.ClearRenderTarget(true, true, Color.clear);
            cmdBuf.SetGlobalTexture("_tex", srcLayer);
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0),
                new Vector3(dstLayerWidth, 0, 0),
                new Vector3(0, dstLayerHeight, 0),
                new Vector3(dstLayerWidth, dstLayerHeight, 0)
            };

            Vector2[] uvs =
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            int[] triangles = {0, 1, 2, 2, 1, 3};
            var material = GetMaterial(ShaderType.GaussianBlur);
            material.SetVector("_viewport", new Vector4(0, 0, dstLayerWidth, dstLayerHeight));
            var mesh = new Mesh {vertices = vertices, triangles = triangles, uv = uvs};
            var viewMatrix = Matrix4x4.identity;

            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 1);
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 0);
            return blurLayer;
        }

        private void DrawShadowBox()
        {
            for (int i = 0; i < drawCnt; i++)
            {
                var offsetY = (i / 10) * 120 + (i % 10) * 10;
                var offsetX = (i % 10) * 50;

                float red = i % 3 == 0 ? 1.0f : 0.0f;
                float green = i % 3 == 1 ? 1.0f : 0.0f;
                float blue = i % 3 == 2 ? 1.0f : 0.0f;
                
                var blurBoxLayer = DrawBlurBox(100, 100, new Vector4(red, green, blue, 1f), blurShaderId);
                Blit(null, blurBoxLayer, windowWidth, windowHeight, offsetX, offsetY, 140, 140);
            }
        }

        protected override void DoDraw()
        {
            DrawShadowBox2(100, 100);
        }
    }
}