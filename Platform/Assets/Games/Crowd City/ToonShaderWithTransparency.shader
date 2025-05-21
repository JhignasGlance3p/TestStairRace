Shader "Toon/ToonShaderWithTransparency"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 0.5)  // Default to semi-transparent black
        _MainTex ("Texture", 2D) = "white" {}    // Main texture of the model
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            uniform float4 _Color;
            uniform sampler2D _MainTex;
            
            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}