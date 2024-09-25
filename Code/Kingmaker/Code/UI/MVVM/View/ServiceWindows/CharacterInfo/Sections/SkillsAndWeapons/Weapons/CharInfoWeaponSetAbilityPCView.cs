using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class CharInfoWeaponSetAbilityPCView : ViewBase<CharInfoWeaponSetAbilityVM>, IWidgetView
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	private SimpleConsoleNavigationEntity m_NavigationEntity;

	public SimpleConsoleNavigationEntity NavigationEntity => m_NavigationEntity;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite value)
		{
			m_Icon.sprite = value;
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
		m_NavigationEntity = new SimpleConsoleNavigationEntity(m_Button, base.ViewModel.Tooltip);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoWeaponSetAbilityVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoWeaponSetAbilityVM;
	}
}
