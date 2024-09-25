using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math.Noise;

public static class ClassicNoise2D
{
	private static float4 mod(float4 x, float4 y)
	{
		return x - y * math.floor(x / y);
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

	private static float2 fade(float2 t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	public static float cnoise(float2 P)
	{
		float4 x = math.floor(P.xyxy) + new float4(0f, 0f, 1f, 1f);
		float4 @float = math.frac(P.xyxy) - new float4(0f, 0f, 1f, 1f);
		x = mod289(x);
		float4 xzxz = x.xzxz;
		float4 yyww = x.yyww;
		float4 xzxz2 = @float.xzxz;
		float4 yyww2 = @float.yyww;
		float4 float2 = math.frac(permute(permute(xzxz) + yyww) / 41f) * 2f - 1f;
		float4 float3 = math.abs(float2) - 0.5f;
		float4 float4 = math.floor(float2 + 0.5f);
		float2 -= float4;
		float2 float5 = new float2(float2.x, float3.x);
		float2 float6 = new float2(float2.y, float3.y);
		float2 float7 = new float2(float2.z, float3.z);
		float2 float8 = new float2(float2.w, float3.w);
		float4 float9 = taylorInvSqrt(new float4(math.dot(float5, float5), math.dot(float7, float7), math.dot(float6, float6), math.dot(float8, float8)));
		float2 x2 = float5 * float9.x;
		float7 *= float9.y;
		float6 *= float9.z;
		float8 *= float9.w;
		float x3 = math.dot(x2, new float2(xzxz2.x, yyww2.x));
		float x4 = math.dot(float6, new float2(xzxz2.y, yyww2.y));
		float y = math.dot(float7, new float2(xzxz2.z, yyww2.z));
		float y2 = math.dot(float8, new float2(xzxz2.w, yyww2.w));
		float2 float10 = fade(@float.xy);
		float2 float11 = math.lerp(new float2(x3, y), new float2(x4, y2), float10.x);
		float num = math.lerp(float11.x, float11.y, float10.y);
		return 2.3f * num;
	}

	public static float pnoise(float2 P, float2 rep)
	{
		float4 x = math.floor(P.xyxy) + new float4(0f, 0f, 1f, 1f);
		float4 @float = math.frac(P.xyxy) - new float4(0f, 0f, 1f, 1f);
		x = mod(x, rep.xyxy);
		x = mod289(x);
		float4 xzxz = x.xzxz;
		float4 yyww = x.yyww;
		float4 xzxz2 = @float.xzxz;
		float4 yyww2 = @float.yyww;
		float4 float2 = math.frac(permute(permute(xzxz) + yyww) / 41f) * 2f - 1f;
		float4 float3 = math.abs(float2) - 0.5f;
		float4 float4 = math.floor(float2 + 0.5f);
		float2 -= float4;
		float2 float5 = new float2(float2.x, float3.x);
		float2 float6 = new float2(float2.y, float3.y);
		float2 float7 = new float2(float2.z, float3.z);
		float2 float8 = new float2(float2.w, float3.w);
		float4 float9 = taylorInvSqrt(new float4(math.dot(float5, float5), math.dot(float7, float7), math.dot(float6, float6), math.dot(float8, float8)));
		float2 x2 = float5 * float9.x;
		float7 *= float9.y;
		float6 *= float9.z;
		float8 *= float9.w;
		float x3 = math.dot(x2, new float2(xzxz2.x, yyww2.x));
		float x4 = math.dot(float6, new float2(xzxz2.y, yyww2.y));
		float y = math.dot(float7, new float2(xzxz2.z, yyww2.z));
		float y2 = math.dot(float8, new float2(xzxz2.w, yyww2.w));
		float2 float10 = fade(@float.xy);
		float2 float11 = math.lerp(new float2(x3, y), new float2(x4, y2), float10.x);
		float num = math.lerp(float11.x, float11.y, float10.y);
		return 2.3f * num;
	}
}
