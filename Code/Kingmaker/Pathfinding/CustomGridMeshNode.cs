using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public struct CustomGridMeshNode
{
	private const float kMaxOffset = 2f;

	private const int kQuantizationRangeMax = 255;

	private const int kQuantizationRangeHalf = 127;

	private const float kQuantizationStep = 0.015748031f;

	public byte packedCornerOffsetSW;

	public byte packedCornerOffsetSE;

	public byte packedCornerOffsetNW;

	public byte packedCornerOffsetNE;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Pack(float originHeight, float cornerHeightSW, float cornerHeightSE, float cornerHeightNW, float cornerHeightNE)
	{
		int num = Mathf.FloorToInt(originHeight / 0.015748031f);
		int num2 = Mathf.FloorToInt(cornerHeightSW / 0.015748031f);
		int num3 = Mathf.FloorToInt(cornerHeightSE / 0.015748031f);
		int num4 = Mathf.FloorToInt(cornerHeightNW / 0.015748031f);
		int num5 = Mathf.FloorToInt(cornerHeightNE / 0.015748031f);
		int num6 = num2 - num;
		int num7 = num3 - num;
		int num8 = num4 - num;
		int num9 = num5 - num;
		packedCornerOffsetSW = (byte)Mathf.Clamp(num6 + 127, 0, 255);
		packedCornerOffsetSE = (byte)Mathf.Clamp(num7 + 127, 0, 255);
		packedCornerOffsetNW = (byte)Mathf.Clamp(num8 + 127, 0, 255);
		packedCornerOffsetNE = (byte)Mathf.Clamp(num9 + 127, 0, 255);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Unpack(float originHeight, out float cornerHeightSW, out float cornerHeightSE, out float cornerHeightNW, out float cornerHeightNE)
	{
		int num = packedCornerOffsetSW - 127;
		int num2 = packedCornerOffsetSE - 127;
		int num3 = packedCornerOffsetNW - 127;
		int num4 = packedCornerOffsetNE - 127;
		int num5 = Mathf.FloorToInt(originHeight / 0.015748031f);
		int num6 = num5 + num;
		int num7 = num5 + num2;
		int num8 = num5 + num3;
		int num9 = num5 + num4;
		cornerHeightSW = (float)num6 * 0.015748031f;
		cornerHeightSE = (float)num7 * 0.015748031f;
		cornerHeightNW = (float)num8 * 0.015748031f;
		cornerHeightNE = (float)num9 * 0.015748031f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Read(BinaryReader reader)
	{
		packedCornerOffsetSW = reader.ReadByte();
		packedCornerOffsetSE = reader.ReadByte();
		packedCornerOffsetNW = reader.ReadByte();
		packedCornerOffsetNE = reader.ReadByte();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write(BinaryWriter writer)
	{
		writer.Write(packedCornerOffsetSW);
		writer.Write(packedCornerOffsetSE);
		writer.Write(packedCornerOffsetNW);
		writer.Write(packedCornerOffsetNE);
	}
}
