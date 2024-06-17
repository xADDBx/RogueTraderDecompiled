using System.Collections.Generic;
using Kingmaker.Visual.Trails;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class WeaponParticlesSnapController : SnapControllerBase
{
	private CompositeTrailRenderer m_Trail;

	private WeaponParticlesSnapMap m_WeaponMap;

	private List<TrailBonesPair> m_BonesPairs;

	[Header("Trails")]
	public List<TrailBonesNamesPair> TrailBonesNames;

	public bool ReceiveAnimationEvents;

	public bool Smooth;

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_WeaponMap != null)
		{
			m_WeaponMap.RemoveTrail(m_Trail);
		}
		m_WeaponMap = null;
	}

	protected override void OnStartPlaying(SnapMapBase snapMap)
	{
		base.OnStartPlaying(snapMap);
		m_Trail = GetComponent<CompositeTrailRenderer>();
		if (m_Trail == null)
		{
			return;
		}
		m_WeaponMap = snapMap as WeaponParticlesSnapMap;
		if (!(m_WeaponMap != null) || m_WeaponMap.Alignment != m_Trail.Alignment)
		{
			return;
		}
		m_BonesPairs = new List<TrailBonesPair>();
		foreach (TrailBonesPair trailBone in m_WeaponMap.TrailBones)
		{
			foreach (TrailBonesNamesPair trailBonesName in TrailBonesNames)
			{
				if (trailBone.Start.Name == trailBonesName.Start && trailBone.End.Name == trailBonesName.End)
				{
					m_BonesPairs.Add(trailBone);
					break;
				}
			}
		}
		m_Trail.Emitters.Clear();
		foreach (TrailBonesPair bonesPair in m_BonesPairs)
		{
			TrailEmitter trailEmitter = new TrailEmitter();
			m_Trail.Emitters.Add(trailEmitter);
			if (bonesPair.Start != null && bonesPair.Start.Transform != null)
			{
				trailEmitter.Spawner = bonesPair.Start.Transform.gameObject;
			}
			if (bonesPair.End != null && bonesPair.End.Transform != null)
			{
				trailEmitter.SecondSpawner = bonesPair.End.Transform.gameObject;
			}
			trailEmitter.Smooth = Smooth;
		}
		if (ReceiveAnimationEvents)
		{
			m_Trail.SetEmittersEnabled(emit: false);
			m_WeaponMap.AddTrail(m_Trail);
		}
	}
}
