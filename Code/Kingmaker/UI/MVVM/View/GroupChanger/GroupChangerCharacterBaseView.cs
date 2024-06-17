using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.GroupChanger;

public class GroupChangerCharacterBaseView : ViewBase<GroupChangerCharacterVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private Image m_Lock;

	[SerializeField]
	private GameObject m_LevelUp;

	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private TextMeshProUGUI m_CharacterLevel;

	[Header("Parts")]
	[SerializeField]
	private UnitBuffPartPCView m_BuffPartView;

	[SerializeField]
	private UnitPortraitPartPCView m_PortraitPartView;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsInParty.CombineLatest(base.ViewModel.IsLock, (bool isInParty, bool isLock) => new { isInParty, isLock }).Subscribe(value =>
		{
			SetState(value.isInParty, value.isLock);
		}));
		AddDisposable(m_Lock.SetHint(UIStrings.Instance.GroupChangerTexts.MustBeInPartyHint.Text));
		m_BuffPartView.Bind(base.ViewModel.BuffPartVm);
		m_PortraitPartView.Bind(base.ViewModel.PortraitPartVm);
		m_LevelUp.SetActive(base.ViewModel.IsLevelUp);
		m_CharacterName.text = base.ViewModel.CharacterName;
		TextMeshProUGUI characterLevel = m_CharacterLevel;
		int characterLevel2 = base.ViewModel.CharacterLevel;
		characterLevel.text = characterLevel2.ToString();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public UnitReference GetUnitReference()
	{
		return base.ViewModel.UnitRef;
	}

	public IConsoleNavigationEntity GetNavigationEntity()
	{
		return this;
	}

	public void SetFocus(bool value)
	{
		base.ViewModel.SetFocused(value);
		if (m_Selectable != null)
		{
			m_Selectable.SetFocus(value);
		}
	}

	public bool IsValid()
	{
		return true;
	}

	protected virtual void SetState(bool isInParty, bool isLock)
	{
		if (isLock)
		{
			m_Selectable.SetActiveLayer("Locked");
		}
		else
		{
			m_Selectable.SetActiveLayer(isInParty ? "Selected" : "Unselected");
		}
	}
}
