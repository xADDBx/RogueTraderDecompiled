using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;

namespace Kingmaker.DLC;

[TypeId("082e047e4941469eb46d59cb65c22dc5")]
public class DlcStoreConsoleFree : DlcStore
{
	[Flags]
	public enum PlatformsType
	{
		None = 0,
		PC = 1,
		Mac = 2,
		Playstation = 4,
		XBox = 8,
		Switch = 0x10
	}

	public PlatformsType AvailableOn = PlatformsType.Playstation | PlatformsType.XBox;

	public override bool IsSuitable => GetStatus() != null;

	public override IDLCStatus GetStatus()
	{
		if (!AvailableOn.HasFlag(PlatformsType.PC))
		{
			return null;
		}
		return DLCStatus.Available;
	}
}
