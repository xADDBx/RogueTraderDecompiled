using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Party.Console;

public class PartySelectorItemConsoleView : ViewBase<PartyCharacterVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private UnitPortraitPartPCView m_PortraitView;

	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private GameObject m_ConnectedIcon;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private GameObject m_LevelUpObject;

	public BaseUnitEntity UnitEntityData => base.ViewModel.UnitEntityData;

	public IReadOnlyReactiveProperty<bool> IsLinked => base.ViewModel.IsLinked;

	public IReadOnlyReactiveProperty<bool> IsLevelUp => base.ViewModel.IsLevelUp;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_PortraitView.Bind(base.ViewModel.PortraitPartVM);
		AddDisposable(base.ViewModel.CharacterName.Subscribe(delegate(string n)
		{
			m_CharacterName.text = n;
		}));
		if (!RootUIContext.Instance.IsSpace)
		{
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			if (loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode)
			{
				AddDisposable(base.ViewModel.IsLinked.Subscribe(m_ConnectedIcon.SetActive));
				goto IL_00a2;
			}
		}
		m_ConnectedIcon.SetActive(value: false);
		goto IL_00a2;
		IL_00a2:
		AddDisposable(base.ViewModel.IsLevelUp.Subscribe(m_LevelUpObject.SetActive));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetSelected()
	{
		base.ViewModel.HandleUnitClick();
	}

	public void LevelUp()
	{
		base.ViewModel.LevelUp();
	}

	public void SetLink()
	{
		base.ViewModel.ToggleLinkUnit();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		SetSelected();
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
