using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class RankEntryDescriptionView : VirtualListElementViewBase<RankEntryDescriptionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		m_Description.text = base.ViewModel.Description;
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_LayoutSettings.Height = m_Description.rectTransform.rect.height;
		}, 1);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
