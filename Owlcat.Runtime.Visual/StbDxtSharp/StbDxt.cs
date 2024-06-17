using System;

namespace StbDxtSharp;

public static class StbDxt
{
	public static readonly int nIterPower = 4;

	public static readonly float[] midpoints5 = new float[32]
	{
		0.015686f, 0.047059f, 0.078431f, 0.111765f, 0.145098f, 0.176471f, 0.207843f, 0.241176f, 0.27451f, 0.305882f,
		0.337255f, 0.370588f, 0.403922f, 0.435294f, 0.466667f, 0.5f, 0.533333f, 0.564706f, 0.596078f, 0.629412f,
		0.662745f, 0.694118f, 0.72549f, 0.758824f, 0.792157f, 0.823529f, 0.854902f, 0.888235f, 0.921569f, 0.952941f,
		0.984314f, 1f
	};

	public static readonly float[] midpoints6 = new float[64]
	{
		0.007843f,
		0.023529f,
		0.039216f,
		0.054902f,
		0.070588f,
		0.086275f,
		0.101961f,
		0.117647f,
		0.133333f,
		0.14902f,
		0.164706f,
		0.180392f,
		0.196078f,
		0.211765f,
		0.227451f,
		0.245098f,
		0.262745f,
		0.278431f,
		0.294118f,
		0.309804f,
		0.32549f,
		0.341176f,
		0.356863f,
		0.372549f,
		0.388235f,
		0.403922f,
		0.419608f,
		0.435294f,
		0.45098f,
		0.466667f,
		0.482353f,
		0.5f,
		0.517647f,
		0.533333f,
		0.54902f,
		0.564706f,
		0.580392f,
		0.596078f,
		0.611765f,
		32f / 51f,
		0.643137f,
		0.658824f,
		0.67451f,
		0.690196f,
		0.705882f,
		0.721569f,
		0.737255f,
		0.754902f,
		0.772549f,
		0.788235f,
		0.803922f,
		0.819608f,
		0.835294f,
		0.85098f,
		0.866667f,
		0.882353f,
		0.898039f,
		0.913725f,
		0.929412f,
		MathF.E * 105f / 302f,
		0.960784f,
		0.976471f,
		0.992157f,
		1f
	};

	public static readonly int[] w1Tab = new int[4] { 3, 0, 2, 1 };

	public static readonly int[] prods = new int[4] { 589824, 2304, 262402, 66562 };

	public unsafe static void stb__DitherBlock(DxtContext ctx, byte* dest, byte* block)
	{
		int* ptr = stackalloc int[8];
		int* ptr2 = ptr;
		int* ptr3 = ptr + 4;
		for (int i = 0; i < 3; i++)
		{
			byte* ptr4 = block + i;
			byte* ptr5 = dest + i;
			byte* ptr6 = ((i == 1) ? ctx.stb__QuantGTab : ctx.stb__QuantRBTab);
			CRuntime.memset(ptr, 0, 32uL);
			for (int j = 0; j < 4; j++)
			{
				*ptr5 = ptr6[*ptr4 + (3 * ptr3[1] + 5 * *ptr3 >> 4)];
				*ptr2 = *ptr4 - *ptr5;
				ptr5[4] = ptr6[ptr4[4] + (7 * *ptr2 + 3 * ptr3[2] + 5 * ptr3[1] + *ptr3 >> 4)];
				ptr2[1] = ptr4[4] - ptr5[4];
				ptr5[8] = ptr6[ptr4[8] + (7 * ptr2[1] + 3 * ptr3[3] + 5 * ptr3[2] + ptr3[1] >> 4)];
				ptr2[2] = ptr4[8] - ptr5[8];
				ptr5[12] = ptr6[ptr4[12] + (7 * ptr2[2] + 5 * ptr3[3] + ptr3[2] >> 4)];
				ptr2[3] = ptr4[12] - ptr5[12];
				ptr4 += 16;
				ptr5 += 16;
				int* intPtr = ptr2;
				ptr2 = ptr3;
				ptr3 = intPtr;
			}
		}
	}

