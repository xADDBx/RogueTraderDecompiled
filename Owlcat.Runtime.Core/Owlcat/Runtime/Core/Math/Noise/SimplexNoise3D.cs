using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math.Noise;

public static class SimplexNoise3D
{
	private static float3 mod289(float3 x)
	{
		return x - math.floor(x / 289f) * 289f;
	}

	private static float4 mod289(float4 x)
	{
		return x - math.floor(x / 289f) * 289f;
	}

	private static float4 permute(float4 x)
	{
		return mod289((x * 34f + 1f) * x);
	}

	private static float4 taylorInvSqrt(float4 r)
	{
		return 1.7928429f - r * 0.85373473f;
	}

	public static float snoise(float3 v)
	{
		float2 @float = new float2(1f / 6f, 1f / 3f);
		float3 float2 = math.floor(v + math.dot(v, @float.yyy));
		float3 float3 = v - float2 + math.dot(float2, @float.xxx);
		float3 float4 = math.step(float3.yzx, float3.xyz);
		float3 float5 = 1f - float4;
		float3 float6 = math.min(float4.xyz, float5.zxy);
		float3 float7 = math.max(float4.xyz, float5.zxy);
		float3 float8 = float3 - float6 + @float.xxx;
		float3 float9 = float3 - float7 + @float.yyy;
		float3 float10 = float3 - 0.5f;
		float2 = mod289(float2);
		float4 float11 = permute(permute(permute(float2.z + new float4(0f, float6.z, float7.z, 1f)) + float2.y + new float4(0f, float6.y, float7.y, 1f)) + float2.x + new float4(0f, float6.x, float7.x, 1f));
		float4 float12 = float11 - 49f * math.floor(float11 / 49f);
		float4 float13 = math.floor(float12 / 7f);
		float4 float14 = math.floor(float12 - 7f * float13);
		float4 x = (float13 * 2f + 0.5f) / 7f - 1f;
		float4 x2 = (float14 * 2f + 0.5f) / 7f - 1f;
		float4 y = 1f - math.abs(x) - math.abs(x2);
		float4 x3 = new float4(x.xy, x2.xy);
		float4 x4 = new float4(x.zw, x2.zw);
		float4 float15 = math.floor(x3) * 2f + 1f;
		float4 float16 = math.floor(x4) * 2f + 1f;
		float4 float17 = -math.step(y, 0f);
		float4 float18 = x3.xzyw + float15.xzyw * float17.xxyy;
		float4 float19 = x4.xzyw + float16.xzyw * float17.zzww;
		float3 float20 = new float3(float18.xy, y.x);
		float3 float21 = new float3(float18.zw, y.y);
		float3 float22 = new float3(float19.xy, y.z);
		float3 float23 = new float3(float19.zw, y.w);
		float4 float24 = taylorInvSqrt(new float4(math.dot(float20, float20), math.dot(float21, float21), math.dot(float22, float22), math.dot(float23, float23)));
		float20 *= float24.x;
		float21 *= float24.y;
		float22 *= float24.z;
		float23 *= float24.w;
		float4 float25 = math.max(0.6f - new float4(math.dot(float3, float3), math.dot(float8, float8), math.dot(float9, float9), math.dot(float10, float10)), 0f);
		float25 *= float25;
		float25 *= float25;
		float4 y2 = new float4(math.dot(float3, float20), math.dot(float8, float21), math.dot(float9, float22), math.dot(float10, float23));
		return 42f * math.dot(float25, y2);
	}

	public static float4 snoise_grad(float3 v)
	{
		float2 @float = new float2(1f / 6f, 1f / 3f);
		float3 float2 = math.floor(v + math.dot(v, @float.yyy));
		float3 float3 = v - float2 + math.dot(float2, @float.xxx);
		float3 float4 = math.step(float3.yzx, float3.xyz);
		float3 float5 = 1f - float4;
		float3 float6 = math.min(float4.xyz, float5.zxy);
		float3 float7 = math.max(float4.xyz, float5.zxy);
		float3 float8 = float3 - float6 + @float.xxx;
		float3 float9 = float3 - float7 + @float.yyy;
		float3 float10 = float3 - 0.5f;
		float2 = mod289(float2);
		float4 float11 = permute(permute(permute(float2.z + new float4(0f, float6.z, float7.z, 1f)) + float2.y + new float4(0f, float6.y, float7.y, 1f)) + float2.x + new float4(0f, float6.x, float7.x, 1f));
		float4 float12 = float11 - 49f * math.floor(float11 / 49f);
		float4 float13 = math.floor(float12 / 7f);
		float4 float14 = math.floor(float12 - 7f * float13);
		float4 x = (float13 * 2f + 0.5f) / 7f - 1f;
		float4 x2 = (float14 * 2f + 0.5f) / 7f - 1f;
		float4 y = 1f - math.abs(x) - math.abs(x2);
		float4 x3 = new float4(x.xy, x2.xy);
		float4 x4 = new float4(x.zw, x2.zw);
		float4 float15 = math.floor(x3) * 2f + 1f;
		float4 float16 = math.floor(x4) * 2f + 1f;
		float4 float17 = -math.step(y, 0f);
		float4 float18 = x3.xzyw + float15.xzyw * float17.xxyy;
		float4 float19 = x4.xzyw + float16.xzyw * float17.zzww;
		float3 float20 = new float3(float18.xy, y.x);
		float3 float21 = new float3(float18.zw, y.y);
		float3 float22 = new float3(float19.xy, y.z);
		float3 float23 = new float3(float19.zw, y.w);
		float4 float24 = taylorInvSqrt(new float4(math.dot(float20, float20), math.dot(float21, float21), math.dot(float22, float22), math.dot(float23, float23)));
		float20 *= float24.x;
		float21 *= float24.y;
		float22 *= float24.z;
		float23 *= float24.w;
		float4 float25 = math.max(0.6f - new float4(math.dot(float3, float3), math.dot(float8, float8), math.dot(float9, float9), math.dot(float10, float10)), 0f);
		float4 float26 = float25 * float25;
		float4 float27 = float26 * float25;
		float4 x5 = float26 * float26;
		float3 xyz = -6f * float27.x * float3 * math.dot(float3, float20) + x5.x * float20 + -6f * float27.y * float8 * math.dot(float8, float21) + x5.y * float21 + -6f * float27.z * float9 * math.dot(float9, float22) + x5.z * float22 + -6f * float27.w * float10 * math.dot(float10, float23) + x5.w * float23;
		return 42f * new float4(xyz, math.dot(y: new float4(math.dot(float3, float20), math.dot(float8, float21), math.dot(float9, float22), math.dot(float10, float23)), x: x5));
	}
}
