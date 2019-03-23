Shader "Animations/Windshield"
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

            float _x_center = 0.5;
            float _y_center = 0;

            float _speed = 1.5;
            float _slope = sin(pow(_Time.y * _speed,1)) * 5;
            float _width = 0.025;

            float _y = (fi.tex.y - _y_center);
            float _x = _y * _slope + _x_center;

            if (_x - _width < fi.tex.x & fi.tex.x < _x + _width)
                col = float4(1,1,1,1);
            else
                col = float4(0,0,0,1);

            float r = sqrt
            (
                pow(fi.tex.x - _x_center,2) +
                pow(fi.tex.y - _y_center,2)
            );

            if (r > 0.5)
                col = float4(0.1,0.1,0.1,1);

            return col;
         }
         ENDCG
     }
 }
}
