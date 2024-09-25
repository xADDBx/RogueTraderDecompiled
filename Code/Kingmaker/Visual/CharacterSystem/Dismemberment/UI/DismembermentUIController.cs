using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentUIController : MonoBehaviour
{
	[Flags]
	private enum UIState
	{
		Empty = 0,
		CharacterLoaded = 1,
		SetSelected = 2,
		SetSliced = 4,
		SetPrefabLoaded = 8,
		Preview = 0x10,
		HasSliceBones = 0x20
	}

	public Button ButtonLoad;

	public Button ButtonAddSet;

	public Button ButtonAddBone;

	public Button ButtonSlice;

	public Button ButtonSave;

	public Button ButtonPreview;

	public Button ButtonReset;

	private UIState m_State;

	private void OnEnable()
	{
		UpdateState();
	}

	public void OnLoad()
	{
		m_State = UIState.CharacterLoaded;
		UpdateState();
	}

	public void OnSetSelectedChanged(bool selected, bool hasBones)
	{
		if (m_State.HasFlag(UIState.CharacterLoaded))
		{
			if (selected)
			{
				m_State = UIState.CharacterLoaded | UIState.SetSelected;
				if (hasBones)
				{
					m_State |= UIState.HasSliceBones;
				}
			}
			else
			{
				m_State = UIState.CharacterLoaded;
			}
		}
		UpdateState();
	}

	public void OnSliceBoneChanged(bool hasBones)
	{
		if (m_State.HasFlag(UIState.SetSelected))
		{
			if (hasBones)
			{
				m_State |= UIState.HasSliceBones;
			}
			else
			{
				m_State &= ~UIState.HasSliceBones;
			}
		}
		UpdateState();
	}

	public void OnSlice()
	{
		m_State |= UIState.SetSliced;
		UpdateState();
	}

	internal void OnSetPrefabLoaded()
	{
		m_State |= UIState.SetPrefabLoaded;
		m_State &= ~UIState.SetSliced;
		UpdateState();
	}

	internal void OnPreview()
	{
		m_State |= UIState.Preview;
		UpdateState();
	}

	private void UpdateState()
	{
		ButtonAddSet.interactable = m_State.HasFlag(UIState.CharacterLoaded);
		ButtonAddBone.interactable = m_State.HasFlag(UIState.SetSelected);
		ButtonSlice.interactable = m_State.HasFlag(UIState.SetSelected) && m_State.HasFlag(UIState.HasSliceBones);
		ButtonSave.interactable = m_State.HasFlag(UIState.SetSelected) && m_State.HasFlag(UIState.SetSliced);
		ButtonPreview.interactable = m_State.HasFlag(UIState.SetPrefabLoaded);
		ButtonReset.interactable = m_State.HasFlag(UIState.SetPrefabLoaded) && m_State.HasFlag(UIState.Preview);
	}
}
