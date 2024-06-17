using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.FoliageInteraction;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.Particles.Blueprints;
using Kingmaker.Visual.Trails;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class SnapToLocator : MonoBehaviour
{
	public string BoneName;

	public AnimationCurve CameraOffsetScale = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool DontScale;

	public bool DontRotate;

	[Tooltip("If disabled then the effect will apply to itself, rotate and scale locator in each frame. The position will be updated the same way if NoOffsetWhileAttached is off")]
	public bool DontAttach;

	[Tooltip("If disabled, the effect will follow the locator in each frame")]
	public bool NoOffsetWhileAttached;

	public bool DropToGround;

	public List<string> BoneNames = new List<string>();

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintFxLocatorGroup.Reference[] m_LocatorGroups;

	[NonSerialized]
	public float RaceScale = 1f;

	private FxBone m_Locator;

	private float m_LocalTime;

	private bool m_Initialized;

	private int m_IndexForExtraSnapping;

	public int IndexForExtraSnapping;

	public BlueprintFxLocatorGroup.Reference[] LocatorGroupsEditorRef
	{
		get
		{
			return m_LocatorGroups;
		}
		set
		{
			m_LocatorGroups = value;
		}
	}

	public FxBone Locator
	{
		get
		{
			return m_Locator;
		}
		set
		{
			m_Locator = value;
		}
	}

	public ReferenceArrayProxy<BlueprintFxLocatorGroup> LocatorGroups
	{
		get
		{
			BlueprintReference<BlueprintFxLocatorGroup>[] locatorGroups = m_LocatorGroups;
			return locatorGroups;
		}
	}

	public void Attach(SnapMapBase map, Transform defaultTarget)
	{
		if (map != null)
		{
			m_Locator = map.Bones.FirstOrDefault((FxBone b) => b.Name == BoneName);
			foreach (string boneName in BoneNames)
			{
				FxBone fxBone = map[boneName];
				if (fxBone != null)
				{
					m_Locator = fxBone;
					break;
				}
			}
			foreach (BlueprintFxLocatorGroup locatorGroup in LocatorGroups)
			{
				IReadOnlyList<FxBone> locators = map.GetLocators(locatorGroup);
				if (locators == null)
				{
					continue;
				}
				if (locators.Count > 1)
				{
					PFLog.TechArt.ErrorWithReport("SnapToLocator: multiple locators in group");
				}
				if (locators.Count != 0)
				{
					FxBone fxBone2 = locators[0];
					if (fxBone2 != null)
					{
						m_Locator = fxBone2;
						break;
					}
				}
			}
		}
		if (m_Locator != null && (bool)m_Locator.Transform)
		{
			if (!DontScale)
			{
				base.transform.localScale = m_Locator.Transform.lossyScale * RaceScale;
			}
			if (!DontRotate)
			{
				base.transform.rotation = m_Locator.Transform.rotation;
			}
			base.transform.position = m_Locator.Transform.position;
			if (!DontScale && !m_Initialized)
			{
				Light[] componentsInChildren = GetComponentsInChildren<Light>(includeInactive: true);
				float num = Mathf.Max(base.transform.localScale.x, Mathf.Max(base.transform.localScale.y, base.transform.localScale.z));
				num *= RaceScale;
				foreach (Light light in componentsInChildren)
				{
					if ((bool)light)
					{
						light.range *= num;
					}
				}
				FoliageInteractionEmitter[] componentsInChildren2 = GetComponentsInChildren<FoliageInteractionEmitter>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].SetLocatorScale(num);
				}
				CompositeTrailRenderer[] componentsInChildren3 = GetComponentsInChildren<CompositeTrailRenderer>();
				for (int j = 0; j < componentsInChildren3.Length; j++)
				{
					componentsInChildren3[j].Width *= num;
				}
				DissolveSetup[] componentsInChildren4 = GetComponentsInChildren<DissolveSetup>();
				for (int j = 0; j < componentsInChildren4.Length; j++)
				{
					componentsInChildren4[j].Settings.DissolveWidthScale *= num;
				}
				m_Initialized = true;
			}
			if (DropToGround && Physics.Linecast(base.transform.position + Vector3.up * 4f, base.transform.position + Vector3.down * 4f, out var hitInfo, 2359553))
			{
				base.transform.position = hitInfo.point;
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (m_Locator == null || m_Locator.Transform == null || DontAttach)
		{
			return;
		}
		float num = CameraOffsetScale.Evaluate(m_LocalTime += Game.Instance.TimeController.DeltaTime);
		Vector3 vector = m_Locator.Transform.TransformPoint(m_Locator.LocalOffset);
		Camera camera = Game.GetCamera();
		if (!(camera == null))
		{
			Vector3 vector2 = camera.transform.position - vector;
			if (!NoOffsetWhileAttached)
			{
				base.transform.position = vector + vector2.normalized * m_Locator.CameraOffset * num;
			}
			if (!DontScale)
			{
				base.transform.localScale = m_Locator.Transform.lossyScale * RaceScale;
			}
			if (!DontRotate)
			{
				base.transform.rotation = m_Locator.Transform.rotation;
			}
		}
	}

	private void OnDisable()
	{
		m_Locator = null;
	}
}
