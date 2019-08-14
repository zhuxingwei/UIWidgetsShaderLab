Shader "Custom/GaussianBlur"
{
    Properties
    {
        _SrcBlend("_SrcBlend", Int) = 1 // One
        _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
    }
    
    CGINCLUDE
    
    struct appdata
    {
         float4 vertex : POSITION;
         float2 uv : TEXCOORD0;
    };
    
    struct v2f2
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    struct v2f
    {
         float2 uv : TEXCOORD0;
         float4 vertex : SV_POSITION;
         float4 blur[2] : TEXCOORD1;
    };

    float4 _viewport;
    sampler2D _tex;
    
    v2f2 vert3(appdata v) {
        v2f2 o;
        o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
        o.uv = v.uv;
        return o;
    }
    
    /*
    * from http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
    * results in a 9-tap gaussian blur
    */
            
    v2f vert(appdata v){
        v2f o;
        o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
        o.uv = v.uv;
        float2 offset1 = float2(0.0, 1.38461538 * 0.01);
        float2 offset2 = float2(0.0, 3.23076923 * 0.01);
        o.blur[0].xy = o.uv + offset1;
        o.blur[0].zw = o.uv - offset1;
        o.blur[1].xy = o.uv + offset2;
        o.blur[1].zw = o.uv - offset2;
        return o;
    }
    
    v2f vert2(appdata v){
        v2f o;
        o.vertex = float4(v.vertex.x * 2.0 / _viewport.z - 1.0, v.vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
        o.uv = v.uv;
        float2 offset1 = float2(1.38461538 * 0.01, 0.0);
        float2 offset2 = float2(3.23076923 * 0.01, 0.0);
        o.blur[0].xy = o.uv + offset1;
        o.blur[0].zw = o.uv - offset1;
        o.blur[1].xy = o.uv + offset2;
        o.blur[1].zw = o.uv - offset2;
        return o;
    }
            
    fixed4 frag(v2f i) : SV_TARGET {
        fixed4 sum = tex2D(_tex, i.uv) * 0.22702702;
		sum += tex2D(_tex, i.blur[0].xy) * 0.31621621;
		sum += tex2D(_tex, i.blur[0].zw) * 0.31621621;
		sum += tex2D(_tex, i.blur[1].xy) * 0.07027027;
		sum += tex2D(_tex, i.blur[1].zw) * 0.07027027;
		return sum;
    }
    
    fixed4 frag3(v2f2 i) : SV_TARGET {
        fixed4 sum = tex2D(_tex, i.uv) * 0.22702702;
        float2 offset1 = float2(1.38461538 * 0.01, 0.0);
        float2 offset2 = float2(3.23076923 * 0.01, 0.0);
        
		sum += tex2D(_tex, (i.uv + offset1)) * 0.31621621;
		sum += tex2D(_tex, (i.uv - offset1)) * 0.31621621;
		sum += tex2D(_tex, (i.uv + offset2)) * 0.07027027;
		sum += tex2D(_tex, (i.uv - offset2)) * 0.07027027;
		return sum;
    }
    
    fixed4 frag2(v2f2 i) : SV_TARGET {
        fixed4 sum = tex2D(_tex, i.uv) * 0.22702702;
        float2 offset1 = float2(0.0, 1.38461538 * 0.01);
        float2 offset2 = float2(0.0, 3.23076923 * 0.01);
		sum += tex2D(_tex, (i.uv + offset1)) * 0.31621621;
		sum += tex2D(_tex, (i.uv - offset1)) * 0.31621621;
		sum += tex2D(_tex, (i.uv + offset2)) * 0.07027027;
		sum += tex2D(_tex, (i.uv - offset2)) * 0.07027027;
		return sum;
    }
    
    ENDCG
    
    SubShader
    {
        Blend [_SrcBlend] [_DstBlend]
        Pass        //Pass#0
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
        Pass        //Pass#1
        {
            CGPROGRAM
            #pragma vertex vert2
            #pragma fragment frag
            ENDCG
        }
        Pass         //Pass#2
        {
            CGPROGRAM
            #pragma vertex vert3
            #pragma fragment frag2
            ENDCG
        }
        Pass         //Pass#3
        {
            CGPROGRAM
            #pragma vertex vert3
            #pragma fragment frag3
            ENDCG
        }
    }
}