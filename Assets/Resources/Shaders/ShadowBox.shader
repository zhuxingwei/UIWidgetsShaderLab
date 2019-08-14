Shader "Custom/ShadowBox"
{
    //originally from http://madebyevan.com/shaders/fast-rounded-rectangle-shadows/
    Properties
    {
        _SrcBlend("_SrcBlend", Int) = 1 // One
        _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
        _StencilComp("_StencilComp", Float) = 8 // - Equal, 8 - Always 
    }
    SubShader
    {
        ZTest Always
        ZWrite Off
        Blend [_SrcBlend] [_DstBlend]
        
        Stencil {
            Ref 128
            Comp [_StencilComp]
        }
        
        Pass {
            CGPROGRAM
            
            float4 box;
            float2 window;
            float sigma;
            float4 color;
            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 coord : TEXCOORD0;
            };
            
            float4 erf(float4 x) 
            {
                float4 s = sign(x);
                float4 a = abs(x);
                x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
                x = x * x;
                return s - s / (x * x);
                return s;
            }
            
            float boxShadow(float2 lower, float2 upper, float2 pnt, float sigma)
            {
                float4 query = float4(pnt - lower, pnt - upper);
                float4 integral = 0.5 + 0.5 * erf(query * (sqrt(0.5) / sigma));
                return (integral.z - integral.x) * (integral.w - integral.y);
            }
            
            v2f vert(appdata v){
                v2f o;
                float padding = 3.0 * sigma;
                o.coord = lerp(box.xy - padding, box.zw + padding, v.vertex.xy);
                o.vertex = float4(o.coord.x * 2.0 /window.x - 1.0, o.coord.y * 2.0/window.y - 1.0, 0, 1);
                return o;
            }
            
            float4 frag(v2f i) : SV_TARGET {
                float4 fragColor = color;
                fragColor.a = fragColor.a * boxShadow(box.xy, box.zw, i.coord, sigma);
                return fragColor;
            }
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
        }
    }
}