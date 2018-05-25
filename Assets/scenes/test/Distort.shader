Shader "Custom/Distort" {

	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_DistortionMap("Distortion Map", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		_Magnitude("Magnitude", Range(0,0.02)) = 0.01
	}

	SubShader {

		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
				
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

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _DistortionMap;
			float _Magnitude;
			float4 _Color;

			float4 frag(v2f i) : SV_TARGET {

				float2 distortionUV = float2(i.uv.x + _Time.x * 2, i.uv.y + _Time.x * 2);

				float2 disp = tex2D(_DistortionMap, distortionUV).xy;
				disp = ((disp * 2) - 1) * _Magnitude;

				float4 col = tex2D(_MainTex, i.uv + disp);
				_Color = (_Color.r, _Color.g, _Color.b, _Color.a - (_Time.y % 1));
				return col * _Color;
			}
			ENDCG
		}
	}
}
