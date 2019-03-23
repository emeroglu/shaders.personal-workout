Shader "Animations/Bouncing Balls"
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
            fixed4 color = fixed4(0,0,0,0);

            fixed _r = 0.05;
            fixed _x_center = (sin(0.125 * pow(_Time.y,1.5)) + 1) * 0.25;
            fixed _y_center = _x_center * 2;

            if (_x_center < 0.25)
                _y_center = _x_center * 2;
            else
                _y_center = -_x_center * 2 + 1;

            _x_center += 0.25;
            _y_center += 0.25;

            fixed r1 = sqrt
            (
                pow(fi.tex.x - _x_center,2) +
                pow(fi.tex.y - _y_center,2)
            );

            if (r1 < _r)
                color = fixed4(1,1,1,1);

            _x_center = (sin(0.125 * pow(_Time.y,1.5)) + 1) * 0.25;
            _y_center = 0;

            _x_center += 0.25;
            _y_center += 0.25;

            fixed r2 = sqrt
            (
                pow(fi.tex.x - _x_center,2) +
                pow(fi.tex.y - _y_center,2)
            );

            fixed c = max(0.5,abs(sin(0.125 * pow(_Time.y,1.5))));

            if (r2 < _r)
                color = fixed4(c,c,c,1);

            _x_center = (sin(0.125 * pow(_Time.y,1.5)) + 1) * 0.25;
            _y_center = 0;

            _x_center += 0.25;
            _y_center += 0.75;

            fixed r3 = sqrt
            (
                pow(fi.tex.x - _x_center,2) +
                pow(fi.tex.y - _y_center,2)
            );

            c = max(0.5,1 - abs(sin(0.125 * pow(_Time.y,1.5))));

            if (r3 < _r)
                color = fixed4(c,c,c,1);

            return color;
         }
         ENDCG
     }
 }
}
