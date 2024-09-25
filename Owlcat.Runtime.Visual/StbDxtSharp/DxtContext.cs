using Unity.Burst;

namespace StbDxtSharp;

[BurstCompile]
public struct DxtContext
{
	public unsafe fixed byte stb__Expand5[32];

	public unsafe fixed byte stb__Expand6[64];

	public unsafe fixed byte stb__OMatch5[512];

	public unsafe fixed byte stb__OMatch6[512];

	public unsafe fixed byte stb__QuantRBTab[272];

	public unsafe fixed byte stb__QuantGTab[272];

	public unsafe void Init()
	{
		int num = 0;
		for (num = 0; num < 32; num++)
		{
			stb__Expand5[num] = (byte)((num << 3) | (num >> 2));
		}
		for (num = 0; num < 64; num++)
		{
			stb__Expand6[num] = (byte)((num << 2) | (num >> 4));
		}
		for (num = 0; num < 272; num++)
		{
			int a = ((num - 8 >= 0) ? ((num - 8 > 255) ? 255 : (num - 8)) : 0);
			stb__QuantRBTab[num] = stb__Expand5[StbDxt.stb__Mul8Bit(a, 31)];
			stb__QuantGTab[num] = stb__Expand6[StbDxt.stb__Mul8Bit(a, 63)];
		}
		fixed (byte* table = stb__OMatch5)
		{
			fixed (byte* expand = stb__Expand5)
			{
				fixed (byte* table2 = stb__OMatch6)
				{
					fixed (byte* expand2 = stb__Expand6)
					{
						StbDxt.stb__PrepareOptTable(table, expand, 32);
						StbDxt.stb__PrepareOptTable(table2, expand2, 64);
					}
				}
			}
		}
	}
}
