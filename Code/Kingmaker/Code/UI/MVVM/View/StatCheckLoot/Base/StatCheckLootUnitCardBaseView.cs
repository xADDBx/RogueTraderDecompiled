using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;

public class StatCheckLootUnitCardBaseView : ViewBase<StatCheckLootUnitCardVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_UnitNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_StatNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_StatValueLabel;

	[SerializeField]
	private Image m_UnitPortraitImage;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	public void Initialize()
	{
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

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
		AddDisposable(base.ViewModel.StatName.Subscribe(delegate(string val)
		{
			m_StatNameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.StatValue.Subscribe(delegate(int val)
		{
			m_StatValueLabel.text = val.ToString();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public void SetInput(InputLayer inputLayer)
	{
		SetInputImpl(inputLayer);
	}

	protected virtual void SetInputImpl(InputLayer inputLayer)
	{
	}

	protected void OnCheckStat()
	{
		base.ViewModel.CheckStat();
	}

	protected void OnSwitchUnit()
	{
		base.ViewModel.SwitchUnit();
	}
}
