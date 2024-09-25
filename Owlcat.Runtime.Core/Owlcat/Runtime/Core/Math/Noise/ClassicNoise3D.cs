using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math.Noise;

public static class ClassicNoise3D
{
	private static float3 mod(float3 x, float3 y)
	{
		return x - y * math.floor(x / y);
	}

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
		return (float4)1.7928429f - r * 0.85373473f;
	}

	private static float3 fade(float3 t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	public static float cnoise(float3 P)
	{
		float3 @float = math.floor(P);
		float3 x = @float + (float3)1.0;
		@float = mod289(@float);
		x = mod289(x);
		float3 float2 = math.frac(P);
		float3 y = float2 - (float3)1.0;
		float4 x2 = new float4(@float.x, x.x, @float.x, x.x);
		float4 float3 = new float4(@float.y, @float.y, x.y, x.y);
		float4 float4 = @float.z;
		float4 float5 = x.z;
		float4 float6 = permute(permute(x2) + float3);
		float4 float7 = permute(float6 + float4);
		float4 float8 = permute(float6 + float5);
		float4 x3 = float7 / 7f;
		float4 x4 = math.frac(math.floor(x3) / 7f) - 0.5f;
		x3 = math.frac(x3);
		float4 y2 = (float4)0.5 - math.abs(x3) - math.abs(x4);
		float4 float9 = math.step(y2, 0f);
		x3 -= float9 * (math.step(0f, x3) - 0.5f);
		x4 -= float9 * (math.step(0f, x4) - 0.5f);
		float4 x5 = float8 / 7f;
		float4 x6 = math.frac(math.floor(x5) / 7f) - 0.5f;
		x5 = math.frac(x5);
		float4 y3 = (float4)0.5f - math.abs(x5) - math.abs(x6);
		float4 float10 = math.step(y3, (float4)0.0);
		x5 -= float10 * (math.step(0f, x5) - 0.5f);
		x6 -= float10 * (math.step(0f, x6) - 0.5f);
		float3 float11 = new float3(x3.x, x4.x, y2.x);
		float3 float12 = new float3(x3.y, x4.y, y2.y);
		float3 float13 = new float3(x3.z, x4.z, y2.z);
		float3 float14 = new float3(x3.w, x4.w, y2.w);
		float3 float15 = new float3(x5.x, x6.x, y3.x);
		float3 float16 = new float3(x5.y, x6.y, y3.y);
		float3 float17 = new float3(x5.z, x6.z, y3.z);
		float3 float18 = new float3(x5.w, x6.w, y3.w);
		float4 float19 = taylorInvSqrt(new float4(math.dot(float11, float11), math.dot(float13, float13), math.dot(float12, float12), math.dot(float14, float14)));
		float11 *= float19.x;
		float13 *= float19.y;
		float12 *= float19.z;
		float14 *= float19.w;
		float4 float20 = taylorInvSqrt(new float4(math.dot(float15, float15), math.dot(float17, float17), math.dot(float16, float16), math.dot(float18, float18)));
		float15 *= float20.x;
		float17 *= float20.y;
		float16 *= float20.z;
		float18 *= float20.w;
		float x7 = math.dot(float11, float2);
		float y4 = math.dot(float12, new float3(y.x, float2.y, float2.z));
		float z = math.dot(float13, new float3(float2.x, y.y, float2.z));
		float w = math.dot(float14, new float3(y.x, y.y, float2.z));
		float x8 = math.dot(float15, new float3(float2.x, float2.y, y.z));
		float y5 = math.dot(float16, new float3(y.x, float2.y, y.z));
		float z2 = math.dot(float17, new float3(float2.x, y.y, y.z));
		float w2 = math.dot(float18, y);
		float3 float21 = fade(float2);
		float4 float22 = math.lerp(new float4(x7, y4, z, w), new float4(x8, y5, z2, w2), float21.z);
		float2 float23 = math.lerp(float22.xy, float22.zw, float21.y);
		float num = math.lerp(float23.x, float23.y, float21.x);
		return 2.2f * num;
	}

	public static float pnoise(float3 P, float3 rep)
	{
		float3 @float = mod(math.floor(P), rep);
		float3 x = mod(@float + (float3)1.0, rep);
		@float = mod289(@float);
		x = mod289(x);
		float3 float2 = math.frac(P);
		float3 y = float2 - (float3)1.0;
		float4 x2 = new float4(@float.x, x.x, @float.x, x.x);
		float4 float3 = new float4(@float.y, @float.y, x.y, x.y);
		float4 float4 = @float.z;
		float4 float5 = x.z;
		float4 float6 = permute(permute(x2) + float3);
		float4 float7 = permute(float6 + float4);
		float4 float8 = permute(float6 + float5);
		float4 x3 = float7 / 7f;
		float4 x4 = math.frac(math.floor(x3) / 7f) - 0.5f;
		x3 = math.frac(x3);
		float4 y2 = (float4)0.5 - math.abs(x3) - math.abs(x4);
		float4 float9 = math.step(y2, 0f);
		x3 -= float9 * (math.step(0f, x3) - 0.5f);
		x4 -= float9 * (math.step(0f, x4) - 0.5f);
		float4 x5 = float8 / 7f;
		float4 x6 = math.frac(math.floor(x5) / 7f) - 0.5f;
		x5 = math.frac(x5);
		float4 y3 = (float4)0.5f - math.abs(x5) - math.abs(x6);
		float4 float10 = math.step(y3, 0f);
		x5 -= float10 * (math.step(0f, x5) - 0.5f);
		x6 -= float10 * (math.step(0f, x6) - 0.5f);
		float3 float11 = new float3(x3.x, x4.x, y2.x);
		float3 float12 = new float3(x3.y, x4.y, y2.y);
		float3 float13 = new float3(x3.z, x4.z, y2.z);
		float3 float14 = new float3(x3.w, x4.w, y2.w);
		float3 float15 = new float3(x5.x, x6.x, y3.x);
		float3 float16 = new float3(x5.y, x6.y, y3.y);
		float3 float17 = new float3(x5.z, x6.z, y3.z);
		float3 float18 = new float3(x5.w, x6.w, y3.w);
		float4 float19 = taylorInvSqrt(new float4(math.dot(float11, float11), math.dot(float13, float13), math.dot(float12, float12), math.dot(float14, float14)));
		float11 *= float19.x;
		float13 *= float19.y;
		float12 *= float19.z;
		float14 *= float19.w;
		float4 float20 = taylorInvSqrt(new float4(math.dot(float15, float15), math.dot(float17, float17), math.dot(float16, float16), math.dot(float18, float18)));
		float15 *= float20.x;
		float17 *= float20.y;
		float16 *= float20.z;
		float18 *= float20.w;
		float x7 = math.dot(float11, float2);
		float y4 = math.dot(float12, new float3(y.x, float2.y, float2.z));
		float z = math.dot(float13, new float3(float2.x, y.y, float2.z));
		float w = math.dot(float14, new float3(y.x, y.y, float2.z));
		float x8 = math.dot(float15, new float3(float2.x, float2.y, y.z));
		float y5 = math.dot(float16, new float3(y.x, float2.y, y.z));
		float z2 = math.dot(float17, new float3(float2.x, y.y, y.z));
		float w2 = math.dot(float18, y);
		float3 float21 = fade(float2);
		float4 float22 = math.lerp(new float4(x7, y4, z, w), new float4(x8, y5, z2, w2), float21.z);
		float2 float23 = math.lerp(float22.xy, float22.zw, float21.y);
		float num = math.lerp(float23.x, float23.y, float21.x);
		return 2.2f * num;
	}
}
