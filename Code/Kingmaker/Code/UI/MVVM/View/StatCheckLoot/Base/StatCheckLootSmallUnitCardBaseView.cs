using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;

public class StatCheckLootSmallUnitCardBaseView : ViewBase<StatCheckLootSmallUnitCardVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_UnitNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_StatValueLabel;

	[SerializeField]
	private Image m_UnitPortraitImage;

	[SerializeField]
	private OwlcatToggle m_SelectUnitToggle;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UnitName.Subscribe(delegate(string val)
		{
			m_UnitNameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.UnitPortrait.Subscribe(delegate(Sprite val)
		{
			m_UnitPortraitImage.sprite = val;
		}));
		AddDisposable(base.ViewModel.StatValue.Subscribe(delegate(int val)
		{
			m_StatValueLabel.text = val.ToString();
		}));
		AddDisposable(base.ViewModel.IsSelected.Subscribe(delegate(bool val)
		{
			m_SelectUnitToggle.Set(val);
		}));
		AddDisposable(m_SelectUnitToggle.IsOn.Subscribe(HandleSelectUnitClick));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetToggleGroup(OwlcatToggleGroup toggleGroup)
	{
		m_SelectUnitToggle.Group = toggleGroup;
	}

	private void HandleSelectUnitClick(bool isSelected)
	{
		if (isSelected)
		{
			base.ViewModel.SelectUnit();
		}
	}

	public void SetFocus(bool value)
	{
		m_SelectUnitToggle.Set(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}
}
