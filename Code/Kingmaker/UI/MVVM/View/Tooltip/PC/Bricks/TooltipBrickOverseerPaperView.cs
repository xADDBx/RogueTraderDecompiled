using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickOverseerPaperView : TooltipBaseBrickView<TooltipBrickOverseerPaperVM>
{
	[SerializeField]
	protected Image m_Icon;

	[FormerlySerializedAs("m_Name")]
	[SerializeField]
	protected TextMeshProUGUI m_CharacterName;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private OwlcatMultiSelectable m_PortraitObject;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_CharacterName.text = base.ViewModel.Name;
		m_Title.text = base.ViewModel.Title;
		AddDisposable(m_PortraitObject.OnPointerClickAsObservable().Subscribe(RequestShowInspect));
	}

	private void RequestShowInspect(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.UnitToShow);
		});
	}
}
