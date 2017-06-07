Shader "Custom/uiElementShader"
{
	Properties
	{
		_Darkness ("Darken Intensity", Range(0.0, 1.0)) = 0
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		LOD 125
		Tags 
		{ 
			"RenderType"="Transparent"
			"RenderType" = "Opaque"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Off
			AlphaTest Off

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Darkness;
			half4 _Color;

			//Note: v2f_img is the 'default empty' v2f function. It is used in shaders where a vertex function is not needed

			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.r = col.r - (col.r * _Darkness);
				col.g = col.g - (col.g * _Darkness);
				col.b = col.b - (col.b * _Darkness);
				return col * _Color;
			}
			ENDCG
		}
	}
}
