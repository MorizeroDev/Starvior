// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Render DOF Factor" {
	Properties{
	_MainTex("Base", 2D) = "white" {}
	_Cutoff("Cutoff", float) = 0.5
	}
		// Helper code used in all of the below subshaders
		CGINCLUDE
		struct v2f {
		float4 pos : POSITION;
		float depth : TEXCOORD0;
	};
	struct v2f_uv {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float depth : TEXCOORD1;
	};
	uniform float4 _FocalParams;
	half DOFFactor(float z) {
		float focalDist = _FocalParams.x;
		float invRange = _FocalParams.w;
		float fromFocal = z - focalDist;
		if (fromFocal < 0.0)
			fromFocal *= 4.0;
		return saturate(abs(fromFocal) * invRange);
	}
	uniform sampler2D _MainTex;
	uniform float _Cutoff;
	half4 frag(v2f i) : COLOR{
	return DOFFactor(i.depth);
	}
		half4 frag_uv(v2f_uv i) : COLOR{
		half4 texcol = tex2D(_MainTex, i.uv);
		clip(texcol.a - _Cutoff);
		return DOFFactor(i.depth);
	}
		ENDCG
		Category {
		Fog{ Mode Off }
			// regular opaque objects
			SubShader{
			Tags { "RenderType" = "Opaque" }
			Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			v2f vert(appdata_base v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			COMPUTE_EYEDEPTH(o.depth);
			return o;
			}
			ENDCG
			}
		}
			// transparent cutout objects
				SubShader{
				Tags { "RenderType" = "TransparentCutout" }
				Pass {
				Cull Off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_uv
				#include "UnityCG.cginc"
				v2f_uv vert(appdata_base v) {
				v2f_uv o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				COMPUTE_EYEDEPTH(o.depth);
				return o;
				}
				ENDCG
				}
			}
				// terrain tree bark
					SubShader{
					Tags { "RenderType" = "TreeOpaque" }
					Pass {
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"
					#include "TerrainEngine.cginc"
					struct appdata {
					float4 vertex : POSITION;
					float4 color : COLOR;
					};
					v2f vert(appdata v) {
					v2f o;
					TerrainAnimateTree(v.vertex, v.color.w);
					o.pos = UnityObjectToClipPos(v.vertex);
					COMPUTE_EYEDEPTH(o.depth);
					return o;
					}
					ENDCG
					}
				}
					// terrain tree leaves
						SubShader{
						Tags { "RenderType" = "TreeTransparentCutout" }
						Pass {
						Cull Off
						CGPROGRAM
						#pragma vertex vert
						#pragma fragment frag_uv
						#include "UnityCG.cginc"
						#include "TerrainEngine.cginc"
						struct appdata {
						float4 vertex : POSITION;
						float4 color : COLOR;
						float4 texcoord : TEXCOORD0;
						};
						v2f_uv vert(appdata v) {
						v2f_uv o;
						TerrainAnimateTree(v.vertex, v.color.w);
						o.pos = UnityObjectToClipPos(v.vertex);
						o.uv = v.texcoord;
						COMPUTE_EYEDEPTH(o.depth);
						return o;
						}
						ENDCG
						}
					}
						// terrain tree billboards
							#warning Upgrade NOTE: SubShader commented out; uses TerrainBillboardTree with 2 arguments; needs to have 3
/*SubShader{
							Tags { "RenderType" = "TreeBillboard" }
							Pass {
							Cull Off
							CGPROGRAM
							#pragma vertex vert
							142
							#pragma fragment frag_tree
							#include "UnityCG.cginc"
							#include "TerrainEngine.cginc"
							struct appdata {
							float4 vertex : POSITION;
							float4 color : COLOR;
							float4 texcoord : TEXCOORD0;
							};
							v2f_uv vert(appdata_tree_billboard v) {
							v2f_uv o;
							TerrainBillboardTree(v.vertex, v.texcoord1.xy);
							o.pos = UnityObjectToClipPos(v.vertex);
							o.uv = v.texcoord;
							COMPUTE_EYEDEPTH(o.depth);
							return o;
							}
							half4 frag_tree(v2f_uv i) : COLOR {
							half4 texcol = tex2D(_MainTex, i.uv);
							clip(texcol.a - 0.5);
							return DOFFactor(i.depth);
							}
							ENDCG
							}
						}*/
							// terrain grass billboards
								#warning Upgrade NOTE: SubShader commented out; uses TerrainWaveGrass with 4 arguments; needs to have 3
/*SubShader{
								Tags { "RenderType" = "GrassBillboard" }
								Pass {
								Cull Off
								CGPROGRAM
								#pragma vertex vert
								#pragma fragment frag_uv
								#pragma multi_compile NO_INTEL_GMA_X3100_WORKAROUND INTEL_GMA_X3100_WORKAROUND
								#include "UnityCG.cginc"
								#include "TerrainEngine.cginc"
								v2f_uv vert(appdata_grass v) {
								v2f_uv o;
								TerrainBillboardGrass(v.vertex, v.texcoord1.xy);
								float waveAmount = v.texcoord1.y;
								float4 dummyColor = 0;
								TerrainWaveGrass(v.vertex, waveAmount, dummyColor, dummyColor);
								o.pos = UnityObjectToClipPos(v.vertex);
								o.uv = v.texcoord;
								COMPUTE_EYEDEPTH(o.depth);
								return o;
								}
								ENDCG
								}
							}*/
								// terrain grass non-billboards
									#warning Upgrade NOTE: SubShader commented out; uses TerrainWaveGrass with 4 arguments; needs to have 3
/*SubShader{
									Tags { "RenderType" = "Grass" }
									Pass {
									Cull Off
									CGPROGRAM
									#pragma vertex vert
									#pragma fragment frag_uv
									#pragma multi_compile NO_INTEL_GMA_X3100_WORKAROUND INTEL_GMA_X3100_WORKAROUND
									#include "UnityCG.cginc"
									#include "TerrainEngine.cginc"
									v2f_uv vert(appdata_grass v) {
									v2f_uv o;
									float waveAmount = v.color.a * _WaveAndDistance.z;
									float4 dummyColor = 0;
									TerrainWaveGrass(v.vertex, waveAmount, dummyColor, dummyColor);
									o.pos = UnityObjectToClipPos(v.vertex);
									o.uv = v.texcoord;
									COMPUTE_EYEDEPTH(o.depth);
									return o;
									}
									ENDCG
									}
								}*/
	}
}