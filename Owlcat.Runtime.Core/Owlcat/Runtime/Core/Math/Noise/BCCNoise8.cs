using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math.Noise;

public static class BCCNoise8
{
	private static float4 bcc8_mod(float4 x, float4 y)
	{
		return x - y * math.floor(x / y);
	}

	private static float4 bcc8_permute(float4 t)
	{
		return t * (t * 34f + 133f);
	}

	private static float3 bcc8_grad(float hash)
	{
		float3 @float = math.frac(math.floor(hash / new float3(1f, 2f, 4f)) * 0.5f) * 4f - 1f;
		float3 float2 = @float;
		float2 *= (float3)(new int3(0, 1, 2) != (int)(hash / 16f));
		float num = math.frac(math.floor(hash / 8f) * 0.5f) * 2f;
		float3 float3 = (1f - num) * @float + num * (float2 + math.cross(@float, float2));
		return (float2 * 1.2247449f + float3) * ((1f - 0.04294244f * num) * 3.5946317f);
	}

	public static float4 Bcc8NoiseBase(float3 X)
	{
		float3 @float = math.floor(X);
		float4 x = math.float4(X - @float, 2.5f);
		float3 float2 = @float + math.floor(math.dot(x, 0.25f));
		float3 float3 = @float + new float3(1f, 0f, 0f) + new float3(-1f, 1f, 1f) * math.floor(math.dot(x, new float4(-0.25f, 0.25f, 0.25f, 0.35f)));
		float3 float4 = @float + new float3(0f, 1f, 0f) + new float3(1f, -1f, 1f) * math.floor(math.dot(x, new float4(0.25f, -0.25f, 0.25f, 0.35f)));
		float3 float5 = @float + new float3(0f, 0f, 1f) + new float3(1f, 1f, -1f) * math.floor(math.dot(x, new float4(0.25f, 0.25f, -0.25f, 0.35f)));
		float4 float6 = bcc8_mod(bcc8_permute(bcc8_mod(bcc8_permute(bcc8_mod(bcc8_permute(bcc8_mod(new float4(float2.x, float3.x, float4.x, float5.x), 289f)) + new float4(float2.y, float3.y, float4.y, float5.y), 289f)) + new float4(float2.z, float3.z, float4.z, float5.z), 289f)), 48f);
		float3 float7 = X - float2;
		float3 float8 = X - float3;
		float3 float9 = X - float4;
		float3 float10 = X - float5;
		float4 float11 = math.max(0.75f - new float4(math.dot(float7, float7), math.dot(float8, float8), math.dot(float9, float9), math.dot(float10, float10)), 0f);
		float4 float12 = float11 * float11;
		float4 float13 = float12 * float12;
		float3 y = bcc8_grad(float6.x);
		float3 y2 = bcc8_grad(float6.y);
		float3 y3 = bcc8_grad(float6.z);
		float3 y4 = bcc8_grad(float6.w);
		float4 float14 = new float4(math.dot(float7, y), math.dot(float8, y2), math.dot(float9, y3), math.dot(float10, y4));
		return new float4(-8f * math.mul(float12 * float11 * float14, new float4x3(float7.x, float7.y, float7.z, float8.x, float8.y, float8.z, float9.x, float9.y, float9.z, float10.x, float10.y, float10.z)) + math.mul(float13, new float4x3(y.x, y.y, y.z, y2.x, y2.y, y2.z, y3.x, y3.y, y3.z, y4.x, y4.y, y4.z)), math.dot(float13, float14));
	}

	private static float4 Bcc8NoiseClassic(float3 X)
	{
		X = math.dot(X, 2f / 3f) - X;
		float4 @float = Bcc8NoiseBase(X) + Bcc8NoiseBase(X + 144.5f);
		return new float4(math.dot(@float.xyz, 2f / 3f) - @float.xyz, @float.w);
	}

	private static float4 Bcc8NoisePlaneFirst(float3 X)
	{
		float3x3 float3x = new float3x3(0.7886751f, -0.21132487f, -0.57735026f, -0.21132487f, 0.7886751f, -0.57735026f, 0.57735026f, 0.57735026f, 0.57735026f);
		X = math.mul(X, float3x);
		float4 @float = Bcc8NoiseBase(X) + Bcc8NoiseBase(X + 144.5f);
		return new float4(math.mul(float3x, @float.xyz), @float.w);
	}
}
