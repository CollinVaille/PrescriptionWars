// Upgrade NOTE: upgraded instancing buffer 'ExoPlanetsLegacyRings' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Exo-Planets/Legacy/Rings"
{
	Properties
	{
		[NoScaleOffset]_RingsTexture("Rings Texture", 2D) = "white" {}
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_BaseColor("Base Color", Color) = (0,0,0,0)
		_Color1("Color", Color) = (0,0,0,0)
		_Coloroffset("Color offset", Range( 0 , 1)) = 0
		[HideInInspector]_LightSourceRings("_LightSourceRings", Vector) = (1,0,0,0)
		_Size("Size", Range( 0 , 5)) = 0.6
		_ShadowsIntensity("Shadows Intensity", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent-16" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float _Size;
		uniform sampler2D _RingsTexture;
		uniform float4 _BaseColor;
		uniform float4 _Color1;
		uniform float _Coloroffset;
		uniform float _ShadowsIntensity;
		uniform float _Opacity;

		UNITY_INSTANCING_BUFFER_START(ExoPlanetsLegacyRings)
			UNITY_DEFINE_INSTANCED_PROP(float3, _LightSourceRings)
#define _LightSourceRings_arr ExoPlanetsLegacyRings
		UNITY_INSTANCING_BUFFER_END(ExoPlanetsLegacyRings)

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz += ( ase_vertex3Pos * _Size );
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float4 color57 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			o.Albedo = color57.rgb;
			float2 uv_RingsTexture1 = i.uv_texcoord;
			float4 tex2DNode1 = tex2D( _RingsTexture, uv_RingsTexture1 );
			float4 temp_output_49_0 = ( tex2DNode1 * _BaseColor );
			float4 blendOpSrc6 = temp_output_49_0;
			float4 blendOpDest6 = _Color1;
			float2 temp_cast_1 = (_Coloroffset).xx;
			float2 uv_TexCoord11 = i.uv_texcoord + temp_cast_1;
			float4 lerpResult9 = lerp( ( saturate( (( blendOpDest6 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest6 ) * ( 1.0 - blendOpSrc6 ) ) : ( 2.0 * blendOpDest6 * blendOpSrc6 ) ) )) , temp_output_49_0 , tex2D( _RingsTexture, uv_TexCoord11 ).a);
			float3 _LightSourceRings_Instance = UNITY_ACCESS_INSTANCED_PROP(_LightSourceRings_arr, _LightSourceRings);
			float3 normalizeResult19 = normalize( _LightSourceRings_Instance );
			float3 LightSourceVector21 = ( normalizeResult19 / 1.0 );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir61 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult31 = normalize( objToWorldDir61 );
			float dotResult22 = dot( LightSourceVector21 , normalizeResult31 );
			float clampResult44 = clamp( ( dotResult22 + 0.45 ) , 0.0 , 1.0 );
			float clampResult75 = clamp( ( _ShadowsIntensity + clampResult44 ) , 0.0 , 1.0 );
			float4 lerpResult28 = lerp( float4( 0,0,0,0 ) , lerpResult9 , clampResult75);
			float4 clampResult41 = clamp( ( lerpResult28 * _Color1.a ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Emission = clampResult41.rgb;
			o.Alpha = ( tex2DNode1.a * _Opacity );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17700
-1680;203;1680;989;-92.85248;1036.859;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;15;-515.9631,-907.8758;Inherit;False;1011.591;371.1455;Light Source Vector from script;5;21;20;19;18;16;Light Source Vector;1,0.6068678,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;16;-482.8538,-842.2879;Inherit;False;304;234;Input is set via LightSource.cs;1;17;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;17;-464.8538,-783.2879;Float;False;InstancedProperty;_LightSourceRings;_LightSourceRings;5;1;[HideInInspector];Create;True;0;0;False;0;1,0,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;18;-135.1235,-704.1375;Float;False;Constant;_Float2;Float 2;36;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;25;-1076.96,-475.5446;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;19;-110.3539,-844.7977;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;61;-806.4595,-589.6123;Inherit;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;20;80.74569,-727.4617;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;255.9721,-726.0272;Float;False;LightSourceVector;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;31;-199.9165,-485.9353;Inherit;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;22;565.5526,-620.2398;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;549.1597,-382.3046;Float;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;False;0;0.45;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;845.9198,-462.371;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-782.7291,330.1622;Float;False;Property;_Coloroffset;Color offset;4;0;Create;True;0;0;False;0;0;0.094;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-597.3553,-249.5238;Inherit;True;Property;_RingsTexture;Rings Texture;0;1;[NoScaleOffset];Create;True;0;0;False;0;-1;None;966045dcd1b1e5c448b8cbbed429f164;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;50;-514.7902,84.12048;Inherit;False;Property;_BaseColor;Base Color;2;0;Create;True;0;0;False;0;0,0,0,0;0.5309999,0.5309999,0.5309999,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;44;1039.581,-421.8224;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-172.4991,43.9577;Float;False;Property;_Color1;Color;3;0;Create;False;0;0;False;0;0,0,0,0;0,0.9534544,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;73;787.8525,-781.8588;Inherit;False;Property;_ShadowsIntensity;Shadows Intensity;7;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-61.4402,-219.21;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-409.3893,301.3984;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0.6,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;74;1149.853,-681.8588;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;108.3539,278.826;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;-1;None;966045dcd1b1e5c448b8cbbed429f164;True;0;False;white;Auto;False;Instance;1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;6;263.9862,-98.33589;Inherit;False;Overlay;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;75;1297.853,-627.8588;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;772.5723,-222.2541;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;28;1264.514,-228.3713;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;5;1215.683,368.9434;Float;False;Property;_Opacity;Opacity;1;0;Create;True;0;0;False;0;1;0.481;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;62;1491.919,503.8987;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;64;1505.919,720.8987;Inherit;False;Property;_Size;Size;6;0;Create;True;0;0;False;0;0.6;0.6;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;1455.529,-170.8495;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;1570.458,148.4492;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;57;1631.369,-407.9073;Inherit;False;Constant;_Color0;Color 0;6;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;29;-828.2434,-150.4851;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;58;-1096.459,-180.6123;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;41;1633.875,-141.8365;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;1879.919,563.8987;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;72;2015.196,-124.3065;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Exo-Planets/Legacy/Rings;False;False;False;False;True;True;True;True;True;True;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;-16;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;0;17;0
WireConnection;61;0;25;0
WireConnection;20;0;19;0
WireConnection;20;1;18;0
WireConnection;21;0;20;0
WireConnection;31;0;61;0
WireConnection;22;0;21;0
WireConnection;22;1;31;0
WireConnection;39;0;22;0
WireConnection;39;1;40;0
WireConnection;44;0;39;0
WireConnection;49;0;1;0
WireConnection;49;1;50;0
WireConnection;11;1;13;0
WireConnection;74;0;73;0
WireConnection;74;1;44;0
WireConnection;12;1;11;0
WireConnection;6;0;49;0
WireConnection;6;1;3;0
WireConnection;75;0;74;0
WireConnection;9;0;6;0
WireConnection;9;1;49;0
WireConnection;9;2;12;4
WireConnection;28;1;9;0
WireConnection;28;2;75;0
WireConnection;45;0;28;0
WireConnection;45;1;3;4
WireConnection;4;0;1;4
WireConnection;4;1;5;0
WireConnection;29;0;25;0
WireConnection;58;0;25;0
WireConnection;41;0;45;0
WireConnection;65;0;62;0
WireConnection;65;1;64;0
WireConnection;72;0;57;0
WireConnection;72;2;41;0
WireConnection;72;9;4;0
WireConnection;72;11;65;0
ASEEND*/
//CHKSM=0A94D579C7716614F3E5E730A4BE55C3DA36AC14