Shader "Custom/GlowShader" {

	Properties {
		_OuterColor("Outer Color", Color) = (1, 1, 1, 0.5)
		_InnerColor("Inner Color", Color) = (1, 1, 1, 1)
 		_Range("Range", Range(0.5, 1.25)) = 0.5
		_Multiplier("Multiplier", Range(0.05, 30.0)) = 2.0
	}

	SubShader {
		Pass {

			Tags {
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAlias" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha


			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _OuterColor;
			float4 _InnerColor;
			float _Range;
			float _Multiplier;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET {
				float t = length(i.uv - float2(0.5, 0.5)) * 2.41715;

				return lerp(_InnerColor, _OuterColor, t + (_Range - 0.5)) * _Multiplier;
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}