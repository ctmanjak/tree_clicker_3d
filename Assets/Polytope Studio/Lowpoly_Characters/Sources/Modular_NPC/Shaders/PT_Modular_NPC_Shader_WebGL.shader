// WebGL 호환 간소화 버전 - 원본: PT_Modular_NPC_Shader_PBR
// 원본과 동일한 색상 ID 맵 기반 마스킹 시스템 사용
Shader "Polytope Studio/PT_Medieval Modular NPC Shader WebGL"
{
    Properties
    {
        [Header(Skin)]
        [HDR]_SKINCOLOR("SKIN COLOR", Color) = (2.02193,1.0081,0.6199315,1)
        _SKINSMOOTHNESS("SKIN SMOOTHNESS", Range(0, 1)) = 0.154

        [Header(Eyes)]
        [HDR]_EYESCOLOR("EYES COLOR", Color) = (0.0734529,0.1320755,0.05046281,1)
        _EYESSMOOTHNESS("EYES SMOOTHNESS", Range(0, 1)) = 0.7
        [HDR]_SCLERACOLOR("SCLERA COLOR", Color) = (0.9056604,0.8159487,0.8159487,1)
        _SCLERASMOOTHNESS("SCLERA SMOOTHNESS", Range(0, 1)) = 0.463

        [Header(Hair)]
        [HDR]_HAIRCOLOR("HAIR COLOR", Color) = (0.24528301,0.06965111,0.005784976,1)
        _HAIRSMOOTHNESS("HAIR SMOOTHNESS", Range(0, 1)) = 0.1

        [Header(Face)]
        [HDR]_LIPSCOLOR("LIPS COLOR", Color) = (0.6981132,0.23067373,0.19428623,1)
        _LIPSSMOOTHNESS("LIPS SMOOTHNESS", Range(0, 1)) = 0.4

        [Header(Other)]
        [HDR]_OTHERCOLOR("OTHER COLOR", Color) = (0.6792453,0.2755839,0.05446779,1)
        _OTHERSMOOTHNESS("OTHER SMOOTHNESS", Range(0, 1)) = 0.3

        [Header(Metal)]
        [HDR]_METAL1COLOR("METAL 1 COLOR", Color) = (0.8792791,0.9922886,1.007606,1)
        _METAL1METALLIC("METAL 1 METALLIC", Range(0, 1)) = 0.765
        _METAL1SMOOTHNESS("METAL 1 SMOOTHNESS", Range(0, 1)) = 0.574
        [HDR]_METAL2COLOR("METAL 2 COLOR", Color) = (0.81301177,0.81301177,0.8490566,1)
        _METAL2METALLIC("METAL 2 METALLIC", Range(0, 1)) = 0.695
        _METAL2SMOOTHNESS("METAL 2 SMOOTHNESS", Range(0, 1)) = 0.7
        [HDR]_METAL3COLOR("METAL 3 COLOR", Color) = (0.4528302,0.4528302,0.4528302,1)
        _METAL3METALLIC("METAL 3 METALLIC", Range(0, 1)) = 0.65
        _METAL3SMOOTHNESS("METAL 3 SMOOTHNESS", Range(0, 1)) = 0.7
        [HDR]_METAL4COLOR("METAL 4 COLOR", Color) = (0.3490566,0.3490566,0.3490566,1)
        _METAL4METALLIC("METAL 4 METALLIC", Range(0, 1)) = 0.758
        _METAL4SMOOTHNESS("METAL 4 SMOOTHNESS", Range(0, 1)) = 0.766

        [Header(Leather)]
        [HDR]_LEATHER1COLOR("LEATHER 1 COLOR", Color) = (0.3018868,0.032959573,0,1)
        _LEATHER1SMOOTHNESS("LEATHER 1 SMOOTHNESS", Range(0, 1)) = 0.515
        [HDR]_LEATHER2COLOR("LEATHER 2 COLOR", Color) = (0.18867922,0.026124813,0,1)
        _LEATHER2SMOOTHNESS("LEATHER 2 SMOOTHNESS", Range(0, 1)) = 0.506
        [HDR]_LEATHER3COLOR("LEATHER 3 COLOR", Color) = (0.8490566,0.31753337,0,1)
        _LEATHER3SMOOTHNESS("LEATHER 3 SMOOTHNESS", Range(0, 1)) = 0.3
        [HDR]_LEATHER4COLOR("LEATHER 4 COLOR", Color) = (0.5283019,0.23091508,0,1)
        _LEATHER4SMOOTHNESS("LEATHER 4 SMOOTHNESS", Range(0, 1)) = 0.3

        [Header(Cloth)]
        [HDR]_CLOTH1COLOR("CLOTH 1 COLOR", Color) = (1.2378799,1.2378799,1.2378799,1)
        [HDR]_CLOTH2COLOR("CLOTH 2 COLOR", Color) = (0.6132076,0.39048594,0.39373732,1)
        [HDR]_CLOTH3COLOR("CLOTH 3 COLOR", Color) = (0.3490566,0.106353186,0,1)
        [HDR]_CLOTH4COLOR("CLOTH 4 COLOR", Color) = (0.3962264,0.3962264,0.3962264,1)

        [Header(Feathers)]
        [HDR]_FEATHERS1COLOR("FEATHERS 1 COLOR", Color) = (0.7735849,0.492613,0.492613,1)
        [HDR]_FEATHERS2COLOR("FEATHERS 2 COLOR", Color) = (1,0,0,1)

        [Header(Coat of Arms)]
        [HDR]_COATOFARMSCOLOR("COAT OF ARMS COLOR", Color) = (1,0,0,1)
        [NoScaleOffset]_COATOFARMSMASK("COAT OF ARMS MASK", 2D) = "black" {}

        [Header(Settings)]
        _OCCLUSION("OCCLUSION", Range(0, 1)) = 0.5
        [Toggle]_MetalicOn("Metalic On", Float) = 1
        [Toggle]_SmoothnessOn("Smoothness On", Float) = 1
        [ToggleOff(_RECEIVE_SHADOWS_OFF)]_ReceiveShadows("Receive Shadows", Float) = 1.0

        [Header(Textures)]
        [NoScaleOffset]_TextureSample0("Color ID Map", 2D) = "white" {}
        [NoScaleOffset]_TextureSample2("Base Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend One Zero
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma prefer_hlslcc gles

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

            TEXTURE2D(_TextureSample0);
            SAMPLER(sampler_TextureSample0);
            TEXTURE2D(_TextureSample2);
            SAMPLER(sampler_TextureSample2);
            TEXTURE2D(_COATOFARMSMASK);
            SAMPLER(sampler_COATOFARMSMASK);

            CBUFFER_START(UnityPerMaterial)
                float4 _SKINCOLOR;
                float _SKINSMOOTHNESS;
                float4 _EYESCOLOR;
                float _EYESSMOOTHNESS;
                float4 _SCLERACOLOR;
                float _SCLERASMOOTHNESS;
                float4 _HAIRCOLOR;
                float _HAIRSMOOTHNESS;
                float4 _LIPSCOLOR;
                float _LIPSSMOOTHNESS;
                float4 _OTHERCOLOR;
                float _OTHERSMOOTHNESS;
                float4 _METAL1COLOR;
                float _METAL1METALLIC;
                float _METAL1SMOOTHNESS;
                float4 _METAL2COLOR;
                float _METAL2METALLIC;
                float _METAL2SMOOTHNESS;
                float4 _METAL3COLOR;
                float _METAL3METALLIC;
                float _METAL3SMOOTHNESS;
                float4 _METAL4COLOR;
                float _METAL4METALLIC;
                float _METAL4SMOOTHNESS;
                float4 _LEATHER1COLOR;
                float _LEATHER1SMOOTHNESS;
                float4 _LEATHER2COLOR;
                float _LEATHER2SMOOTHNESS;
                float4 _LEATHER3COLOR;
                float _LEATHER3SMOOTHNESS;
                float4 _LEATHER4COLOR;
                float _LEATHER4SMOOTHNESS;
                float4 _CLOTH1COLOR;
                float4 _CLOTH2COLOR;
                float4 _CLOTH3COLOR;
                float4 _CLOTH4COLOR;
                float4 _FEATHERS1COLOR;
                float4 _FEATHERS2COLOR;
                float4 _COATOFARMSCOLOR;
                float _OCCLUSION;
                float _MetalicOn;
                float _SmoothnessOn;
            CBUFFER_END

            // 색상 매칭 함수 - 원본과 동일한 로직
            float ColorMatch(float3 idColor, float3 targetColor)
            {
                float dist = distance(idColor, targetColor);
                return saturate(1.0 - ((dist - 0.1) / max(0.0, 1e-05)));
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = input.uv;
                output.uv2 = input.uv2;
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                output.shadowCoord = TransformWorldToShadowCoord(posInputs.positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 텍스처 샘플링
                float4 colorIdMap = SAMPLE_TEXTURE2D(_TextureSample0, sampler_TextureSample0, input.uv);
                float4 baseTex = SAMPLE_TEXTURE2D(_TextureSample2, sampler_TextureSample2, input.uv);
                float coatMask = 1.0 - SAMPLE_TEXTURE2D(_COATOFARMSMASK, sampler_COATOFARMSMASK, input.uv2).a;

                float3 idColor = colorIdMap.rgb;

                // 색상 ID 정의 - Gamma/Linear Space 자동 처리 (원본과 동일)
                // 0.4980392 in Gamma = 0.2122307 in Linear
                float mid = 0.4980392;
                #ifndef UNITY_COLORSPACE_GAMMA
                    mid = 0.2122307;
                #endif

                float3 colSkin = float3(mid, 0, 0);
                float3 colHair = float3(1, mid, 0);
                float3 colEyes = float3(1, 0, 0);
                float3 colSclera = float3(mid, mid, mid);
                float3 colLips = float3(mid, mid, 0);
                float3 colOther = float3(1, 1, 0);
                float3 colMetal1 = float3(mid, 0, mid);
                float3 colMetal2 = float3(0, 0, mid);
                float3 colMetal3 = float3(0, mid, mid);
                float3 colMetal4 = float3(mid, mid, 1);
                float3 colLeather1 = float3(1, mid, 1);
                float3 colLeather2 = float3(1, 0, 1);
                float3 colLeather3 = float3(1, 1, mid);
                float3 colLeather4 = float3(1, mid, mid);
                float3 colCloth1 = float3(0, mid, 0);
                float3 colCloth2 = float3(0, 1, 0);
                float3 colCloth3 = float3(0, 1, 1);
                float3 colCloth4 = float3(0, 0, 1);
                float3 colFeathers1 = float3(mid, 1, mid);
                float3 colFeathers2 = float3(mid, 1, 1);

                // 마스크 계산
                float maskSkin = ColorMatch(idColor, colSkin);
                float maskHair = ColorMatch(idColor, colHair);
                float maskEyes = ColorMatch(idColor, colEyes);
                float maskSclera = ColorMatch(idColor, colSclera);
                float maskLips = ColorMatch(idColor, colLips);
                float maskOther = ColorMatch(idColor, colOther);
                float maskMetal1 = ColorMatch(idColor, colMetal1);
                float maskMetal2 = ColorMatch(idColor, colMetal2);
                float maskMetal3 = ColorMatch(idColor, colMetal3);
                float maskMetal4 = ColorMatch(idColor, colMetal4);
                float maskLeather1 = ColorMatch(idColor, colLeather1);
                float maskLeather2 = ColorMatch(idColor, colLeather2);
                float maskLeather3 = ColorMatch(idColor, colLeather3);
                float maskLeather4 = ColorMatch(idColor, colLeather4);
                float maskCloth1 = ColorMatch(idColor, colCloth1);
                float maskCloth2 = ColorMatch(idColor, colCloth2);
                float maskCloth3 = ColorMatch(idColor, colCloth3);
                float maskCloth4 = ColorMatch(idColor, colCloth4);
                float maskFeathers1 = ColorMatch(idColor, colFeathers1);
                float maskFeathers2 = ColorMatch(idColor, colFeathers2);

                // 알베도 계산 - 원본과 동일한 순서로 lerp
                float3 albedo = float3(0, 0, 0);
                albedo = lerp(albedo, baseTex.rgb * _FEATHERS2COLOR.rgb, maskFeathers2);
                albedo = lerp(albedo, baseTex.rgb * _FEATHERS1COLOR.rgb, maskFeathers1);
                albedo = lerp(albedo, baseTex.rgb * _CLOTH4COLOR.rgb, maskCloth4);
                albedo = lerp(albedo, baseTex.rgb * _CLOTH3COLOR.rgb, maskCloth3);
                albedo = lerp(albedo, baseTex.rgb * _CLOTH2COLOR.rgb, maskCloth2);
                albedo = lerp(albedo, baseTex.rgb * _CLOTH1COLOR.rgb, maskCloth1);
                albedo = lerp(albedo, baseTex.rgb * _LEATHER4COLOR.rgb, maskLeather4);
                albedo = lerp(albedo, baseTex.rgb * _LEATHER3COLOR.rgb, maskLeather3);
                albedo = lerp(albedo, baseTex.rgb * _LEATHER2COLOR.rgb, maskLeather2);
                albedo = lerp(albedo, baseTex.rgb * _LEATHER1COLOR.rgb, maskLeather1);
                albedo = lerp(albedo, baseTex.rgb * _METAL4COLOR.rgb, maskMetal4);
                albedo = lerp(albedo, baseTex.rgb * _METAL3COLOR.rgb, maskMetal3);
                albedo = lerp(albedo, baseTex.rgb * _METAL2COLOR.rgb, maskMetal2);
                albedo = lerp(albedo, baseTex.rgb * _METAL1COLOR.rgb, maskMetal1);
                albedo = lerp(albedo, baseTex.rgb * _OTHERCOLOR.rgb, maskOther);
                albedo = lerp(albedo, baseTex.rgb * _LIPSCOLOR.rgb, maskLips);
                albedo = lerp(albedo, baseTex.rgb * _SCLERACOLOR.rgb, maskSclera);
                albedo = lerp(albedo, baseTex.rgb * _EYESCOLOR.rgb, maskEyes);
                albedo = lerp(albedo, baseTex.rgb * _HAIRCOLOR.rgb, maskHair);
                albedo = lerp(albedo, baseTex.rgb * _SKINCOLOR.rgb, maskSkin);

                // Coat of Arms 적용
                albedo = lerp(albedo, _COATOFARMSCOLOR.rgb, (1.0 - coatMask));

                // Metallic 계산
                float metallic = 0.0;
                metallic = lerp(metallic, _METAL4METALLIC, maskMetal4);
                metallic = lerp(metallic, _METAL3METALLIC, maskMetal3);
                metallic = lerp(metallic, _METAL2METALLIC, maskMetal2);
                metallic = lerp(metallic, _METAL1METALLIC, maskMetal1);
                metallic = _MetalicOn > 0.5 ? metallic : 0.0;

                // Smoothness 계산
                float smoothness = 0.0;
                smoothness = lerp(smoothness, _LEATHER4SMOOTHNESS, maskLeather4);
                smoothness = lerp(smoothness, _LEATHER3SMOOTHNESS, maskLeather3);
                smoothness = lerp(smoothness, _LEATHER2SMOOTHNESS, maskLeather2);
                smoothness = lerp(smoothness, _LEATHER1SMOOTHNESS, maskLeather1);
                smoothness = lerp(smoothness, _METAL4SMOOTHNESS, maskMetal4);
                smoothness = lerp(smoothness, _METAL3SMOOTHNESS, maskMetal3);
                smoothness = lerp(smoothness, _METAL2SMOOTHNESS, maskMetal2);
                smoothness = lerp(smoothness, _METAL1SMOOTHNESS, maskMetal1);
                smoothness = lerp(smoothness, _OTHERSMOOTHNESS, maskOther);
                smoothness = lerp(smoothness, _LIPSSMOOTHNESS, maskLips);
                smoothness = lerp(smoothness, _SCLERASMOOTHNESS, maskSclera);
                smoothness = lerp(smoothness, _EYESSMOOTHNESS, maskEyes);
                smoothness = lerp(smoothness, _HAIRSMOOTHNESS, maskHair);
                smoothness = lerp(smoothness, _SKINSMOOTHNESS, maskSkin);
                smoothness = _SmoothnessOn > 0.5 ? smoothness : 0.0;

                // 라이팅 계산
                float3 normalWS = normalize(input.normalWS);

                // Occlusion
                float occlusion = 1.0 + (_OCCLUSION) * (0.5 - 1.0);

                // 가짜 AO: 노말이 아래쪽을 향하면 어둡게
                float fakeAO = saturate(normalWS.y * 0.5 + 0.5); // -1~1 -> 0~1
                fakeAO = lerp(0.4, 1.0, fakeAO); // 최소 0.4, 최대 1.0
                occlusion *= fakeAO;
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                // 그림자 좌표
                float4 shadowCoord = float4(0, 0, 0, 0);
                #if !defined(_RECEIVE_SHADOWS_OFF)
                    #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                        shadowCoord = input.shadowCoord;
                    #endif
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.fogCoord = input.fogFactor;
                inputData.shadowCoord = shadowCoord;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = saturate(metallic);
                surfaceData.specular = float3(0.5, 0.5, 0.5);
                surfaceData.smoothness = saturate(smoothness);
                surfaceData.normalTS = float3(0, 0, 1);
                surfaceData.occlusion = occlusion;
                surfaceData.emission = float3(0, 0, 0);
                surfaceData.alpha = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, input.fogFactor);
                color.a = 1.0; // Opaque 셰이더이므로 alpha 강제

                return color;
            }
            ENDHLSL
        }

        // 그림자 패스
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma prefer_hlslcc gles

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Depth 패스
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma prefer_hlslcc gles

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}
