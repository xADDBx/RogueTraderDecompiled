using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Mechanics.Actions;

[TypeId("0d122ac7f4ba21c48943faac877cfa63")]
public class ContextActionStarSystemTargetScope : ContextAction
{
	public enum TargetType
	{
		ScannerUnit,
		Starship,
		Party
	}

	public new TargetType Target;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions with specific target type that differs from main";
	}

	public override void RunAction()
	{
		if (Target == TargetType.Party)
		{
			foreach (BaseUnitEntity item in Game.Instance.Player.Party)
			{
				using (base.Context.GetDataScope(item.ToTargetWrapper()))
				{
					Actions.Run();
				}
			}
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		baseUnitEntity = Target switch
		{
			TargetType.ScannerUnit => ContextData<StarSystemContextData>.Current?.TargetUnit, 
			TargetType.Starship => ContextData<StarSystemContextData>.Current?.Starship, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		using (base.Context.GetDataScope(baseUnitEntity.ToTargetWrapper()))
		{
			Actions.Run();
		}
	}
}
