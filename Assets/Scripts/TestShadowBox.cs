using UnityEngine;

namespace DefaultNamespace
{
    public class TestShadowBox : FlushDrawer
    {
        public TestShadowBox(int height, int width, int msaaSamples) : base(height, width, msaaSamples)
        {
            
        }

        private int boxId = Shader.PropertyToID("box");
        private int windowId = Shader.PropertyToID("window");
        private int sigmaId = Shader.PropertyToID("sigma");
        private int colorId = Shader.PropertyToID("color");
        

        protected override void DoDraw()
        {
            var material = GetMaterial(ShaderType.ShadowBox);
            
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
            var mpb = new MaterialPropertyBlock();
            mpb.SetVector(boxId, new Vector4(50, 50, 150, 150));
            mpb.SetVector(windowId, new Vector2(windowWidth, windowHeight));
            mpb.SetFloat(sigmaId, 3f);
            mpb.SetVector(colorId, new Vector4(0, 0, 0, 1));
            var viewMatrix = Matrix4x4.identity;
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 0, mpb);
            
            var mpb2 = new MaterialPropertyBlock();
            
            mpb2.SetVector(boxId, new Vector4(150, 150, 200, 200));
            mpb2.SetVector(windowId, new Vector2(windowWidth, windowHeight));
            mpb2.SetFloat(sigmaId, 3f);
            mpb2.SetVector(colorId, new Vector4(1, 0, 0, 1));
            cmdBuf.DrawMesh(mesh, viewMatrix, material, 0, 0, mpb2);
        }
    }
}