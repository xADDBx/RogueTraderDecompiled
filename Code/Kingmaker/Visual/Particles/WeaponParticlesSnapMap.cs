using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Visual.Trails;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[DisallowMultipleComponent]
public class WeaponParticlesSnapMap : SnapMapBase
{
	public enum WeaponSlot
	{
		Unknown = -1,
		PrimaryHand,
		SecondaryHand,
		Additional1,
		Additional2,
		Additional3,
		Additional4,
		Additional5,
		Additional6,
		Additional7,
		Additional8
	}

	public WeaponSlot Slot;

	[Header("Trails")]
	public GameObject DefaultTrailPrefab;

	public Gradient ColorGradient;

	public CompositeTrailRenderer.TrailAlignment Alignment;

	[SerializeField]
	private List<TrailBonesPair> m_TrailBones = new List<TrailBonesPair>();

	private readonly List<CompositeTrailRenderer> m_ActiveTrails = new List<CompositeTrailRenderer>();

	[CanBeNull]
	private GameObject m_DefaultTrailInstance;

	public List<TrailBonesPair> TrailBones => m_TrailBones;

	protected override void OnInitialize()
	{
		foreach (TrailBonesPair trailBone in TrailBones)
		{
			trailBone.Start.Transform = base.transform.FindChildRecursive(trailBone.Start.Name);
			trailBone.End.Transform = base.transform.FindChildRecursive(trailBone.End.Name);
		}
	}

	private void Start()
	{
		if (!base.Initialized)
		{
			Init();
		}
		if (DefaultTrailPrefab != null)
		{
			m_DefaultTrailInstance = FxHelper.SpawnFxOnGameObject(DefaultTrailPrefab, base.gameObject);
		}
	}

	private void OnEnable()
	{
		m_DefaultTrailInstance.Or(null)?.SetActive(value: true);
	}

	private void OnDisable()
	{
		m_DefaultTrailInstance.Or(null)?.SetActive(value: false);
	}

	public void AddTrail(CompositeTrailRenderer trail)
	{
		if (!m_ActiveTrails.Contains(trail))
		{
			m_ActiveTrails.Add(trail);
		}
	}

	public void RemoveTrail(CompositeTrailRenderer trail)
	{
		m_ActiveTrails.Remove(trail);
	}

	public void StartTrail(float time)
	{
		if (m_ActiveTrails.Count <= 0)
		{
			return;
		}
		foreach (CompositeTrailRenderer activeTrail in m_ActiveTrails)
		{
			activeTrail.SetEmittersEnabled(emit: true);
			activeTrail.Lifetime = time;
		}
		StopAllCoroutines();
		StartCoroutine(AnimateTrail(time));
	}

	private IEnumerator AnimateTrail(float time)
	{
		float start = Time.time;
		while (Time.time - start < time)
		{
			foreach (CompositeTrailRenderer activeTrail in m_ActiveTrails)
			{
				activeTrail.MultiplyColor = ColorGradient.Evaluate((Time.time - start) / time);
			}
			yield return null;
		}
		foreach (CompositeTrailRenderer activeTrail2 in m_ActiveTrails)
		{
			activeTrail2.SetEmittersEnabled(emit: false);
		}
	}
}
