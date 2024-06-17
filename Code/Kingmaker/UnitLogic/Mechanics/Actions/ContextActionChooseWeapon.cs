using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("15d40cd5c6d3e5449871c546a35ececa")]
public class ContextActionChooseWeapon : ContextAction
{
	public bool ForgetWeapon;

	public override string GetCaption()
	{
		return "Store equipped weapon into unit part for later use";
	}

	public override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		WarhammerUnitPartChooseWeapon orCreate = entity.GetOrCreate<WarhammerUnitPartChooseWeapon>();
		if (!ForgetWeapon)
		{
			ItemEntityWeapon firstWeapon = entity.GetFirstWeapon();
			if (firstWeapon != null)
			{
				orCreate.ChooseWeapon(firstWeapon);
			}
		}
		else
		{
			orCreate.RemoveWeapon();
		}
	}
}
