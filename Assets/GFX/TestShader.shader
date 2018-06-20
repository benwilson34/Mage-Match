// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TestShader" {
	
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_SecondTexture("Second Texture", 2D) = "white" {}
		_Tween("Tween", Range(0, 1)) = 0
	}

	SubShader {
		Pass {

			ZTest Off
			Blend SrcAlpha OneMinusSrcAlpha

			Tags {
				"Queue" = "Transparent"
			}
		
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
			sampler2D _SecondTexture;
			float _Tween;

			float4 frag(v2f i) : SV_TARGET {
				float4 texColor = (tex2D(_MainTex, i.uv) * (1 - _Tween)) + (tex2D(_SecondTexture, i.uv) * _Tween);
				return texColor;
			}

			ENDCG
		}

	}

}