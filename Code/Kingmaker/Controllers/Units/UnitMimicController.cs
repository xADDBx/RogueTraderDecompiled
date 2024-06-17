using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

public class UnitMimicController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitPartMimic optional = unit.GetOptional<UnitPartMimic>();
		if (!optional || !optional.AmbushObjectAttached)
		{
			return;
		}
		if (unit.IsVisibleForPlayer)
		{
			optional.HideAmbushObject();
			unit.View.Fader.Or(null)?.FastForward();
			return;
		}
		MapObjectView mapObjectView = optional.AmbushObject?.View;
		if (mapObjectView != null)
		{
			mapObjectView.ViewTransform.position = unit.View.ViewTransform.localPosition;
			mapObjectView.ViewTransform.rotation = unit.View.ViewTransform.localRotation;
		}
	}
}
