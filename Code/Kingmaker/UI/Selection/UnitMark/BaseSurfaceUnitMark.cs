using System.Collections.Generic;

namespace Kingmaker.UI.Selection.UnitMark;

public abstract class BaseSurfaceUnitMark : BaseUnitMark
{
	protected List<UnitMarkDecal> AllUnitMarkDecal = new List<UnitMarkDecal>();

	protected override void OnEnabled()
	{
		AllUnitMarkDecal = GetAllDecals();
		base.OnEnabled();
	}

	protected abstract List<UnitMarkDecal> GetAllDecals();

	protected void SetUnitSize(bool isBig)
	{
		AllUnitMarkDecal.ForEach(delegate(UnitMarkDecal um)
		{
			um.SetBigSize(isBig);
		});
	}
}
