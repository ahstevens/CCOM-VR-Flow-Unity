Shader "Custom/Rings"
{
    Properties
    {
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SpecColor("Specular Color", Color) = (0.5,0.5,0.5,0.5)
        _Specular("Specular", Range(0,1)) = 0.5
        _Glossiness ("Glossiness", Range(0,500)) = 1
        _Amount("Extrusion Amount", Range(-1,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200

        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf BlinnPhong fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        //sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 vertColor : COLOR;
            float3 normal : NORMAL;
        };

        float _Amount;
        void vert(inout appdata_full v) {
            v.vertex.xyz += v.normal * _Amount;
        }

        half _Specular;
        fixed _Glossiness;
        fixed4 _RingColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 ring = frac((floor(IN.uv_MainTex*2) / 2).x) * 2;

            if (_RingColor.a == 0)
            {
                clip(-ring);
            }

            o.Albedo = lerp(IN.vertColor, _RingColor, ring.x);
                        
            o.Specular = _Specular;
            o.Gloss = _Glossiness;
            o.Alpha = _RingColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
