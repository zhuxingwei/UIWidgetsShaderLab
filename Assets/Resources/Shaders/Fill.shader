Shader "Custom/Fill"
{
    Properties
    {
        
    }
    SubShader
    {
        Pass {
            CGPROGRAM
            
            float4 _viewport;
            
            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            v2f vert(appdata v){
                v2f o;
                o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_TARGET {
                return i.color;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
