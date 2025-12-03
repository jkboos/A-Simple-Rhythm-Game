Shader "Custom/UIHighQualityGaussian"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // --- 核心參數 ---
        _BlurSize ("Blur Size (模糊強度)", Range(0, 20)) = 3.0
        [IntRange] _Samples ("Quality (採樣次數 - 越高越細但越卡)", Range(3, 10)) = 5
        _StandardDeviation ("Sigma (柔和度)", Range(0.1, 10)) = 2.0
        
        // --- UI 必要屬性 (不用動) ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 // 需要 Shader Model 3.0 來支援複雜迴圈

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // 自動取得貼圖尺寸
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            // 自定義參數
            float _BlurSize;
            int _Samples;
            float _StandardDeviation;

            static const float PI = 3.14159265359;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            // 高斯分佈函數
            float gaussian(float x, float y, float sigma)
            {
                return (1.0 / (2.0 * PI * sigma * sigma)) * exp(-(x * x + y * y) / (2.0 * sigma * sigma));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float4 sum = float4(0, 0, 0, 0);
                float weightSum = 0;
                
                // 根據貼圖大小計算像素偏移
                float2 res = _MainTex_TexelSize.xy;
                
                // 為了效能，限制最大迴圈數
                int upper = _Samples;
                int lower = -upper;

                // 雙重迴圈進行高斯採樣 (Single Pass Gaussian)
                for (int x = lower; x <= upper; ++x)
                {
                    for (int y = lower; y <= upper; ++y)
                    {
                        // 計算偏移 UV
                        float2 offset = float2(x, y) * _BlurSize * res;
                        float2 uv = IN.texcoord + offset;

                        // 計算高斯權重
                        float weight = gaussian(x, y, _StandardDeviation);

                        sum += tex2D(_MainTex, uv) * weight;
                        weightSum += weight;
                    }
                }

                // 正規化顏色 (避免過亮或過暗)
                fixed4 col = sum / weightSum;

                // 處理 UI 顏色疊加與裁切
                col *= IN.color;
                col.a += _TextureSampleAdd.a;
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}