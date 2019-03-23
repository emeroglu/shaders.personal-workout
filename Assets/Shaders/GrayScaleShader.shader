Shader "Custom/GrayScaleShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

            sampler2D _MainTex;

            struct VertInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            FragInput vert (VertInput v)
            {
                FragInput o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

			fixed4 frag (FragInput i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

                fixed c = (col.r + col.g + col.b) * 0.3;

				return fixed4(c,c,c,1);
			}
			ENDCG
		}
	}
}
