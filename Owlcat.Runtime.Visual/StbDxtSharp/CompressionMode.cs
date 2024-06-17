using System;

namespace StbDxtSharp;

[Flags]
public enum CompressionMode
{
	None = 0,
	Dithered = 1,
	HighQuality = 2
}
