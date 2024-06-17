using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.PagesMenu;

public class CharInfoPagesMenuEntityVM : SelectionGroupEntityVM
{
	public readonly string Label;

	public readonly CharInfoPageType PageType;

	public CharInfoPagesMenuEntityVM(CharInfoPageType pageType, IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(allowSwitchOff: false)
	{
		PageType = pageType;
		Label = UIStrings.Instance.CharacterSheet.GetMenuLabel(pageType);
		AddDisposable(unit?.Subscribe(UpdateState));
	}

	protected override void DoSelectMe()
	{
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateState(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			SetAvailableState(PageType != CharInfoPageType.PsykerPowers || unit.GetStatOptional(StatType.PsyRating) != null);
		}
	}
}
