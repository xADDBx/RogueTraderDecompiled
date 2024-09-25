using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math.Noise;

public static class SimplexNoise2D
{
	private static float3 mod289(float3 x)
	{
		return x - math.floor(x / 289f) * 289f;
	}

	private static float2 mod289(float2 x)
	{
		return x - math.floor(x / 289f) * 289f;
	}

	private static float3 permute(float3 x)
	{
		return mod289((x * 34f + 1f) * x);
	}

	private static float3 taylorInvSqrt(float3 r)
	{
		return 1.7928429f - 0.85373473f * r;
	}

	public static float snoise(float2 v)
	{
		float4 @float = new float4(0.21132487f, 0.36602542f, -0.57735026f, 1f / 41f);
		float2 float2 = math.floor(v + math.dot(v, @float.yy));
		float2 float3 = v - float2 + math.dot(float2, @float.xx);
		float2 float4 = default(float2);
		float4.x = math.step(float3.y, float3.x);
		float4.y = 1f - float4.x;
		float2 float5 = float3 + @float.xx - float4;
		float2 float6 = float3 + @float.zz;
		float2 = mod289(float2);
		float3 float7 = permute(permute(float2.y + math.float3(0f, float4.y, 1f)) + float2.x + math.float3(0f, float4.x, 1f));
		float3 float8 = math.max(0.5f - math.float3(math.dot(float3, float3), math.dot(float5, float5), math.dot(float6, float6)), 0f);
		float8 *= float8;
		float8 *= float8;
		float3 float9 = 2f * math.frac(float7 * @float.www) - 1f;
		float3 float10 = math.abs(float9) - 0.5f;
		float3 float11 = math.floor(float9 + 0.5f);
		float3 float12 = float9 - float11;
		float8 *= taylorInvSqrt(float12 * float12 + float10 * float10);
		float3 y = default(float3);
		y.x = float12.x * float3.x + float10.x * float3.y;
		y.y = float12.y * float5.x + float10.y * float5.y;
		y.z = float12.z * float6.x + float10.z * float6.y;
		return 130f * math.dot(float8, y);
	}

	public static float3 snoise_grad(float2 v)
	{
		float4 @float = new float4(0.21132487f, 0.36602542f, -0.57735026f, 1f / 41f);
		float2 float2 = math.floor(v + math.dot(v, @float.yy));
		float2 float3 = v - float2 + math.dot(float2, @float.xx);
		float2 float4 = default(float2);
		float4.x = math.step(float3.y, float3.x);
		float4.y = 1f - float4.x;
		float2 float5 = float3 + @float.xx - float4;
		float2 float6 = float3 + @float.zz;
		float2 = mod289(float2);
		float3 float7 = permute(permute(float2.y + new float3(0f, float4.y, 1f)) + float2.x + new float3(0f, float4.x, 1f));
		float3 float8 = math.max(0.5f - new float3(math.dot(float3, float3), math.dot(float5, float5), math.dot(float6, float6)), 0f);
		float3 float9 = float8 * float8;
		float3 float10 = float9 * float8;
		float3 x = float9 * float9;
		float3 float11 = 2f * math.frac(float7 * @float.www) - 1f;
		float3 float12 = math.abs(float11) - 0.5f;
		float3 float13 = math.floor(float11 + 0.5f);
		float3 float14 = float11 - float13;
		float3 obj = float14;
		float3 float15 = taylorInvSqrt(obj * obj + float12 * float12);
		float2 float16 = new float2(float14.x, float12.x) * float15.x;
		float2 float17 = new float2(float14.y, float12.y) * float15.y;
		float2 float18 = new float2(float14.z, float12.z) * float15.z;
		float2 xy = -6f * float10.x * float3 * math.dot(float3, float16) + x.x * float16 + -6f * float10.y * float5 * math.dot(float5, float17) + x.y * float17 + -6f * float10.z * float6 * math.dot(float6, float18) + x.z * float18;
		return 130f * new float3(xy, math.dot(y: new float3(math.dot(float3, float16), math.dot(float5, float17), math.dot(float6, float18)), x: x));
	}
}
