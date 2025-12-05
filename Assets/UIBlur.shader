Shader "Custom/UIHighQualityBokeh"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // --- 核心參數 ---
        _BlurSize ("Blur Radius (模糊半徑)", Range(0, 20)) = 3.0
        // 採樣次數建議 20-40 之間，越高越細緻但越耗效能
        [IntRange] _Iteration ("Quality (顆粒細緻度)", Range(10, 60)) = 30
        
        // --- UI 必要屬性 ---
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
            #pragma target 3.0

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
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            float _BlurSize;
            int _Iteration;

            // 黃金角度 (Golden Angle) 約為 2.39996323 弧度
            static const float GOLDEN_ANGLE = 2.39996323;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                // 初始化顏色累積與權重
                half4 colorSum = half4(0, 0, 0, 0);
                // 為了避免除以零，初始給一點點權重
                float weightSum = 0.001; 
                
                float2 res = _MainTex_TexelSize.xy;
                
                // 旋轉矩陣參數
                float s, c;
                
                // 核心迴圈：黃金角度螺旋採樣
                // 這種算法能用最少的點，達到最圓潤的覆蓋率
                for (int i = 0; i < _Iteration; i++)
                {
                    // 1. 計算角度：每次增加黃金角度
                    float theta = i * GOLDEN_ANGLE;
                    sincos(theta, s, c);
                    
                    // 2. 計算半徑：半徑隨索引的平方根增加，確保採樣點分佈均勻
                    float r = sqrt((float)i) * _BlurSize;
                    
                    // 3. 計算偏移 UV
                    // 這裡組合了旋轉 (cos, sin) 與半徑擴展
                    float2 offset = float2(c * r, s * r) * res;
                    float2 uv = IN.texcoord + offset;

                    // 4. 採樣並累積
                    // 這裡不做複雜的高斯權重計算，因為螺旋分佈本身就是一種加權
                    // 這樣可以省下 exp() 的昂貴開銷
                    half4 sampleColor = tex2D(_MainTex, uv);
                    
                    colorSum += sampleColor;
                    weightSum += 1.0;
                }

                // 計算平均值
                fixed4 col = colorSum / weightSum;

                // --- UI 標準處理 ---
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