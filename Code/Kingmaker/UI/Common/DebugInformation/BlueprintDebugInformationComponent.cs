using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.Common.DebugInformation;

public class BlueprintDebugInformationComponent : MonoBehaviour, IDebugInformationUIHandler, ISubscriber
{
	[SerializeField]
	private RectTransform m_ParentTransform;

	[SerializeField]
	private TextAnchor m_BubblePosition = TextAnchor.UpperRight;

	private BlueprintDebugInformationBubble m_InformationBubble;

	private bool m_IsDevelopment;

	private void OnEnable()
	{
		m_IsDevelopment = false;
		if (m_IsDevelopment)
		{
			EventBus.Subscribe(this);
			if (Game.Instance.RootUiContext.IsDebugBlueprintsInformationShow)
			{
				DelayedInvoker.InvokeInFrames(HandleShowDebugBubble, 3);
			}
		}
	}

	private void OnDisable()
	{
		if (m_IsDevelopment)
		{
			HandleHideDebugBubble();
			EventBus.Unsubscribe(this);
		}
	}

	private BlueprintScriptableObject GetBlueprintInformation()
	{
		return GetComponent<IHasBlueprintInfo>()?.Blueprint;
	}

	public void HandleShowDebugBubble()
	{
		if (m_IsDevelopment)
		{
			BlueprintScriptableObject blueprintInformation = GetBlueprintInformation();
			if (blueprintInformation != null && !(m_InformationBubble != null))
			{
				m_InformationBubble = WidgetFactory.GetWidget(UIConfig.Instance.DebugBubble, activate: true, strictMatching: true);
				m_InformationBubble.Initialize(blueprintInformation);
				Transform transform;
				(transform = m_InformationBubble.transform).SetParent(m_ParentTransform, worldPositionStays: false);
				SetBubblePosition(transform as RectTransform);
			}
		}
	}

	public void HandleHideDebugBubble()
	{
		if (m_IsDevelopment && !(m_InformationBubble == null))
		{
			m_InformationBubble.Dispose();
			WidgetFactory.DisposeWidget(m_InformationBubble);
			m_InformationBubble = null;
		}
	}

	private void SetBubblePosition(RectTransform rectTransform)
	{
		if (m_IsDevelopment)
		{
			Dictionary<TextAnchor, Vector2> dictionary = new Dictionary<TextAnchor, Vector2>
			{
				{
					TextAnchor.UpperLeft,
					new Vector2(0f, 1f)
				},
				{
					TextAnchor.UpperCenter,
					new Vector2(0.5f, 1f)
				},
				{
					TextAnchor.UpperRight,
					new Vector2(1f, 1f)
				},
				{
					TextAnchor.MiddleLeft,
					new Vector2(0f, 0.5f)
				},
				{
					TextAnchor.MiddleCenter,
					new Vector2(0.5f, 0.5f)
				},
				{
					TextAnchor.MiddleRight,
					new Vector2(1f, 0.5f)
				},
				{
					TextAnchor.LowerLeft,
					new Vector2(0f, 0f)
				},
				{
					TextAnchor.LowerCenter,
					new Vector2(0.5f, 0f)
				},
				{
					TextAnchor.LowerRight,
					new Vector2(1f, 0f)
				}
			};
			Vector2 anchorMax = (rectTransform.anchorMin = (rectTransform.pivot = (dictionary.ContainsKey(m_BubblePosition) ? dictionary[m_BubblePosition] : new Vector2(1f, 1f))));
			rectTransform.anchorMax = anchorMax;
			rectTransform.anchoredPosition = Vector2.zero;
		}
	}
}
