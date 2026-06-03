using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Sound;

namespace Kingmaker.UI.MVVM.VM.Inspect;

public class AugmentationsInspectVM : InGameInspectVM
{
	public bool IsShown;

	protected override void OnUnitInvoke(AbstractUnitEntity entity)
	{
		if (entity is BaseUnitEntity value)
		{
			if (!IsShown)
			{
				UISounds.Instance.Sounds.AugmentationsWindow.AugmentsInspectOpen.Play();
			}
			m_Unit.Value = value;
			Game.Instance.Player.UISettings.ShowInspect = true;
			m_Disposable?.Dispose();
			Tooltip.Value = new TooltipTemplateAugmentationsInspect(m_Unit);
			IsShown = true;
		}
	}

	protected override void HideInspect()
	{
		base.HideInspect();
		UISounds.Instance.Sounds.AugmentationsWindow.AugmentInspectClose.Play();
		IsShown = false;
	}
}
