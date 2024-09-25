using Kingmaker.Code.UI.MVVM.View.Colonization.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalOrderResourcesConsoleView : ColonyResourceConsoleView
{
	[SerializeField]
	private Image m_Arrow;

	[SerializeField]
	private Sprite m_GreenArrow;

	[SerializeField]
	private Sprite m_RedArrow;

	[SerializeField]
	private OwlcatSelectable m_OwlcatSelectable;

	[SerializeField]
	private Image m_BackgroundImage;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.ArrowDirection.Subscribe(delegate(int value)
		{
			m_OwlcatSelectable.SetFocus(value != 0);
			m_Arrow.gameObject.SetActive(value != 0);
			if (value != 0)
			{
				m_Arrow.sprite = ((value == 1) ? m_GreenArrow : m_RedArrow);
			}
		}));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int val)
		{
			AddDisposable(m_BackgroundImage.SetTooltip(new TooltipTemplateColonyResource(base.ViewModel.BlueprintResource.Value, val)));
		}));
	}
}
