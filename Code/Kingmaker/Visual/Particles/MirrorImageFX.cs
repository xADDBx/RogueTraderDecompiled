using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class MirrorImageFX : MonoBehaviour
{
	[Serializable]
	public class MirrorImageEntry
	{
		public int ImageIndex;

		public GameObject MainFxPrefab;

		public GameObject DeathFxPrefab;

		public Vector3 Offset;

		[NonSerialized]
		public GameObject MainFxInstance;
	}

	private const int MaxImages = 8;

	[SerializeField]
	private List<MirrorImageEntry> m_Entries;

	private UnitPartMirrorImage m_MirrorImage;

	private UnitEntityView m_Unit;

	private readonly List<MirrorImageEntry> m_ActiveEntries = new List<MirrorImageEntry>();

	public void Init(UnitEntityView unit)
	{
		m_Unit = unit;
		m_MirrorImage = unit.EntityData.GetOptional<UnitPartMirrorImage>();
		if (m_MirrorImage == null || m_MirrorImage.VisualImages.Count <= 0)
		{
			base.enabled = false;
			FxHelper.Destroy(base.gameObject);
			return;
		}
		m_MirrorImage.Fx = this;
		List<MirrorImageEntry> list = new List<MirrorImageEntry>();
		list.AddRange(m_Entries);
		m_ActiveEntries.Clear();
		foreach (int visualImage in m_MirrorImage.VisualImages)
		{
			if (list.Count <= 0)
			{
				break;
			}
			int index = PFStatefulRandom.Visuals.Particles.Range(0, list.Count - 1);
			MirrorImageEntry mirrorImageEntry = list[index];
			mirrorImageEntry.ImageIndex = visualImage;
			m_ActiveEntries.Add(mirrorImageEntry);
			list.RemoveAt(index);
		}
		foreach (MirrorImageEntry activeEntry in m_ActiveEntries)
		{
			activeEntry.MainFxInstance = FxHelper.SpawnFxOnEntity(activeEntry.MainFxPrefab, unit);
		}
	}

	private void OnDisable()
	{
		DestroyImages();
	}

	private void OnDestroy()
	{
		DestroyImages();
	}

	private void DestroyImages()
	{
		while (m_ActiveEntries.Count > 0)
		{
			MirrorImageEntry mirrorImageEntry = m_ActiveEntries[0];
			m_ActiveEntries.RemoveAt(0);
			FxHelper.SpawnFxOnEntity(mirrorImageEntry.DeathFxPrefab, m_Unit);
			FxHelper.Destroy(mirrorImageEntry.MainFxInstance);
			mirrorImageEntry.MainFxInstance = null;
		}
		m_Unit = null;
	}

	public void DestroyImage(int imageIndex)
	{
		MirrorImageEntry mirrorImageEntry = m_ActiveEntries.FirstOrDefault((MirrorImageEntry e) => e.ImageIndex == imageIndex);
		if (mirrorImageEntry != null)
		{
			m_ActiveEntries.Remove(mirrorImageEntry);
			FxHelper.SpawnFxOnEntity(mirrorImageEntry.DeathFxPrefab, m_Unit);
			FxHelper.Destroy(mirrorImageEntry.MainFxInstance);
			mirrorImageEntry.MainFxInstance = null;
			if (m_ActiveEntries.Count <= 0)
			{
				FxHelper.Destroy(base.gameObject);
			}
		}
	}

	public MirrorImageEntry GetImage(int imageIndex)
	{
		return m_ActiveEntries.FirstOrDefault((MirrorImageEntry e) => e.ImageIndex == imageIndex);
	}

	private void Update()
	{
		foreach (MirrorImageEntry activeEntry in m_ActiveEntries)
		{
			if (activeEntry.MainFxInstance == null)
			{
				activeEntry.MainFxInstance = FxHelper.SpawnFxOnEntity(activeEntry.MainFxPrefab, m_Unit);
			}
			ParticlesSnapController componentInChildren = activeEntry.MainFxInstance.GetComponentInChildren<ParticlesSnapController>();
			if ((bool)componentInChildren)
			{
				Renderer[] componentsInChildren = activeEntry.MainFxInstance.GetComponentsInChildren<Renderer>();
				foreach (Renderer obj in componentsInChildren)
				{
					Vector2 vector = CameraRig.Instance.WorldToViewport(m_Unit.ViewTransform.position);
					Vector2 vector2 = CameraRig.Instance.WorldToViewport(m_Unit.ViewTransform.position + componentInChildren.CurrentOffset);
					Material material = obj.material;
					material.SetVector(ShaderProps._DistortionOffset, vector - vector2);
					obj.material = material;
				}
			}
		}
		if (m_MirrorImage == null || m_MirrorImage.VisualImages.Count <= 0)
		{
			FxHelper.Destroy(base.gameObject);
		}
	}
}
