// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Exo-Planets/Legacy/SunAtmosphere"
{
	Properties
	{
		_ExteriorIntensity("Exterior Intensity", Float) = 0.7
		_AtmosphereSize("Atmosphere Size", Range( -1 , 5)) = 2
		_Color0("Color 0", Color) = (0.9755421,0.2198126,0,0)

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One One
		Cull Front
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float3 ase_normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform float _AtmosphereSize;
			uniform float4 _Color0;
			uniform float _ExteriorIntensity;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.ase_texcoord.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.w = 0;
				o.ase_texcoord1.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( (0.0 + (_AtmosphereSize - 0.0) * (1.0 - 0.0) / (3.0 - 0.0)) * ( v.vertex.xyz * 1 ) );
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				float3 ase_worldPos = i.ase_texcoord.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = i.ase_texcoord1.xyz;
				float dotResult162 = dot( ase_worldViewDir , ase_worldNormal );
				float clampResult164 = clamp( -dotResult162 , 0.0 , 1.0 );
				float3 temp_cast_0 = (clampResult164).xxx;
				float3 temp_cast_1 = (clampResult164).xxx;
				float3 gammaToLinear165 = GammaToLinearSpace( temp_cast_1 );
				float3 saferPower167 = max( gammaToLinear165 , 0.0001 );
				float3 temp_cast_2 = (5.0).xxx;
				float4 blendOpSrc170 = _Color0;
				float4 blendOpDest170 = float4( pow( saferPower167 , temp_cast_2 ) , 0.0 );
				float4 clampResult172 = clamp( ( blendOpSrc170 * blendOpDest170 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				
				
				finalColor = ( clampResult172 * _ExteriorIntensity );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17700
-1680;203;1680;989;-838.4614;471.3922;1.900681;True;False
Node;AmplifyShaderEditor.WorldNormalVector;173;527.1054,249.5905;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;161;518.1757,66.7643;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;162;803.6415,139.194;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;163;932.4305,164.0563;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;164;1072.107,161.1285;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GammaToLinearNode;165;1252.687,156.566;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;166;1239.684,271.6344;Float;False;Constant;_Float8;Float 8;11;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;160;1446.969,-159.0345;Float;False;Property;_Color0;Color 0;2;0;Create;True;0;0;False;0;0.9755421,0.2198126,0,0;0.9755421,0.2198126,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;167;1509.612,149.487;Inherit;True;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;3;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;154;2979.121,414.6427;Inherit;False;728.4116;455.2193;Comment;4;159;158;157;156;First shader pass Output;0,0.7511432,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;169;2272.313,458.5973;Float;False;Property;_AtmosphereSize;Atmosphere Size;1;0;Create;True;0;0;False;0;2;1;-1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;170;1984.336,35.16469;Inherit;False;Multiply;False;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;155;2171.862,188.0016;Inherit;False;346;136;Material Property;1;171;;0,1,0.3411765,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;157;2994.323,751.1057;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;172;2266.846,28.40004;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;171;2227.362,238.4498;Float;False;Property;_ExteriorIntensity;Exterior Intensity;0;0;Create;True;0;0;False;0;0.7;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;158;3185.954,747.9562;Inherit;False;1;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;156;3004.544,569.2853;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;3;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;174;2568.221,68.86781;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;3307.664,624.2326;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;177;3749.603,308.1215;Float;False;True;-1;2;ASEMaterialInspector;100;1;Exo-Planets/Legacy/SunAtmosphere;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;4;1;False;-1;1;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;1;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;162;0;161;0
WireConnection;162;1;173;0
WireConnection;163;0;162;0
WireConnection;164;0;163;0
WireConnection;165;0;164;0
WireConnection;167;0;165;0
WireConnection;167;1;166;0
WireConnection;170;0;160;0
WireConnection;170;1;167;0
WireConnection;172;0;170;0
WireConnection;158;0;157;0
WireConnection;156;0;169;0
WireConnection;174;0;172;0
WireConnection;174;1;171;0
WireConnection;159;0;156;0
WireConnection;159;1;158;0
WireConnection;177;0;174;0
WireConnection;177;1;159;0
ASEEND*/
//CHKSM=9C6E525B1BE4E2F5F969302C9FBB963B1D43FEEF