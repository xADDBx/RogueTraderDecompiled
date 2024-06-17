using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickNonStackView : TooltipBaseBrickView<TooltipBrickNonStackVm>
{
	[SerializeField]
	private TextMeshProUGUI m_Header;

	[SerializeField]
	private TooltipBrickNonStackEntityView m_EntityView;

	private readonly List<TooltipBrickNonStackEntityView> m_Entities = new List<TooltipBrickNonStackEntityView>();

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Header.text = UIStrings.Instance.Tooltips.NonStackHeaderLabel;
		m_Header.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		foreach (TooltipBrickNonStackVm.NonStackEntity entity in base.ViewModel.Entities)
		{
			TooltipBrickNonStackEntityView widget = WidgetFactory.GetWidget(m_EntityView);
			widget.transform.SetParent(base.transform, worldPositionStays: false);
			widget.Initialize(entity);
			m_Entities.Add(widget);
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Entities.ForEach(WidgetFactory.DisposeWidget);
		m_Entities.Clear();
	}
}
