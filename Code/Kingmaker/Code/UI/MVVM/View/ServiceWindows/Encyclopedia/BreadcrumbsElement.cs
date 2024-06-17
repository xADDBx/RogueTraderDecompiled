using JetBrains.Annotations;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia;

public class BreadcrumbsElement : Selectable, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	[UsedImplicitly]
	private GameObject m_Point;

	private INode m_Blueprint;

	private bool m_IsLastElement;

	public BreadcrumbsElement GetCopyInstance()
	{
		return Object.Instantiate(this).GetComponent<BreadcrumbsElement>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (base.interactable)
		{
			EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
			{
				x.HandleEncyclopediaPage(m_Blueprint);
			});
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (!m_IsLastElement)
		{
			m_Label.fontStyle = FontStyles.Normal;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (!m_IsLastElement)
		{
			m_Label.fontStyle = FontStyles.Underline;
		}
	}

	public void Prepare()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Initialize(Transform parent, INode node, bool isFirstElement, bool isLastElement)
	{
		m_Blueprint = node;
		m_IsLastElement = isLastElement;
		base.gameObject.SetActive(value: true);
		base.transform.SetParent(parent);
		base.transform.ResetAll();
		base.transform.SetAsFirstSibling();
		m_Label.text = node.GetTitle();
		m_Point.SetActive(!isFirstElement);
		m_Label.fontStyle = ((!isLastElement) ? FontStyles.Underline : FontStyles.Normal);
		base.interactable = !isLastElement;
	}

	public void Dispose()
	{
		Object.Destroy(base.gameObject);
	}
}
