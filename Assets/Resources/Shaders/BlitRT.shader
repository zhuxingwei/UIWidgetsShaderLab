Shader "Custom/BlitRT"
{
    Properties
    {
        _SrcBlend("_SrcBlend", Int) = 1 // One
        _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
    }
    SubShader
    {
        Blend [_SrcBlend] [_DstBlend]
        Pass {
            CGPROGRAM
            
            float4 _viewport;
            sampler2D _tex;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 tcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 ftcoord : TEXCOORD0;
            };
            
            v2f vert(appdata v){
                v2f o;
                o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);//UnityObjectToClipPos(v.vertex);
                o.ftcoord = v.tcoord;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_TARGET {
                float4 color = tex2D(_tex, i.ftcoord);
                return color;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
