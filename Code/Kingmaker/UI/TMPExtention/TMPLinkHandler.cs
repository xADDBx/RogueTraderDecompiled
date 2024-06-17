using System;
using Kingmaker.Utility.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.TMPExtention;

public class TMPLinkHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	protected enum TransnitionState
	{
		Normal,
		Highlighted,
		Pressed
	}

	[Serializable]
	public class LinkEventData : UnityEvent<PointerEventData, TMP_LinkInfo>
	{
	}

	[SerializeField]
	protected bool m_Interactable = true;

	[SerializeField]
	protected bool m_ColorTransnition = true;

	[SerializeField]
	protected bool m_OverlayNormalColor;

	[SerializeField]
	protected Color m_NormalColor = Color.white;

	[SerializeField]
	protected Color m_HighlightedColor = Color.blue;

	[SerializeField]
	protected bool m_HighlightedUnderline = true;

	protected TextMeshProUGUI m_TextComponent;

	protected PointerEventData m_CurrentEventData;

	protected int m_HoverdLink = -1;

	protected int m_DownIndex = -1;

	[SerializeField]
	protected LinkEventData m_OnClick = new LinkEventData();

	[SerializeField]
	protected LinkEventData m_OnEnter = new LinkEventData();

	[SerializeField]
	protected LinkEventData m_OnHover = new LinkEventData();

	[SerializeField]
	protected LinkEventData m_OnExit = new LinkEventData();

	public TextMeshProUGUI TextComponent
	{
		get
		{
			if (!m_TextComponent)
			{
				return m_TextComponent = GetComponent<TextMeshProUGUI>();
			}
			return m_TextComponent;
		}
	}

	public bool IsHover => m_CurrentEventData != null;

	public LinkEventData OnClick
	{
		get
		{
			return m_OnClick;
		}
		set
		{
			m_OnClick = value;
		}
	}

	public LinkEventData OnEnter
	{
		get
		{
			return m_OnEnter;
		}
		set
		{
			m_OnEnter = value;
		}
	}

	public LinkEventData OnHover
	{
		get
		{
			return m_OnHover;
		}
		set
		{
			m_OnHover = value;
		}
	}

	public LinkEventData OnExit
	{
		get
		{
			return m_OnExit;
		}
		set
		{
			m_OnExit = value;
		}
	}

	protected void OnEnable()
	{
		m_TextComponent = GetComponent<TextMeshProUGUI>();
	}

	protected void OnDisable()
	{
		if (m_HoverdLink >= 0)
		{
			CallEvent(m_OnExit, m_CurrentEventData, m_TextComponent.textInfo.linkInfo[m_HoverdLink]);
			m_HoverdLink = -1;
		}
		m_DownIndex = -1;
	}

	private void Reset()
	{
		TMP_CharacterInfo[] characterInfo = m_TextComponent.textInfo.characterInfo;
		for (int i = 0; i < characterInfo.Length; i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = characterInfo[i];
			for (int j = 0; j < 4; j++)
			{
				m_TextComponent.textInfo.meshInfo[tMP_CharacterInfo.materialReferenceIndex].colors32[tMP_CharacterInfo.vertexIndex + j] = Color.Lerp(tMP_CharacterInfo.color * Color.white, Color.white, 0f);
			}
		}
		m_TextComponent.UpdateVertexData((TMP_VertexDataUpdateFlags)17);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_Interactable)
		{
			m_DownIndex = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, eventData.position, eventData.enterEventCamera);
			if (m_DownIndex >= 0)
			{
				TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_DownIndex];
				DoTransnition(info, TransnitionState.Pressed);
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_Interactable && m_DownIndex >= 0)
		{
			TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_DownIndex];
			CallEvent(m_OnClick, eventData, info);
			m_DownIndex = -1;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_Interactable)
		{
			m_CurrentEventData = eventData;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_CurrentEventData = null;
		if (m_HoverdLink >= 0)
		{
			TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_HoverdLink];
			CallEvent(m_OnExit, eventData, info);
			DoTransnition(info, TransnitionState.Normal);
			m_HoverdLink = -1;
		}
	}

	public void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.TmpDisabled && !focus && m_HoverdLink >= 0)
		{
			TMP_LinkInfo info = m_TextComponent.textInfo.linkInfo[m_HoverdLink];
			CallEvent(m_OnExit, m_CurrentEventData, info);
			DoTransnition(info, TransnitionState.Normal);
			m_HoverdLink = -1;
		}
	}

	public void Update()
	{
		if (m_TextComponent == null)
		{
			return;
		}
		if (!m_Interactable)
		{
			if (m_CurrentEventData != null)
			{
				m_CurrentEventData = null;
			}
			return;
		}
		TMP_LinkInfo[] linkInfo = m_TextComponent.textInfo.linkInfo;
		if (!IsHover)
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, m_CurrentEventData.position, m_CurrentEventData.enterEventCamera);
		if (num != m_HoverdLink)
		{
			if (m_HoverdLink >= 0)
			{
				TMP_LinkInfo info = linkInfo[m_HoverdLink];
				CallEvent(m_OnExit, m_CurrentEventData, info);
				DoTransnition(info, TransnitionState.Normal);
			}
			m_HoverdLink = num;
			if (m_HoverdLink >= 0)
			{
				TMP_LinkInfo info2 = linkInfo[m_HoverdLink];
				CallEvent(m_OnEnter, m_CurrentEventData, info2);
				DoTransnition(info2, TransnitionState.Highlighted);
			}
		}
		if (m_HoverdLink >= 0)
		{
			TMP_LinkInfo info3 = linkInfo[m_HoverdLink];
			CallEvent(m_OnHover, m_CurrentEventData, info3);
		}
	}

	protected void DoTransnition(TMP_LinkInfo info, TransnitionState state)
	{
		if (m_ColorTransnition)
		{
			switch (state)
			{
			}
		}
	}

	public void ResetTransnition(bool isInit)
	{
		TMP_LinkInfo[] linkInfo = m_TextComponent.textInfo.linkInfo;
		if (linkInfo == null)
		{
			return;
		}
		Color color = (isInit ? m_NormalColor : Color.white);
		if (linkInfo.Length != 0)
		{
			for (int i = 0; i < linkInfo.Length; i++)
			{
				TMP_LinkInfo tMP_LinkInfo = linkInfo[i];
				TransnitionWord(tMP_LinkInfo.linkTextfirstCharacterIndex, tMP_LinkInfo.linkTextLength, color, m_OverlayNormalColor ? 1 : 0, underLine: false);
			}
		}
	}

	protected void TransnitionWord(int index, int lenght, Color color, float lerp, bool underLine)
	{
		TMP_CharacterInfo[] characterInfo = m_TextComponent.textInfo.characterInfo;
		if (characterInfo == null)
		{
			return;
		}
		for (int i = index; i < Mathf.Min(index + lenght, characterInfo.Length); i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = m_TextComponent.textInfo.characterInfo[i];
			if (tMP_CharacterInfo.isVisible)
			{
				for (int j = 0; j < 4; j++)
				{
					m_TextComponent.textInfo.meshInfo[tMP_CharacterInfo.materialReferenceIndex].colors32[tMP_CharacterInfo.vertexIndex + j] = Color.Lerp(tMP_CharacterInfo.color * color, color, lerp);
				}
			}
		}
		m_TextComponent.UpdateVertexData((TMP_VertexDataUpdateFlags)17);
	}

	protected void CallEvent(LinkEventData linkEvent, PointerEventData eventData, TMP_LinkInfo info)
	{
		linkEvent?.Invoke(eventData, info);
	}
}
