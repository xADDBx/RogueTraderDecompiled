using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.StateContext;

namespace Kingmaker.UI.Models.LevelUp;

public class ChargenUnit
{
	public readonly BlueprintUnit Blueprint;

	public BaseUnitEntity Unit;

	public bool Used;

	public ChargenUnit([NotNull] BlueprintUnit blueprint)
	{
		Blueprint = blueprint;
		RecreateUnit();
	}

	public void RecreateUnit()
	{
		using (ContextData<EditStateContext>.Request())
		{
			if (Unit != null && !Unit.IsDisposed)
			{
				Unit.Dispose();
			}
			using (ContextData<UnitHelper.ChargenUnit>.Request())
			{
				Unit = Blueprint.CreateEntity();
			}
			Unit.AttachToViewOnLoad(null);
			Used = false;
		}
	}
}
