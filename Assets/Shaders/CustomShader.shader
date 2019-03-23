Shader "Custom/CustomShader"
{
 Properties
 {
     _MainTex ("Texture", 2D) = "white" {}
     _Flip ("Flip", Range(-1,1)) = 0
 }
 SubShader
 {
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
         float _Flip;
         
         FragInput vert (VertInput v)
         {
            FragInput fi;
                
            fi.pos = UnityObjectToClipPos(v.pos);
            fi.tex = v.tex;

            return fi;
         }
         
         fixed4 frag (FragInput i) : COLOR
         {
            fixed4 col = tex2D(_MainTex, sign(_Flip) * i.tex);

            if (0.3 < col.x & 0.3 < col.y & 0.3 < col.z)
                discard;

            float r = sqrt( pow( i.tex.x - 0.5 , 2 ) + pow( i.tex.y - 0.5 , 2 ) );

            if (r < 0.45)
                col += fixed4( 1, 0, 0, 1 );
            else
                discard;

            return col;
         }
         ENDCG
     }
 }
}
