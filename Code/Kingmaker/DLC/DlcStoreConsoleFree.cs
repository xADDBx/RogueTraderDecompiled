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

	public override bool IsSuitable
	{
		get
		{
			IDLCStatus value;
			return TryGetStatus(out value);
		}
	}

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on DlcStoreConsoleFree";

	public override bool TryGetStatus(out IDLCStatus value)
	{
		value = null;
		try
		{
			value = (AvailableOn.HasFlag(PlatformsType.PC) ? DLCStatus.Available : null);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return value != null;
	}
}
