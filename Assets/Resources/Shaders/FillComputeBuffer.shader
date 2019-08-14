Shader "Custom/FillComputeBuffer"
{
    Properties
    {
        
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct vdata
            {
                float2 vertex;
                float4 color;
            };
            
            struct vindex
            {
                int index;
            };

            struct psInput
            {
                float4 position : SV_POSITION;
                float4 color: COLOR;
            };

            StructuredBuffer<vdata> databuffer;
            StructuredBuffer<int> indexbuffer;
            float4 _viewport;
            int _startVertex;
            
            psInput vert (uint vertex_id: SV_VertexID, uint instance_id: SV_InstanceID)
            {
                psInput o = (psInput)0;
                o.position = float4(databuffer[indexbuffer[_startVertex + vertex_id]].vertex.x * 2.0 / _viewport.z - 1.0, databuffer[indexbuffer[_startVertex + vertex_id]].vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
                o.color = databuffer[indexbuffer[_startVertex + vertex_id]].color;
                return o;
            }
            
            fixed4 frag (psInput i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}