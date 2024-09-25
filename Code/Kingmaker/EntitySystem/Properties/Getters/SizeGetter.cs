using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("571bda39fcd97c741ae37d36c4688bad")]
public class SizeGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " size";
	}

	protected override int GetBaseValue()
	{
		return GetSizeValue(base.CurrentEntity.Size);
	}

	private static int GetSizeValue(Size size)
	{
		return size switch
		{
			Size.Medium => 0, 
			Size.Large => 1, 
			Size.Huge => 2, 
			Size.Gargantuan => 3, 
			Size.Colossal => 4, 
			Size.Raider_1x1 => 5, 
			Size.Frigate_1x2 => 6, 
			Size.Cruiser_2x4 => 7, 
			Size.GrandCruiser_3x6 => 8, 
			_ => throw new ArgumentOutOfRangeException("size", size, null), 
		};
	}
}
