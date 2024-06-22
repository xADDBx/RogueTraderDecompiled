using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickTextConsoleView : TooltipBrickTextView, IConsoleTooltipBrick
{
	[SerializeField]
	private RectTransform m_TextContainer;

	[SerializeField]
	private OwlcatMultiButton m_FrameButtonPrefab;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonFirstFocus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonSecondFocus;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private FloatConsoleNavigationBehaviour m_FloatNavigationBehaviour;

	private GridConsoleNavigationBehaviour m_GridNavigationBehaviour;

	private List<IFloatConsoleNavigationEntity> m_LinkEntities = new List<IFloatConsoleNavigationEntity>();

	private readonly List<(TextMeshProUGUI, OwlcatMultiButton)> m_TextsCache = new List<(TextMeshProUGUI, OwlcatMultiButton)>();

	private IConsoleEntity m_CurrentNavigation;

	private RectTransform m_TextTransform;

	protected override void BindViewImplementation()
	{
		if ((object)m_TextTransform == null)
		{
			m_TextTransform = m_Text.GetComponent<RectTransform>();
		}
		m_TextContainer.gameObject.SetActive(value: true);
		m_TextsCache.ForEach(delegate((TextMeshProUGUI, OwlcatMultiButton) pair)
		{
			pair.Item1.gameObject.SetActive(value: false);
		});
		base.BindViewImplementation();
		AddDisposable(m_FloatNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		AddDisposable(m_GridNavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	public void CreateNavigation()
	{
		m_FloatNavigationBehaviour.Clear();
		m_GridNavigationBehaviour.Clear();
		AddDisposable(m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		if (m_TextContainer.sizeDelta.x == 0f || m_TextContainer.sizeDelta.y == 0f)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent.GetComponent<RectTransform>());
		}
		m_LinkEntities = TMPLinkNavigationGenerator.GenerateEntityList(m_Text, m_MultiButtonFirstFocus, m_MultiButtonSecondFocus, null, null, TooltipHelper.GetLinkTooltipTemplate);
		bool flag = m_LinkEntities.Count > 0 || base.ViewModel.IsHeader;
		m_TextContainer.gameObject.SetActive(flag);
		if (!flag)
		{
			m_GridNavigationBehaviour.AddColumn(SplitTextByParagraphs());
			m_CurrentNavigation = m_GridNavigationBehaviour;
			return;
		}
		if (m_LinkEntities.Count > 0)
		{
			m_FloatNavigationBehaviour.AddEntities(m_LinkEntities);
		}
		m_CurrentNavigation = m_FloatNavigationBehaviour;
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_CurrentNavigation;
	}

	private List<SimpleConsoleNavigationEntity> SplitTextByParagraphs()
	{
		List<SimpleConsoleNavigationEntity> list = new List<SimpleConsoleNavigationEntity>();
		string[] array = base.ViewModel.Text.Split('\n');
		for (int i = m_TextsCache.Count; i < array.Length; i++)
		{
			TextMeshProUGUI textMeshProUGUI = Object.Instantiate(m_Text, base.transform, worldPositionStays: false);
			OwlcatMultiButton owlcatMultiButton = Object.Instantiate(m_FrameButtonPrefab, textMeshProUGUI.transform, worldPositionStays: false);
			textMeshProUGUI.gameObject.SetActive(value: false);
			owlcatMultiButton.gameObject.SetActive(value: true);
			m_TextsCache.Add((textMeshProUGUI, owlcatMultiButton));
		}
		for (int j = 0; j < array.Length; j++)
		{
			(TextMeshProUGUI, OwlcatMultiButton) tuple = m_TextsCache[j];
			TextMeshProUGUI item = tuple.Item1;
			OwlcatMultiButton item2 = tuple.Item2;
			item.text = array[j];
			if (string.IsNullOrEmpty(array[j]))
			{
				item.text = "\n";
			}
			ApplyStyleTo(item, base.ViewModel.Type);
			item.gameObject.SetActive(value: true);
			if (!string.IsNullOrEmpty(array[j]))
			{
				list.Add(new SimpleConsoleNavigationEntity(item2));
			}
		}
		return list;
	}

	protected override void ChangeTextSize()
	{
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
