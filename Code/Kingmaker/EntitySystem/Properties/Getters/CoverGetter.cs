using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5122bc0b20863d749bd0fc23b8ac58d7")]
public class CoverGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[Flags]
	public enum CoverType
	{
		None = 1,
		Half = 2,
		Full = 4,
		Invisible = 8
	}

	public PropertyTargetType Target;

	public bool UseBestShootingPosition;

	[EnumFlagsAsDropdown]
	public CoverType Covers;

	protected override int GetBaseValue()
	{
		LosCalculations.CoverType coverType = ((!UseBestShootingPosition) ? LosCalculations.GetWarhammerLos(base.CurrentEntity, (MechanicEntity)this.GetTargetByType(Target)).CoverType : LosCalculations.GetWarhammerLos(LosCalculations.GetBestShootingPosition(base.CurrentEntity, (MechanicEntity)this.GetTargetByType(Target)), base.CurrentEntity.SizeRect, this.GetTargetByType(Target).Position, ((MechanicEntity)this.GetTargetByType(Target)).SizeRect).CoverType);
		if (!Covers.HasFlag(coverType switch
		{
			LosCalculations.CoverType.Full => CoverType.Full, 
			LosCalculations.CoverType.Half => CoverType.Half, 
			LosCalculations.CoverType.Invisible => CoverType.Invisible, 
			_ => CoverType.None, 
		}))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return $"Cover to {Target}";
	}
}
