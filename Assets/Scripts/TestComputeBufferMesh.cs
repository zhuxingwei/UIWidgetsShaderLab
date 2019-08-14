using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = System.Random;

namespace DefaultNamespace
{
    public class TestComputeBufferMesh: FlushDrawer
    {
        private const float size = 20;
        private const int num = 10;

        private const bool drawBuffer = true;
        
        public TestComputeBufferMesh(int height, int width, int msaaSamples) : base(height, width, msaaSamples)
        {
        }

        protected override void DoDraw()
        {
            if (drawBuffer)
            {
                DrawBuffer();
            }
            else
            {
                DrawMesh();
            }
        }
        
        struct TVertex
        {
            public Vector2 position;
            public Vector4 color;
        }

        struct Index
        {
            public int index;
        }

        private ComputeBuffer computeBuffer;
        private List<TVertex> tvertexes;

        private ComputeBuffer indexBuffer;
        private List<int> indexes;
        
        private int startVertex;
        private int startIndex;

        private void DrawBuffer()
        {
            if (computeBuffer == null)
            {
                var stride = Marshal.SizeOf(typeof(TVertex));
                computeBuffer = new ComputeBuffer(1024 * 1024, stride);
                tvertexes = new List<TVertex>();
                
                indexBuffer = new ComputeBuffer(1024 * 1024, Marshal.SizeOf(typeof(int)));
                indexes = new List<int>();
            }
            
            tvertexes.Clear();
            indexes.Clear();
            startVertex = 0;
            startIndex = 0;
            
            var material = GetMaterial(ShaderType.FillComputeBuffer);
            material.SetVector("_viewport", new Vector4(0, 0, windowWidth, windowHeight));
            
            var random = new Random();
            for (var i = 0; i < num; i++)
            {
                for (var j = 0; j < num; j++)
                {
                        var offsetY = i * size;
                        var offsetX = j * size;
                        var color = new Color(random.Next(100) / 100.0f, random.Next(100) / 100.0f,
                            random.Next(100) / 100.0f);
                        var centerX = offsetX + size / 2;
                        var centerY = offsetY + size / 2;
                        var width = size;
                        var height = size;

                        startVertex = tvertexes.Count;
                        startIndex = indexes.Count;
                        
                    
                        tvertexes.AddRange(new[]
                        {
                            new TVertex
                            {
                                position = new Vector2(centerX - width / 2, centerY - height / 2), color = color
                            },
                            new TVertex
                            {
                                position = new Vector2(centerX + width / 2, centerY - height / 2), color = color
                            },
                            new TVertex
                            {
                                position = new Vector2(centerX + width / 2, centerY + height / 2), color = color
                            },
//                            new TVertex
//                            {
//                                position = new Vector2(centerX - width / 2, centerY - height / 2), color = color
//                            },
//                            new TVertex
//                            {
//                                position = new Vector2(centerX + width / 2, centerY + height / 2), color = color
//                            },
                            new TVertex
                            {
                                position = new Vector2(centerX - width / 2, centerY + height / 2), color = color
                            }
                        });
                    
                        indexes.AddRange(new []
                        {
                            startVertex, startVertex + 1, startVertex + 2, startVertex, startVertex + 2, startVertex + 3
                            //new Index{index = startVertex}, new Index{index = startVertex + 1}, new Index{index = startVertex + 2},
                            //new Index{index = startVertex}, new Index{index = startVertex + 2}, new Index{index = startVertex + 3}
                        });

                        var mpb = new MaterialPropertyBlock();
                        mpb.SetBuffer("databuffer", computeBuffer);
                        mpb.SetBuffer("indexbuffer", indexBuffer);
                        mpb.SetInt("_startVertex", startIndex);
                        cmdBuf.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 6, 1, mpb);
                }
            }
            
            computeBuffer.SetData(tvertexes);
            indexBuffer.SetData(indexes);
        }

        private void DrawMesh()
        {
            var material = GetMaterial(ShaderType.Fill);
            material.SetVector("_viewport", new Vector4(0, 0, windowWidth, windowHeight));

            var random = new Random();
            
            for (var i = 0; i < num; i++)
            {
                for (var j = 0; j < num; j++)
                {
                    var offsetY = i * size;
                    var offsetX = j * size;
                    var color = new Color(random.Next(100) / 100.0f, random.Next(100) / 100.0f, random.Next(100) / 100.0f);
                    
                    DrawBox(offsetX + size / 2, offsetY + size / 2, size, size, color, material);
                }
            }
        }
    }
}