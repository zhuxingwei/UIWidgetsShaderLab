using UnityEngine;

namespace DefaultNamespace
{
    public class TestShadowRBox : FlushDrawer
    {
        public TestShadowRBox(int height, int width, int msaaSamples) : base(height, width, msaaSamples)
        {
            
        }

        protected override void DoDraw()
        {
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
            material.SetVector("box", new Vector4(50, 100, 150, 150));
            //width: 100, height: 100
            material.SetVector("window", new Vector2(windowWidth, windowHeight));
            material.SetFloat("sigma", 3f);
            material.SetVector("color", new Vector4(0, 0, 0, 1));
            material.SetFloat("corner", 50f);
            var viewMatrix = Matrix4x4.identity;
            cmdBuf.DrawMesh(mesh, viewMatrix, material);
        }
    }
}