Shader "Unlit/Raymarching"
{
	Properties
	{
		_TempValue("Temp", vector) = (0,0,0,0)
	}
		SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
			"Queue" = "Geometry+0"
		}
		LOD 100

		Pass
		{
			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#pragma multi_compile_instancing
			#pragma multi_compile_fog

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT

			#define EPSILON		0.001f
			#define MAX_DIST	500
			#define MAX_STEPS	250
			#define SURF_DIST	0.001f


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 origin : TEXCOORD1;
				float3 direction : TEXCOORD2;
				float4 pos : TEXCOORD3;
			};


			struct ObjectStructure
			{
				float3 Position;
				float Type;
				float Size;
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TempValue;
			CBUFFER_END


			#define MAX_OBJECT_COUNT 30
			StructuredBuffer<ObjectStructure> _Objects;
			float _ObjectCount;

			float smin(float a, float b, float k)
			{
				float h = max(k - abs(a - b), 0.0) / k;
				return min(a, b) - h * h * k * (1.0 / 4.0);
			}

			float SDF(float3 position)
			{
				float d = 10000;

				float planeDist = position.y;

				for (int i = 0; i < _ObjectCount; ++i)
				{
					//float4 value = _Objects[i];
					float3 pos = _Objects[i].Position;
					float radius = _Objects[i].Size;

					float dist = length(pos - position) - radius;
					
					d = smin(dist, d,1);

				}

				return smin(d,planeDist,1);
			}

			float Raymarching(float3 ro, float3 rd)
			{
				float d = 0;

				for (int i = 0; i < MAX_STEPS; ++i)
				{
					float3 p = ro + (rd * d);
					float dS = SDF(p);
					d += dS;

					if (d > MAX_DIST)
						break;

					if (dS < SURF_DIST)
						break;

				}

				return d;
			}

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.uv = v.uv;

				float3 camPosVS = mul(UNITY_MATRIX_V, float4(GetCameraPositionWS(),1)).xyz;
				o.origin = mul(UNITY_MATRIX_MV, v.vertex);
				o.direction = normalize(o.origin - camPosVS);
				return o;
			}

			float3 GetNormal(float3 p)
			{
				float d = SDF(p);
				float2 e = float2(.01, 0);
				float3 n = d - float3(SDF(p - e.xyy), SDF(p - e.yxy), SDF(p - e.yyx));
				return normalize(n);
			}

			float GetLight(float3 p)
			{
				float3 lightPos = float3(0, 5, 6);
				lightPos.xz += float2(sin(_Time.y), cos(_Time.y)) * 2.0;
				float3 l = normalize(lightPos - p);
				float3 n = GetNormal(p);
				float dif = clamp(dot(n, l), 0.0, 1.0);
				float d = Raymarching(p + n * SURF_DIST * 2.0, l);
				if (d < length(lightPos - p)) dif *= 0.1;
				return dif;
			}


			half4 frag(v2f i) : SV_Target
			{
				float2 uv = float2(i.uv - 0.5);
				float3 ro = float3(0, 1, 0);
				float3 rd = normalize(float3(uv.x, uv.y, 1));

				float d = Raymarching(ro, rd);
				float3 p = ro + rd * d;

				float l = GetLight(p);
				float4 col = float4(l, l, l, 1);

				return col;
			}

			ENDHLSL

		}
	}
}