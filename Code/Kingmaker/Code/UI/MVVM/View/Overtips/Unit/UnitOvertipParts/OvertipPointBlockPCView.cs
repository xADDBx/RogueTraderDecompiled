using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipPointBlockPCView : ViewBase<OvertipPointBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ActivePointsText;

	[SerializeField]
	private TextMeshProUGUI m_MovePointsText;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ActionPoints.Subscribe(delegate(float value)
		{
			m_ActivePointsText.text = $"{value} ap";
		}));
		AddDisposable(base.ViewModel.MovePoints.Subscribe(delegate(float value)
		{
			m_MovePointsText.text = $"{value} mp";
		}));
		AddDisposable(base.ViewModel.NeedToShow.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
