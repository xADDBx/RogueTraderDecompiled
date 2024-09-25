using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math.Noise;

public static class BCCNoise4
{
	private static float4 bcc4_mod(float4 x, float4 y)
	{
		return x - y * math.floor(x / y);
	}

	private static float4 bcc4_permute(float4 t)
	{
		return t * (t * 34f + 133f);
	}

	private static float3 bcc4_grad(float hash)
	{
		float3 @float = math.frac(math.floor(hash / new float3(1f, 2f, 4f)) * 0.5f) * 4f - 1f;
		float3 float2 = @float;
		float2 *= (float3)(new int3(0, 1, 2) != (int)(hash / 16f));
		float num = math.frac(math.floor(hash / 8f) * 0.5f) * 2f;
		float3 float3 = (1f - num) * @float + num * (float2 + math.cross(@float, float2));
		return (float2 * 1.2247449f + float3) * ((1f - 0.04294244f * num) * 32.802013f);
	}

	public static float4 Bcc4NoiseBase(float3 X)
	{
		float3 @float = math.round(X);
		float3 float2 = X - @float;
		float3 float3 = math.abs(float2);
		float3 float4 = (float3)(math.max(float3.yzx, float3.zxy) < float3);
		float3 float5 = default(float3);
		float5.x = ((!(float2.x < 0f)) ? 1 : (-1));
		float5.y = ((!(float2.y < 0f)) ? 1 : (-1));
		float5.z = ((!(float2.z < 0f)) ? 1 : (-1));
		float3 float6 = @float + float4 * float5;
		float3 float7 = X - float6;
		float3 float8 = X + 144.5f;
		float3 float9 = math.round(float8);
		float3 float10 = float8 - float9;
		float3 float11 = math.abs(float10);
		float3 float12 = (float3)(math.max(float11.yzx, float11.zxy) < float11);
		float5.x = ((!(float10.x < 0f)) ? 1 : (-1));
		float5.y = ((!(float10.y < 0f)) ? 1 : (-1));
		float5.z = ((!(float10.z < 0f)) ? 1 : (-1));
		float3 float13 = float9 + float12 * float5;
		float3 float14 = float8 - float13;
		float4 float15 = bcc4_mod(bcc4_permute(bcc4_mod(bcc4_permute(bcc4_mod(bcc4_permute(bcc4_mod(new float4(@float.x, float6.x, float9.x, float13.x), 289f)) + new float4(@float.y, float6.y, float9.y, float13.y), 289f)) + new float4(@float.z, float6.z, float9.z, float13.z), 289f)), 48f);
		float4 float16 = math.max(0.5f - new float4(math.dot(float2, float2), math.dot(float7, float7), math.dot(float10, float10), math.dot(float14, float14)), 0f);
		float4 float17 = float16 * float16;
		float4 float18 = float17 * float17;
		float3 y = bcc4_grad(float15.x);
		float3 y2 = bcc4_grad(float15.y);
		float3 y3 = bcc4_grad(float15.z);
		float3 y4 = bcc4_grad(float15.w);
		float4 float19 = new float4(math.dot(float2, y), math.dot(float7, y2), math.dot(float10, y3), math.dot(float14, y4));
		return new float4(-8f * math.mul(float17 * float16 * float19, new float4x3(float2.x, float2.y, float2.z, float7.x, float7.y, float7.z, float10.x, float10.y, float10.z, float14.x, float14.y, float14.z)) + math.mul(float18, new float4x3(y.x, y.y, y.z, y2.x, y2.y, y2.z, y3.x, y3.y, y3.z, y4.x, y4.y, y4.z)), math.dot(float18, float19));
	}

	private static float4 Bcc4NoiseClassic(float3 X)
	{
		float4 @float = Bcc4NoiseBase(math.dot(X, 2f / 3f) - X);
		return new float4(math.dot(@float.xyz, 2f / 3f) - @float.xyz, @float.w);
	}

	private static float4 Bcc4NoisePlaneFirst(float3 X)
	{
		float3x3 float3x = new float3x3(0.7886751f, -0.21132487f, -0.57735026f, -0.21132487f, 0.7886751f, -0.57735026f, 0.57735026f, 0.57735026f, 0.57735026f);
		float4 @float = Bcc4NoiseBase(math.mul(X, float3x));
		return new float4(math.mul(float3x, @float.xyz), @float.w);
	}
}
