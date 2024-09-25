using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

public readonly struct NavmeshMasksGeneration : IEquatable<NavmeshMasksGeneration>
{
	private readonly Color[] Data;

	private readonly int Height;

	private readonly int Width;

	public NavmeshMasksGeneration(Color[] data, int height, int width)
	{
		Data = data;
		Height = height;
		Width = width;
	}

	public bool IsRemoved(int x, int z)
	{
		Color cell = GetCell(x, z);
		if (!(cell.r > 0.5f))
		{
			return cell.b > 0.5f;
		}
		return true;
	}

	public bool IsAdded(int x, int z)
	{
		return GetCell(x, z).g > 0.5f;
	}

	private Color GetCell(int x, int z)
	{
		if (Data == null || x < 0 || z < 0 || x >= Width || z >= Height)
		{
			return Color.clear;
		}
		return Data[z * Width + x];
	}

	public override bool Equals(object obj)
	{
		if (obj is NavmeshMasksGeneration other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(NavmeshMasksGeneration other)
	{
		if (EqualityComparer<Color[]>.Default.Equals(Data, other.Data) && Height == other.Height)
		{
			return Width == other.Width;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = (-1098418262 * -1521134295 + EqualityComparer<Color[]>.Default.GetHashCode(Data)) * -1521134295;
		int height = Height;
		int num2 = (num + height.GetHashCode()) * -1521134295;
		height = Width;
		return num2 + height.GetHashCode();
	}

	public static bool operator ==(NavmeshMasksGeneration left, NavmeshMasksGeneration right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NavmeshMasksGeneration left, NavmeshMasksGeneration right)
	{
		return !(left == right);
	}
}
