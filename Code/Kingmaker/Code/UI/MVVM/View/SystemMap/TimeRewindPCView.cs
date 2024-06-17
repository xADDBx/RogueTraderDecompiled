using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class TimeRewindPCView : ViewBase<TimeRewindVM>
{
	[SerializeField]
	private GameObject m_TimeRewindBlock;

	[Header("Text Fields")]
	[SerializeField]
	protected TextMeshProUGUI m_CurrentTimeSegment;

	[SerializeField]
	protected TextMeshProUGUI m_CurrentFullTime;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(delegate
		{
			ShowPart();
		}));
		AddDisposable(base.ViewModel.CurrentSegment.Subscribe(delegate(float currentTimeSegment)
		{
			m_CurrentTimeSegment.text = currentTimeSegment + " :";
		}));
		AddDisposable(base.ViewModel.CurrentVVYear.CombineLatest(base.ViewModel.CurrentAMRCYear, base.ViewModel.CurrentMillenium, (float vv, float anrc, float mill) => new { vv, anrc, mill }).Subscribe(value =>
		{
			m_CurrentFullTime.text = $"{value.vv}VV.{value.anrc}ANRC.M{value.mill}";
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private bool OnShow()
	{
		return base.ViewModel.ShouldShow.Value;
	}

	private void ShowPart()
	{
		m_TimeRewindBlock.SetActive(OnShow());
	}
}
