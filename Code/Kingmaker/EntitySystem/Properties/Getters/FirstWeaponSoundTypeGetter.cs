using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("28e8a5e4df1443fab3382b9987b613ef")]
public class FirstWeaponSoundTypeGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current weapon of " + FormulaTargetScope.Current + " sound type";
	}

	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.GetFirstWeapon()?.Blueprint.VisualParameters.SoundTypeSwitch.ValueHash ?? 0);
	}
}
