using System;
using System.Collections.Generic;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Code._TmpTechArt;

public class AddOffset : MonoBehaviour
{
	private AbstractUnitEntity m_CurrentUnit;

	private string m_Race;

	private string m_RaceOfCurrentAnimationClip;

	private Transform m_Obj;

	private AbstractUnitEntityView m_UnitEntityView;

	private float m_ActiveAnimationWeight;

	private IReadOnlyList<AnimationActionHandle> m_Actions;

	[SerializeField]
	[HideInInspector]
	private bool m_IsPc;

	private void Start()
	{
		m_UnitEntityView = GetComponent<AbstractUnitEntityView>();
		m_CurrentUnit = m_UnitEntityView.EntityData;
		if (base.gameObject.name.Contains("PC_", StringComparison.OrdinalIgnoreCase))
		{
			m_IsPc = true;
		}
		if (m_CurrentUnit != null)
		{
			m_Obj = m_CurrentUnit.View.ViewTransform.GetComponentInChildren<UnitAnimationManager>().transform;
		}
		DeterminateRace();
	}

	private void Update()
	{
		if ((bool)m_Obj && (bool)m_CurrentUnit.View)
		{
			m_Actions = m_CurrentUnit.AnimationManager.ActiveActions;
			if (m_Actions != null && m_Actions.Count > 0)
			{
				m_ActiveAnimationWeight = m_CurrentUnit.AnimationManager.CurrentAction.ActiveAnimation?.GetWeight() ?? 1f;
				DeterminateRaceOfCurrentAnimationClip();
				AddOffsetMethod();
			}
		}
	}

	private void DeterminateRace()
	{
		if (m_CurrentUnit != null && m_CurrentUnit.AnimationManager != null && m_CurrentUnit.AnimationManager.AnimationSet != null)
		{
			string text = m_CurrentUnit.AnimationManager.AnimationSet.name;
			if (text.Contains("human", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "human";
			}
			else if (text.Contains("eldar", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "eldar";
			}
			else if (text.Contains("spaceMarine", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "spaceMarine";
			}
		}
	}

	private void DeterminateRaceOfCurrentAnimationClip()
	{
		string text = "";
		foreach (AnimationActionHandle action in m_Actions)
		{
			if (action.ActiveAnimation != null)
			{
				text = action.ActiveAnimation.GetActiveClip().name;
			}
		}
		if (text.Contains("eldar", StringComparison.OrdinalIgnoreCase))
		{
			m_RaceOfCurrentAnimationClip = "eldar";
		}
		else if (text.Contains("spaceMarine", StringComparison.OrdinalIgnoreCase))
		{
			m_RaceOfCurrentAnimationClip = "spaceMarine";
		}
		else
		{
			m_RaceOfCurrentAnimationClip = "unknown";
		}
	}

	private void AddOffsetMethod()
	{
		string race = m_Race;
		string raceOfCurrentAnimationClip = m_RaceOfCurrentAnimationClip;
		if (!(race == "eldar"))
		{
			if (race == "spaceMarine" && raceOfCurrentAnimationClip == "unknown")
			{
				if (m_IsPc)
				{
					ControlPcIk(enable: false);
				}
				m_Obj.transform.localPosition = Vector3.zero + new Vector3(0f, 0.306f * m_ActiveAnimationWeight, 0f);
				return;
			}
		}
		else if (raceOfCurrentAnimationClip == "unknown")
		{
			if (m_IsPc)
			{
				ControlPcIk(enable: false);
			}
			m_Obj.transform.localPosition = Vector3.zero + new Vector3(0f, 0.161f * m_ActiveAnimationWeight, 0f);
			return;
		}
		if (m_Obj.transform.localPosition != Vector3.zero)
		{
			if (m_IsPc)
			{
				ControlPcIk(enable: true);
			}
			m_Obj.transform.localPosition = Vector3.zero;
		}
	}

	private void ControlPcIk(bool enable)
	{
		m_CurrentUnit.View.IkController.EnableIK = enable;
	}
}
