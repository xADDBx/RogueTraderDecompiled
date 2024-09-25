using System;

namespace Core.RestServer.Common;

public struct RestServerStatus
{
	public Guid Guid { get; set; }

	public bool IsEditor { get; set; }

	public bool IsPlaying { get; set; }

	public bool IsHeadless { get; set; }

	public string[] LoadedMaps { get; set; }

	public int ProcessID { get; set; }
}
