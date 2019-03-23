Shader "Animations/Bouncing Ring"
{
 SubShader
 {
    Blend SrcAlpha OneMinusSrcAlpha

     Pass
     {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag

         struct VertInput
         {
         float4 pos : POSITION;
            float2 tex : TEXCOORD0;
         };

         struct FragInput
         {
            float4 pos : SV_POSITION;
            float2 tex : TEXCOORD0;
         };
         
         FragInput vert (VertInput v)
         {
            FragInput fi;
                
            fi.pos = UnityObjectToClipPos(v.pos);
            fi.tex = v.tex;

            return fi;
         }

         fixed4 frag (FragInput fi) : COLOR
         {
            fixed4 col;

            float4 _ring_Color = float4(0.4,0.2,0.8,1);
            float4 _out_Color = float4(1,1,1,1);

            float _center_X = 0.5;
            float _center_Y = 0.5;

            float _r_Max = max(0.3,0.45 * abs(sin(pow(_Time.y * 2,1))));
            float _r_Min = max(0.175,0.3 * abs(sin(pow(_Time.y * 2,1))));

            float r = sqrt
            (
                pow(fi.tex.x - _center_X,2) +
                pow(fi.tex.y - _center_Y,2)
            );

            if (_r_Min < r & r < _r_Max)
            {
                col = _ring_Color * float4(_r_Max,_r_Max,_r_Max,1);
            }
            else
                col = _out_Color;

            return col;
         }
         ENDCG
     }
 }
}
