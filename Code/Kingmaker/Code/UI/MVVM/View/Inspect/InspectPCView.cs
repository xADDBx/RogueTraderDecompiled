using Kingmaker.Code.UI.MVVM.View.InfoWindow.PC;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Inspect;

public class InspectPCView : ViewBase<InspectVM>, IAbilityTargetSelectionUIHandler, ISubscriber, ITurnBasedModeHandler
{
	[SerializeField]
	private InfoWindowPCView m_InfoWindow;

	private InfoWindowVM m_InfoWindowVM;

	public void Initialize()
	{
		m_InfoWindow.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.Tooltip.Skip(1).Subscribe(delegate(TooltipBaseTemplate value)
		{
			m_InfoWindowVM?.Dispose();
			if (value == null)
			{
				m_InfoWindow.Hide();
			}
			else
			{
				m_InfoWindowVM = new InfoWindowVM(value, Close);
				m_InfoWindow.Bind(m_InfoWindowVM);
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_InfoWindowVM?.Dispose();
		m_InfoWindowVM = null;
	}

	private void Close()
	{
		m_InfoWindow.Hide();
		Game.Instance.Player.UISettings.ShowInspect = false;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		Close();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		Close();
	}
}
