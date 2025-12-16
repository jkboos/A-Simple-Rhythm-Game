Shader "Custom/2D/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        
        // --- Outline Properties ---
        _OutlineColor ("Outline Color", Color) = (1,1,0,1) // 預設黃色
        _OutlineThickness ("Outline Thickness", Range(0, 10)) = 1
        _OutlineThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
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

        Cull Off
        Lighting Off
        ZWrite Off
        // 標準透明混合模式
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            
            // --- 修正處：正確宣告變數 ---
            fixed4 _OutlineColor; 
            float _OutlineThickness;
            float _OutlineThreshold;
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 originalPixel = tex2D(_MainTex, IN.texcoord);

                // 1. 如果當前像素是不透明的 (本體)，直接顯示原圖
                if (originalPixel.a > _OutlineThreshold)
                {
                    return originalPixel * IN.color;
                }

                // 2. 如果是透明的，開始檢查周圍有沒有「本體」
                float totalAlpha = 0;
                
                // 計算偏移量 (根據紋理大小和設定的厚度)
                float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;

                // 上下左右採樣
                totalAlpha += tex2D(_MainTex, IN.texcoord + float2(offset.x, 0)).a;  // 右
                totalAlpha += tex2D(_MainTex, IN.texcoord + float2(-offset.x, 0)).a; // 左
                totalAlpha += tex2D(_MainTex, IN.texcoord + float2(0, offset.y)).a;  // 上
                totalAlpha += tex2D(_MainTex, IN.texcoord + float2(0, -offset.y)).a; // 下

                // 3. 如果周圍有不透明像素，代表這裡是邊緣
                if (totalAlpha > _OutlineThreshold)
                {
                    // --- 修正處：使用 _OutlineColor 並乘上整體透明度 ---
                    return _OutlineColor * IN.color.a; 
                }

                // 4. 背景透明
                return fixed4(0,0,0,0);
            }
        ENDCG
        }
    }
}