	public unsafe static Error.Value CompressDxt(DxtContext ctx, int width, int height, byte* data, byte* dest, bool hasAlpha, CompressionMode mode)
	{
		if (width % 4 != 0 || height % 4 != 0)
		{
			return Error.Value.ERROR_DIMENSIONS;
		}
		byte* ptr = stackalloc byte[64];
		byte* ptr2 = dest;
		for (int i = 0; i < height; i += 4)
		{
			for (int j = 0; j < width; j += 4)
			{
				for (int k = 0; k < 4 && i + k < height; k++)
				{
					CRuntime.memcpy(ptr + k * 16, data + width * 4 * (i + k) + j * 4, 16L);
				}
				stb_compress_dxt_block(ctx, ptr2, ptr, hasAlpha ? 1 : 0, (int)mode);
				ptr2 += (hasAlpha ? 16 : 8);
			}
		}
		return Error.Value.NO_ERROR;
	}

	public unsafe static Error.Value CompressDxt1(DxtContext ctx, int width, int height, byte* data, byte* dest, CompressionMode mode = CompressionMode.None)
	{
		return CompressDxt(ctx, width, height, data, dest, hasAlpha: false, mode);
	}

	public unsafe static Error.Value CompressDxt5(DxtContext ctx, int width, int height, byte* data, byte* dest, CompressionMode mode = CompressionMode.None)
	{
		return CompressDxt(ctx, width, height, data, dest, hasAlpha: true, mode);
	}

	public static int stb__Mul8Bit(int a, int b)
	{
		int num = a * b + 128;
		return num + (num >> 8) >> 8;
	}

	public unsafe static void stb__From16Bit(DxtContext ctx, byte* _out_, ushort v)
	{
		int num = (v & 0xF800) >> 11;
		int num2 = (v & 0x7E0) >> 5;
		int num3 = v & 0x1F;
		*_out_ = ctx.stb__Expand5[num];
		_out_[1] = ctx.stb__Expand6[num2];
		_out_[2] = ctx.stb__Expand5[num3];
		_out_[3] = 0;
	}

	public static ushort stb__As16Bit(int r, int g, int b)
	{
		return (ushort)((stb__Mul8Bit(r, 31) << 11) + (stb__Mul8Bit(g, 63) << 5) + stb__Mul8Bit(b, 31));
	}

	public static int stb__Lerp13(int a, int b)
	{
		return (2 * a + b) / 3;
	}

	public unsafe static void stb__Lerp13RGB(byte* _out_, byte* p1, byte* p2)
	{
		*_out_ = (byte)stb__Lerp13(*p1, *p2);
		_out_[1] = (byte)stb__Lerp13(p1[1], p2[1]);
		_out_[2] = (byte)stb__Lerp13(p1[2], p2[2]);
	}

