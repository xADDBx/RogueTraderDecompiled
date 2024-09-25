using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Owlcat.Runtime.Core.Logging;

internal static class UnityUberLogJointSuppressor
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Suppressor : IDisposable
	{
		public void Dispose()
		{
			SuppressCount.Value--;
		}
	}

	private static readonly ThreadLocal<int> SuppressCount = new ThreadLocal<int>();

	public static bool IsSuppressed => SuppressCount.Value > 0;

	public static Suppressor Suppress()
	{
		SuppressCount.Value++;
		return default(Suppressor);
	}
}
