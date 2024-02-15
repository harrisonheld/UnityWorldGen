Shader "Custom/MultiTexture"
{
    Properties
    {
        _TextureArray ("Tex", 2DArray) = "" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // texture arrays are not available everywhere,
            // only compile shader on platforms where they are
            #pragma require 2darray
            
            #include "UnityCG.cginc"

            // The data we want to get in from the vertex shader.
            struct appdata
            {
                // vertex pos 
                float4 vertex : POSITION;
                // mesh.uv
                float4 texcoord0 : TEXCOORD0;
                // mesh.uv2
                float4 texcoord1 : TEXCOORD1;
            };
            
            // the data to pass to the fragment shader
            struct v2f
            {
                // vertex position
                float4 vertex : SV_POSITION;
                // x and y specify the texture coordinate. z specifies which texture to sample.
                float3 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.texcoord0.xy + 0.5;
                o.uv.z = v.texcoord1.x;
                return o;
            }
            
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);

            half4 frag (v2f i) : SV_Target
            {
                return UNITY_SAMPLE_TEX2DARRAY(_TextureArray, i.uv);
            }
            ENDCG
        }
    }
}