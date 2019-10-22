Shader "Custom/ClipAlpha"
{
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            
            float4 _viewport;
            sampler2D _tex;
            sampler2D _maskTex;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 tcoord : TEXCOORD0;
                float2 tcoord2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 ftcoord : TEXCOORD0;
                float2 ftcoord2 : TEXCOORD2;
            };
            
            v2f vert(appdata v){
                v2f o;
                o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);//UnityObjectToClipPos(v.vertex);
                o.ftcoord = v.tcoord;
                o.ftcoord2 = v.tcoord2;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_TARGET {
                float4 color = tex2D(_tex, i.ftcoord2);
                float4 alpha = tex2D(_maskTex, i.ftcoord);
                color.a = alpha.a;
                return color;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
