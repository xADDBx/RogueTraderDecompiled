using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.Utility.CodeTimer;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProfileScope : IDisposable
{
	public static bool Disable { get; set; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Open(string name, UnityEngine.Object ctx)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ProfileScope? NewScope([CallerMemberName] string text = "")
	{
		return New(text);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ProfileScope? New(string text, UnityEngine.Object ctx = null)
	{
		if (!BuildModeUtility.IsDevelopment || Disable)
		{
			return null;
		}
		Open(text, ctx);
		return default(ProfileScope);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
	}
}