	public unsafe static void stb__PrepareOptTable(byte* Table, byte* expand, int size)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (num = 0; num < 256; num++)
		{
			int num4 = 256;
			for (num2 = 0; num2 < size; num2++)
			{
				for (num3 = 0; num3 < size; num3++)
				{
					int num5 = expand[num2];
					int num6 = expand[num3];
					int num7 = CRuntime.abs(stb__Lerp13(num6, num5) - num);
					num7 += CRuntime.abs(num6 - num5) * 3 / 100;
					if (num7 < num4)
					{
						Table[num * 2] = (byte)num3;
						Table[num * 2 + 1] = (byte)num2;
						num4 = num7;
					}
				}
			}
		}
	}

	public unsafe static void stb__EvalColors(DxtContext ctx, byte* color, ushort c0, ushort c1)
	{
		stb__From16Bit(ctx, color, c0);
		stb__From16Bit(ctx, color + 4, c1);
		stb__Lerp13RGB(color + 8, color, color + 4);
		stb__Lerp13RGB(color + 12, color + 4, color);
	}

	public unsafe static uint stb__MatchColorsBlock(byte* block, byte* color, int dither)
	{
		uint num = 0u;
		int num2 = *color - color[4];
		int num3 = color[1] - color[5];
		int num4 = color[2] - color[6];
		int* ptr = stackalloc int[16];
		int* ptr2 = stackalloc int[4];
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		for (num5 = 0; num5 < 16; num5++)
		{
			ptr[num5] = block[num5 * 4] * num2 + block[num5 * 4 + 1] * num3 + block[num5 * 4 + 2] * num4;
		}
		for (num5 = 0; num5 < 4; num5++)
		{
			ptr2[num5] = color[num5 * 4] * num2 + color[num5 * 4 + 1] * num3 + color[num5 * 4 + 2] * num4;
		}
		num6 = ptr2[1] + ptr2[3];
		num7 = ptr2[3] + ptr2[2];
		num8 = ptr2[2] + *ptr2;
		if (dither == 0)
		{
			for (num5 = 15; num5 >= 0; num5--)
			{
				int num9 = ptr[num5] * 2;
				num <<= 2;
				num = ((num9 >= num7) ? (num | ((num9 < num8) ? 2u : 0u)) : (num | ((num9 < num6) ? 1u : 3u)));
			}
		}
		else
		{
			int* ptr3 = stackalloc int[8];
			int* ptr4 = ptr3;
			int* ptr5 = ptr3 + 4;
			int* ptr6 = ptr;
			int num10 = 0;
			num6 <<= 3;
			num7 <<= 3;
			num8 <<= 3;
			for (num5 = 0; num5 < 8; num5++)
			{
				ptr3[num5] = 0;
			}
			for (num10 = 0; num10 < 4; num10++)
			{
				int num11 = 0;
				int num12 = 0;
				int num13 = 0;
				num11 = (*ptr6 << 4) + 3 * ptr5[1] + 5 * *ptr5;
				num13 = ((num11 >= num7) ? ((num11 < num8) ? 2 : 0) : ((num11 < num6) ? 1 : 3));
				*ptr4 = *ptr6 - ptr2[num13];
				num12 = num13;
				num11 = (ptr6[1] << 4) + 7 * *ptr4 + 3 * ptr5[2] + 5 * ptr5[1] + *ptr5;
				num13 = ((num11 >= num7) ? ((num11 < num8) ? 2 : 0) : ((num11 < num6) ? 1 : 3));
				ptr4[1] = ptr6[1] - ptr2[num13];
				num12 |= num13 << 2;
				num11 = (ptr6[2] << 4) + 7 * ptr4[1] + 3 * ptr5[3] + 5 * ptr5[2] + ptr5[1];
				num13 = ((num11 >= num7) ? ((num11 < num8) ? 2 : 0) : ((num11 < num6) ? 1 : 3));
				ptr4[2] = ptr6[2] - ptr2[num13];
				num12 |= num13 << 4;
				num11 = (ptr6[3] << 4) + 7 * ptr4[2] + 5 * ptr5[3] + ptr5[2];
				num13 = ((num11 >= num7) ? ((num11 < num8) ? 2 : 0) : ((num11 < num6) ? 1 : 3));
				ptr4[3] = ptr6[3] - ptr2[num13];
				num12 |= num13 << 6;
				ptr6 += 4;
				num |= (uint)(num12 << num10 * 8);
				int* intPtr = ptr4;
				ptr4 = ptr5;
				ptr5 = intPtr;
			}
		}
		return num;
	}

	public unsafe static void stb__OptimizeColorsBlock(byte* block, ushort* pmax16, ushort* pmin16)
	{
		int num = int.MaxValue;
		int num2 = -2147483647;
		byte* ptr = null;
		byte* ptr2 = null;
		double num3 = 0.0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		float* ptr3 = stackalloc float[6];
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		int* ptr4 = stackalloc int[6];
		int* ptr5 = stackalloc int[3];
		int* ptr6 = stackalloc int[3];
		int* ptr7 = stackalloc int[3];
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		for (num10 = 0; num10 < 3; num10++)
		{
			byte* ptr8 = block + num10;
			int num13 = 0;
			int num14 = 0;
			int num15 = 0;
			num13 = (num14 = (num15 = *ptr8));
			for (num11 = 4; num11 < 64; num11 += 4)
			{
				num13 += ptr8[num11];
				if (ptr8[num11] < num14)
				{
					num14 = ptr8[num11];
				}
				else if (ptr8[num11] > num15)
				{
					num15 = ptr8[num11];
				}
			}
			ptr5[num10] = num13 + 8 >> 4;
			ptr6[num10] = num14;
			ptr7[num10] = num15;
		}
		for (num11 = 0; num11 < 6; num11++)
		{
			ptr4[num11] = 0;
		}
		for (num11 = 0; num11 < 16; num11++)
		{
			int num16 = block[num11 * 4] - *ptr5;
			int num17 = block[num11 * 4 + 1] - ptr5[1];
			int num18 = block[num11 * 4 + 2] - ptr5[2];
			*ptr4 += num16 * num16;
			ptr4[1] += num16 * num17;
			ptr4[2] += num16 * num18;
			ptr4[3] += num17 * num17;
			ptr4[4] += num17 * num18;
			ptr4[5] += num18 * num18;
		}
		for (num11 = 0; num11 < 6; num11++)
		{
			ptr3[num11] = (float)ptr4[num11] / 255f;
		}
		num7 = *ptr7 - *ptr6;
		num8 = ptr7[1] - ptr6[1];
		num9 = ptr7[2] - ptr6[2];
		for (num12 = 0; num12 < nIterPower; num12++)
		{
			float num19 = num7 * *ptr3 + num8 * ptr3[1] + num9 * ptr3[2];
			float num20 = num7 * ptr3[1] + num8 * ptr3[3] + num9 * ptr3[4];
			float num21 = num7 * ptr3[2] + num8 * ptr3[4] + num9 * ptr3[5];
			num7 = num19;
			num8 = num20;
			num9 = num21;
		}
		num3 = CRuntime.fabs(num7);
		if ((double)CRuntime.fabs(num8) > num3)
		{
			num3 = CRuntime.fabs(num8);
		}
		if ((double)CRuntime.fabs(num9) > num3)
		{
			num3 = CRuntime.fabs(num9);
		}
		if (num3 < 4.0)
		{
			num4 = 299;
			num5 = 587;
			num6 = 114;
		}
		else
		{
			num3 = 512.0 / num3;
			num4 = (int)((double)num7 * num3);
			num5 = (int)((double)num8 * num3);
			num6 = (int)((double)num9 * num3);
		}
		for (num11 = 0; num11 < 16; num11++)
		{
			int num22 = block[num11 * 4] * num4 + block[num11 * 4 + 1] * num5 + block[num11 * 4 + 2] * num6;
			if (num22 < num)
			{
				num = num22;
				ptr = block + num11 * 4;
			}
			if (num22 > num2)
			{
				num2 = num22;
				ptr2 = block + num11 * 4;
			}
		}
		*pmax16 = stb__As16Bit(*ptr2, ptr2[1], ptr2[2]);
		*pmin16 = stb__As16Bit(*ptr, ptr[1], ptr[2]);
	}

	public static ushort stb__Quantize5(float x)
	{
		ushort num = 0;
		x = ((x < 0f) ? 0f : ((x > 1f) ? 1f : x));
		num = (ushort)(x * 31f);
		return (ushort)(num + (ushort)((x > midpoints5[num]) ? 1 : 0));
	}

	public static ushort stb__Quantize6(float x)
	{
		ushort num = 0;
		x = ((x < 0f) ? 0f : ((x > 1f) ? 1f : x));
		num = (ushort)(x * 63f);
		return (ushort)(num + (ushort)((x > midpoints6[num]) ? 1 : 0));
	}

	public unsafe static int stb__RefineBlock(DxtContext ctx, byte* block, ushort* pmax16, ushort* pmin16, uint mask)
	{
		float num = 0f;
		ushort num2 = 0;
		ushort num3 = 0;
		ushort num4 = 0;
		ushort num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		int num16 = 0;
		uint num17 = mask;
		num2 = *pmin16;
		num3 = *pmax16;
		if ((mask ^ (mask << 2)) < 4)
		{
			int num18 = 8;
			int num19 = 8;
			int num20 = 8;
			for (num6 = 0; num6 < 16; num6++)
			{
				num18 += block[num6 * 4];
				num19 += block[num6 * 4 + 1];
				num20 += block[num6 * 4 + 2];
			}
			num18 >>= 4;
			num19 >>= 4;
			num20 >>= 4;
			num5 = (ushort)((ctx.stb__OMatch5[num18 * 2] << 11) | (ctx.stb__OMatch6[num19 * 2] << 5) | ctx.stb__OMatch5[num20 * 2]);
			num4 = (ushort)((ctx.stb__OMatch5[num18 * 2 + 1] << 11) | (ctx.stb__OMatch6[num19 * 2 + 1] << 5) | ctx.stb__OMatch5[num20 * 2 + 1]);
		}
		else
		{
			num11 = (num12 = (num13 = 0));
			num14 = (num15 = (num16 = 0));
			num6 = 0;
			while (num6 < 16)
			{
				int num21 = (int)(num17 & 3);
				int num22 = w1Tab[num21];
				int num23 = block[num6 * 4];
				int num24 = block[num6 * 4 + 1];
				int num25 = block[num6 * 4 + 2];
				num7 += prods[num21];
				num11 += num22 * num23;
				num12 += num22 * num24;
				num13 += num22 * num25;
				num14 += num23;
				num15 += num24;
				num16 += num25;
				num6++;
				num17 >>= 2;
			}
			num14 = 3 * num14 - num11;
			num15 = 3 * num15 - num12;
			num16 = 3 * num16 - num13;
			num8 = num7 >> 16;
			num10 = (num7 >> 8) & 0xFF;
			num9 = num7 & 0xFF;
			num = 1f / 85f / (float)(num8 * num10 - num9 * num9);
			num5 = (ushort)(stb__Quantize5((float)(num11 * num10 - num14 * num9) * num) << 11);
			num5 |= (ushort)(stb__Quantize6((float)(num12 * num10 - num15 * num9) * num) << 5);
			num5 |= stb__Quantize5((float)(num13 * num10 - num16 * num9) * num);
			num4 = (ushort)(stb__Quantize5((float)(num14 * num8 - num11 * num9) * num) << 11);
			num4 |= (ushort)(stb__Quantize6((float)(num15 * num8 - num12 * num9) * num) << 5);
			num4 |= stb__Quantize5((float)(num16 * num8 - num13 * num9) * num);
		}
		*pmin16 = num4;
		*pmax16 = num5;
		if (num2 == num4 && num3 == num5)
		{
			return 0;
		}
		return 1;
	}

	public unsafe static void stb__CompressColorBlock(DxtContext ctx, byte* dest, byte* block, int mode)
	{
		uint num = 0u;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		ushort num5 = 0;
		ushort num6 = 0;
		byte* ptr = stackalloc byte[64];
		byte* color = stackalloc byte[16];
		num3 = mode & 1;
		num4 = (((mode & 2) == 0) ? 1 : 2);
		for (num2 = 1; num2 < 16 && *(uint*)(block + (nint)num2 * (nint)4) == *(uint*)block; num2++)
		{
		}
		if (num2 == 16)
		{
			int num7 = *block;
			int num8 = block[1];
			int num9 = block[2];
			num = 2863311530u;
			num5 = (ushort)((ctx.stb__OMatch5[num7 * 2] << 11) | (ctx.stb__OMatch6[num8 * 2] << 5) | ctx.stb__OMatch5[num9 * 2]);
			num6 = (ushort)((ctx.stb__OMatch5[num7 * 2 + 1] << 11) | (ctx.stb__OMatch6[num8 * 2 + 1] << 5) | ctx.stb__OMatch5[num9 * 2 + 1]);
		}
		else
		{
			if (num3 != 0)
			{
				stb__DitherBlock(ctx, ptr, block);
			}
			stb__OptimizeColorsBlock((num3 != 0) ? ptr : block, &num5, &num6);
			if (num5 != num6)
			{
				stb__EvalColors(ctx, color, num5, num6);
				num = stb__MatchColorsBlock(block, color, num3);
			}
			else
			{
				num = 0u;
			}
			for (num2 = 0; num2 < num4; num2++)
			{
				uint num10 = num;
				if (stb__RefineBlock(ctx, (num3 != 0) ? ptr : block, &num5, &num6, num) != 0)
				{
					if (num5 == num6)
					{
						num = 0u;
						break;
					}
					stb__EvalColors(ctx, color, num5, num6);
					num = stb__MatchColorsBlock(block, color, num3);
				}
				if (num == num10)
				{
					break;
				}
			}
		}
		if (num5 < num6)
		{
			ushort num11 = num6;
			num6 = num5;
			num5 = num11;
			num ^= 0x55555555u;
		}
		*dest = (byte)num5;
		dest[1] = (byte)(num5 >> 8);
		dest[2] = (byte)num6;
		dest[3] = (byte)(num6 >> 8);
		dest[4] = (byte)num;
		dest[5] = (byte)(num >> 8);
		dest[6] = (byte)(num >> 16);
		dest[7] = (byte)(num >> 24);
	}

	public unsafe static void stb__CompressAlphaBlock(byte* dest, byte* src, int stride)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		num8 = (num9 = *src);
		for (num = 1; num < 16; num++)
		{
			if (src[num * stride] < num8)
			{
				num8 = src[num * stride];
			}
			else if (src[num * stride] > num9)
			{
				num9 = src[num * stride];
			}
		}
		*dest = (byte)num9;
		dest[1] = (byte)num8;
		dest += 2;
		num2 = num9 - num8;
		num4 = num2 * 4;
		num5 = num2 * 2;
		num3 = ((num2 < 8) ? (num2 - 1) : (num2 / 2 + 2));
		num3 -= num8 * 7;
		num6 = 0;
		num7 = 0;
		for (num = 0; num < 16; num++)
		{
			int num10 = src[num * stride] * 7 + num3;
			int num11 = 0;
			int num12 = 0;
			num12 = ((num10 >= num4) ? (-1) : 0);
			num11 = num12 & 4;
			num10 -= num4 & num12;
			num12 = ((num10 >= num5) ? (-1) : 0);
			num11 += num12 & 2;
			num10 -= num5 & num12;
			num11 += ((num10 >= num2) ? 1 : 0);
			num11 = -num11 & 7;
			num11 ^= ((2 > num11) ? 1 : 0);
			num7 |= num11 << num6;
			if ((num6 += 3) >= 8)
			{
				*(dest++) = (byte)num7;
				num7 >>= 8;
				num6 -= 8;
			}
		}
	}

	public unsafe static void stb_compress_dxt_block(DxtContext ctx, byte* dest, byte* src, int alpha, int mode)
	{
		byte* ptr = stackalloc byte[64];
		if (alpha != 0)
		{
			int num = 0;
			stb__CompressAlphaBlock(dest, src + 3, 4);
			dest += 8;
			CRuntime.memcpy(ptr, src, 64uL);
			for (num = 0; num < 16; num++)
			{
				ptr[num * 4 + 3] = byte.MaxValue;
			}
			src = ptr;
		}
		stb__CompressColorBlock(ctx, dest, src, mode);
	}

	public unsafe static void stb_compress_bc4_block(byte* dest, byte* src)
	{
		stb__CompressAlphaBlock(dest, src, 1);
	}

	public unsafe static void stb_compress_bc5_block(byte* dest, byte* src)
	{
		stb__CompressAlphaBlock(dest, src, 2);
		stb__CompressAlphaBlock(dest + 8, src + 1, 2);
	}
}
