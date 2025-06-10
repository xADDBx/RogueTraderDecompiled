using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Components;
using UniRx;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;

public class PetSummaryVM : CharInfoComponentVM
{
	public readonly ReactiveCommand RefreshCommand = new ReactiveCommand();

	public readonly ReactiveProperty<bool> IsUnitPet = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<VideoClip> PetVideoClip = new ReactiveProperty<VideoClip>();

	public readonly ReactiveProperty<string> NarrativeDescription = new ReactiveProperty<string>();

	public readonly ReactiveProperty<TooltipBrickTextVM> StrategyDescription = new ReactiveProperty<TooltipBrickTextVM>();

	public readonly ReactiveProperty<string> TipsDescription = new ReactiveProperty<string>();

	public PetSummaryVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		RefreshData();
	}

	protected sealed override void RefreshData()
	{
		base.RefreshData();
		PetCharscreenUnitComponent component = Unit.Value.Blueprint.GetComponent<PetCharscreenUnitComponent>();
		if (component != null)
		{
			PetVideoClip.Value = component.PetGameplayVideoLink.Load();
			StrategyDescription.Value = new TooltipBrickTextVM(component.StrategyDescription, TooltipTextType.Simple);
			TipsDescription.Value = component.TipsDescription;
			NarrativeDescription.Value = component.NarrativeDescription.Get().Description;
		}
	}
}
