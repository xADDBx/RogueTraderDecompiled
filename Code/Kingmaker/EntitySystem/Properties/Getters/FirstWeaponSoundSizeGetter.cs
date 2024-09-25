using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("763882f4485a41b8a232778836d8dbc4")]
public class FirstWeaponSoundSizeGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current weapon of " + FormulaTargetScope.Current + " sound size";
	}

	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.GetFirstWeapon()?.Blueprint.VisualParameters.SoundSizeSwitch.ValueHash ?? 0);
	}
}
