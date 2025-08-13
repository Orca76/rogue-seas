Shader "Custom/SpriteAdditiveGlow"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Color   ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,10)) = 3
        _Softness  ("Softness", Range(0,2)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One One     // �����Z����

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float  _Intensity;
            float  _Softness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                // �A���t�@�������_�炩���i�O�����Ȃ߂炩�Ɂj
                float a = saturate(pow(tex.a, 1.0 + _Softness * 2.0));
                fixed3 rgb = tex.rgb * _Color.rgb * i.color.rgb;
                fixed3 glow = rgb * _Intensity * a;

                return fixed4(glow, a); // ���Z�Ȃ̂�Alpha�͌����ڂɉe�����Ȃ�
            }
            ENDCG
        }
    }
}
