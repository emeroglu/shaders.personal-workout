Shader "Custom/DrawingShader"
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

			fixed4 frag (FragInput i) : COLOR
			{
				fixed4 col;
                fixed c = 0;
                fixed a = 0;
                
                fixed isRightHalf = max(0, sign(i.uv.x - 0.5));
                fixed isLeftHalf = 1 - isRightHalf;

                fixed isTopHalf = max(0, sign(i.uv.y - 0.5));
                fixed isBottomHalf = 1 - isTopHalf;

                fixed isTopLeftDiagonal = max(0, sign(i.uv.y - i.uv.x));
                fixed isBottomRightDiagonal = 1 - isTopLeftDiagonal;

                fixed isTopRightDiagonal = max(0, sign(i.uv.y + i.uv.x - 1));
                fixed isBottomLeftDiagonal = 1 - isTopRightDiagonal;

                fixed rInner = sqrt( pow(i.uv.x - 0.5,2) + pow(i.uv.y - 0.5,2) ) - 0.25;
                fixed insideInnerCircle = 1 - max(0,sign(rInner));

                fixed rOuter = sqrt( pow(i.uv.x - 0.5,2) + pow(i.uv.y - 0.5,2) ) - 0.5;
                fixed insideOuterCircle = 1 - max(0,sign(rOuter));

                fixed rLeft = sqrt( pow(i.uv.x,2) + pow(i.uv.y - 0.5,2) ) - 0.5;
                fixed insideLeftCircle = 1 - max(0,sign(rLeft));

                fixed rRight = sqrt( pow(i.uv.x - 1,2) + pow(i.uv.y - 0.5,2) ) - 0.5;
                fixed insideRightCircle = 1 - max(0,sign(rRight));

                fixed rTop = sqrt( pow(i.uv.x - 0.5,2) + pow(i.uv.y - 1,2) ) - 0.5;
                fixed insideTopCircle = 1 - max(0,sign(rTop));

                fixed rBottom = sqrt( pow(i.uv.x - 0.5,2) + pow(i.uv.y,2) ) - 0.5;
                fixed insideBottomCircle = 1 - max(0,sign(rBottom));

                c += isTopLeftDiagonal * isRightHalf * 0.6;
                c += isBottomRightDiagonal * isTopHalf * 0.4;
                c += isTopRightDiagonal * isBottomHalf * 0.6;
                c += isBottomLeftDiagonal * isRightHalf * 0.4;
                c += isBottomRightDiagonal * isLeftHalf * 0.6;
                c += isTopLeftDiagonal * isBottomHalf * 0.4;
                c += isBottomLeftDiagonal * isTopHalf * 0.6;
                c += isTopRightDiagonal * isLeftHalf * 0.4;

                c += insideInnerCircle * 0.025;
                c += insideOuterCircle * 0.025;
                c += insideTopCircle * insideLeftCircle * 0.025;
                c += insideTopCircle * insideRightCircle * 0.025;
                c += insideBottomCircle * insideLeftCircle * 0.025;
                c += insideBottomCircle * insideRightCircle * 0.025;

                a = insideOuterCircle;

                col = fixed4(c,c,c,a);

				return col;
			}

			ENDCG
		}
	}
}
