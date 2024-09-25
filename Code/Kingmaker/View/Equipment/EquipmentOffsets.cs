using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.Equipment;

[RequireComponent(typeof(OccludedObjectHighlighterBlockerHierarchy))]
[RequireComponent(typeof(HighlighterBlockerHierarchy))]
public class EquipmentOffsets : UpdateableBehaviour
{
	[Serializable]
	public class Offsets
	{
		public Vector3 Position;

		public Vector3 Rotation;

		public void Apply(Transform t)
		{
			t.localRotation = Quaternion.Euler(Rotation);
			t.localPosition = Vector3.Scale(Position, t.localScale);
		}
	}

	[Serializable]
	public class RaceOffset
	{
		public Race race;

		public Vector3 OffsetLeftIk;
	}

	[Serializable]
	public class RaceScale
	{
		public Race race;

		public float WeaponScale;
	}

	[SerializeField]
	[FormerlySerializedAs("m_InHand")]
	public Offsets m_MainHand;

	[SerializeField]
	[FormerlySerializedAs("m_OffHandInHand")]
	public Offsets m_OffHand;

	[SerializeField]
	public Offsets[] m_SlotOffsets;

	public bool BackpackOffsets;

	public Transform JointsParent;

	public Transform IkTargetLeftHand;

	public Transform IkTargetRightHand;

	public List<RaceOffset> raceOffset = new List<RaceOffset>();

	public List<RaceScale> raceScaleList = new List<RaceScale>();

	private float m_LossyScale;

	private byte DefaultRenderingLayer = 2;

	public void Apply(UnitEquipmentVisualSlotType slot, bool isOffHand, Character character, Transform t = null)
	{
		Offsets offsets = GetOffsets(slot, isOffHand);
		if (offsets != null)
		{
			t = t ?? base.transform;
			offsets.Apply(t);
			m_LossyScale = base.transform.lossyScale.x;
		}
	}

	public Offsets GetOffsets(UnitEquipmentVisualSlotType slot, bool isOffHand)
	{
		if (slot != 0)
		{
			return m_SlotOffsets.ElementAtOrDefault((int)slot);
		}
		if (!isOffHand)
		{
			return m_MainHand;
		}
		return m_OffHand;
	}

	public override void DoUpdate()
	{
		RigidbodyWeaponRescaleJoints();
	}

	private void RigidbodyWeaponRescaleJoints()
	{
		if (!(JointsParent == null) && !(JointsParent.GetComponentInChildren<CharacterJoint>() == null))
		{
			if (Math.Abs(m_LossyScale - base.transform.lossyScale.x) > 0f)
			{
				ReScale();
			}
			m_LossyScale = base.transform.lossyScale.x;
		}
	}

	private void ReScale()
	{
		CharacterJoint[] componentsInChildren = JointsParent.GetComponentsInChildren<CharacterJoint>();
		foreach (CharacterJoint characterJoint in componentsInChildren)
		{
			if (characterJoint.anchor.x > 0f)
			{
				characterJoint.anchor = new Vector3(-0.001f, 0f, 0f);
			}
			else
			{
				characterJoint.anchor = new Vector3(0.001f, 0f, 0f);
			}
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		Character componentInParent = GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			MeshRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].renderingLayerMask = componentInParent.CurrentLayer;
			}
		}
		else
		{
			MeshRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].renderingLayerMask = DefaultRenderingLayer;
			}
		}
	}
}
