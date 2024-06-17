using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;

public class SurfaceCombatUnitOrderVerticalPCView : SurfaceCombatUnitOrderVerticalView
{
	private string m_HintLabel;

	private IDisposable m_HintSubscription;

	protected override void InternalBind()
	{
		base.InternalBind();
		if (base.ViewModel.Round != null)
		{
			AddDisposable(base.ViewModel.Round.Subscribe(delegate
			{
				m_HintLabel = UIStrings.Instance.TurnBasedTexts.NextRound.Text;
			}));
		}
		if (base.ViewModel.OrderIndex != null)
		{
			AddDisposable(base.ViewModel.OrderIndex.Subscribe(delegate(int index)
			{
				m_HintLabel = ((index == 0) ? ((string)UIStrings.Instance.TurnBasedTexts.CurrentUnit) : string.Format(UIStrings.Instance.TurnBasedTexts.UnitOrder, index));
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HintSubscription?.Dispose();
	}

	public override void HandleHighlightChange(bool isOn)
	{
		base.HandleHighlightChange(isOn);
		m_HintSubscription?.Dispose();
		if (isOn)
		{
			m_HintSubscription = this.SetHint(m_HintLabel);
		}
	}
}
