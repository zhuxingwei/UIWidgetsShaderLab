Shader "Custom/BlurShader"
{
    Properties
	{
	    _SrcBlend("_SrcBlend", Int) = 1 // One
        _DstBlend("_DstBlend", Int) = 10 // OneMinusSrcAlpha
		_radius ("Blur radius", Range (0.01,2.0)) = 0.5
	}
	SubShader
	{
		Pass
		{
		    Blend [_SrcBlend] [_DstBlend]
		    
			CGPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma target 5.0

			float _radius;
			sampler2D _tex;
			float4 _viewport;
			
			struct structure
			{
				float4 vertex:SV_POSITION;
				float2 uv : TEXCOORD0;
			};
		
			void vertex_shader(float4 vertex:POSITION,float2 uv:TEXCOORD0,out structure vs) 
			{
				vs.vertex = float4(vertex.x * 2.0 / _viewport.z - 1.0, vertex.y * 2.0 / _viewport.w - 1.0, 0, 1);
				vs.uv = uv; 
			}

			float4 surface (sampler2D input, float2 uv)
			{
				return tex2D(input,uv);
			}

			float4 blur(float2 uv,float radius)
			{
				float2x2 m = float2x2(-0.736717,0.6762,-0.6762,-0.736717);
				float4 total = float4(0.0,0.0,0.0,0.0);
				float2 texel = float2(0.02,0.02);
				float2 angle = float2(0.0,radius);
				radius = 1.0;
				for (int j=0;j<10;j++)
				{  
					radius += rcp(radius); //sm 5.0 fast reciprocal function
					angle = mul(angle,m);
					float4 color = surface(_tex,uv+texel*(radius-1.0)*angle);
					total += color;
				}
				return total/10.0;
			}

			void pixel_shader(in structure ps, out float4 fragColor:SV_Target0) 
			{	
				float2 uv = ps.uv.xy;  
				fragColor = blur(uv,_radius);
			}
			ENDCG
		}
	}
}
