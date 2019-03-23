Shader "Custom/GreenScreenShader"
{
 Properties
 {
      
 }
 SubShader
 {
     // No culling or depth
     Cull Off ZWrite Off ZTest Always

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

         sampler2D _MainTex;
         float4 _MainTex_ST;
         
         FragInput vert (VertInput v)
         {
            FragInput fi;
                
            fi.pos = UnityObjectToClipPos(v.pos);
            fi.tex = v.tex;

            return fi;
         }
         
         fixed4 frag (FragInput i) : COLOR
         {
            if (i.tex.x < 0.15 || 0.85 < i.tex.x)
                discard;

            if (i.tex.y < 0.15 || 0.85 < i.tex.y)
                discard;

            fixed4 col = tex2D(_MainTex, i.tex + float2(0,sin(i.tex.x*0.75+_Time[2])*0.1));

            if (col.r < 0.9 || col.g < 0.9 || col.b < 0.9) discard;

            //if (0.25 < col.r & 0.25 < col.g & 0.25 < col.b) discard;

            //if (col.r < 0.25 & col.b < 0.5 & 0.25 < col.g) discard;



            return col;
         }
         ENDCG
     }
 }
}
