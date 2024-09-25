using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Utilities;

internal sealed class FogUtils
{
	private const double LN2_SQRT_DBL = 0.8325546111576977;

	private const float LN2_SQRT = 0.83255464f;

	private static readonly int s_FogColorPropertyId = Shader.PropertyToID("unity_FogColor");

	private static readonly int s_FogParamsPropertyId = Shader.PropertyToID("unity_FogParams");

	private static readonly GlobalKeyword s_FogLinearKeyword = new GlobalKeyword("FOG_LINEAR");

	private static readonly GlobalKeyword s_FogExpKeyword = new GlobalKeyword("FOG_EXP");

	private static readonly GlobalKeyword s_FogExp2Keyword = new GlobalKeyword("FOG_EXP2");

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetupFogMode(CommandBuffer cmd, FogMode fogMode)
	{
		cmd.SetKeyword(in s_FogLinearKeyword, fogMode == FogMode.Linear);
		cmd.SetKeyword(in s_FogExpKeyword, fogMode == FogMode.Exponential);
		cmd.SetKeyword(in s_FogExp2Keyword, fogMode == FogMode.ExponentialSquared);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetupFogProperties(CommandBuffer cmd, in Color fogColor, in float4 fogParams)
	{
		cmd.SetGlobalColor(s_FogColorPropertyId, fogColor);
		cmd.SetGlobalVector(s_FogParamsPropertyId, fogParams);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 MakeFogLinearParams(float startDistance, float endDistance)
	{
		if (endDistance <= startDistance)
		{
			endDistance = startDistance + 0.01f;
		}
		return new float4(0f, 0f, -1f / (endDistance - startDistance), endDistance / (endDistance - startDistance));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 MakeFogExpParams(float density)
	{
		return new float4(0f, density / 0.6931472f, 0f, 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 MakeFogExp2Params(float density)
	{
		return new float4(density / math.sqrt(0.6931472f), 0f, 0f, 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 MakeFogLinearParamsFromExp(float density)
	{
		return MakeFogLinearParams(0f, (0f - math.log2(0.100000024f)) * 0.6931472f / math.max(density, 0.001f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 MakeFogLinearParamsFromExp2(float density)
	{
		return MakeFogLinearParams(0f, (0f - math.log2(0.100000024f)) * 0.83255464f / math.max(density, 0.001f));
	}
}
