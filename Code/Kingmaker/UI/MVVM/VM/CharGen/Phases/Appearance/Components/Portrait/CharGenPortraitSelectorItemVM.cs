using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;

public class CharGenPortraitSelectorItemVM : SelectionGroupEntityVM
{
	private readonly Action m_OnPortraitCreate;

	private readonly Action<CharGenPortraitSelectorItemVM> m_OnPortraitChange;

	private readonly BlueprintPortrait m_BlueprintPortrait;

	public readonly PortraitData PortraitData;

	public readonly bool IsCustom;

	public bool CustomPortraitCreatorItem { get; }

	public CharGenPortraitSelectorItemVM(BlueprintPortrait blueprintPortrait, bool custom = false)
		: base(allowSwitchOff: false)
	{
		m_BlueprintPortrait = blueprintPortrait;
		IsCustom = custom;
		if (!custom)
		{
			PortraitData = m_BlueprintPortrait.Data;
		}
	}

	public CharGenPortraitSelectorItemVM(PortraitData portraitData, Action<CharGenPortraitSelectorItemVM> onPortraitChange)
		: this(BlueprintRoot.Instance.CharGenRoot.CustomPortrait, custom: true)
	{
		PortraitData = portraitData;
		m_OnPortraitChange = onPortraitChange;
	}

	public CharGenPortraitSelectorItemVM(Action onPortraitCreate)
		: base(allowSwitchOff: false)
	{
		m_OnPortraitCreate = onPortraitCreate;
		CustomPortraitCreatorItem = true;
	}

	public BlueprintPortrait GetBlueprintPortrait()
	{
		m_BlueprintPortrait.Data = PortraitData;
		return m_BlueprintPortrait;
	}

	public void OnCustomPortraitCreate()
	{
		m_OnPortraitCreate?.Invoke();
	}

	public void OnCustomPortraitChange()
	{
		m_OnPortraitChange?.Invoke(this);
	}

	protected override void DoSelectMe()
	{
	}

	protected override void DisposeImplementation()
	{
	}
}
