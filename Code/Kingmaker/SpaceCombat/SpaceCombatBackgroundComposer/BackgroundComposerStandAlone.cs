using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Blueprints;
using UnityEngine;

namespace Kingmaker.SpaceCombat.SpaceCombatBackgroundComposer;

public class BackgroundComposerStandAlone : MonoBehaviour
{
	public Material m_DefaultSkyDomeMaterial;

	public float m_SkyDomeScale = 300f;

	public float m_CommonFadeOverride = 0.3f;

	public BoxCollider m_FogOverrideCollider;

	private Material m_SkyDomeMaterial;

	private GameObject m_SkyDome;

	private void OnEnable()
	{
		GetSkyDomeMaterial();
		CreateSkyDome(m_SkyDomeScale);
	}

	private void OnDisable()
	{
		Object.Destroy(m_SkyDomeMaterial);
		Object.Destroy(m_SkyDome);
	}

	private void GetSkyDomeMaterial()
	{
		BlueprintStarSystemMap currentStarSystem = Game.Instance.Player.CurrentStarSystem;
		if (currentStarSystem != null)
		{
			m_SkyDomeMaterial = Object.Instantiate(currentStarSystem.BackgroundComposerConfig.SkyDomeMaterial.Load());
		}
		else if ((bool)m_DefaultSkyDomeMaterial)
		{
			m_SkyDomeMaterial = Object.Instantiate(m_DefaultSkyDomeMaterial);
		}
		else
		{
			PFLog.TechArt.Error("DefaultSkyDomeMaterial not found");
		}
		m_SkyDomeMaterial.SetFloat("_Common_fade", m_CommonFadeOverride);
	}

	public void CreateSkyDome(float skyDomeScale)
	{
		if (BlueprintRoot.Instance.SkyDome != null)
		{
			m_SkyDome = Object.Instantiate(BlueprintRoot.Instance.SkyDome, base.gameObject.transform);
			m_SkyDome.layer = base.gameObject.layer;
			m_FogOverrideCollider.gameObject.layer = base.gameObject.layer;
			m_SkyDome.transform.localScale = new Vector3(skyDomeScale, skyDomeScale, skyDomeScale);
			if ((bool)m_SkyDome && (bool)m_SkyDomeMaterial)
			{
				m_SkyDome.GetComponent<MeshRenderer>().material = m_SkyDomeMaterial;
			}
			else
			{
				PFLog.TechArt.Error("m_SkyDome or m_SkyDomeMaterial not founded");
			}
			float num = skyDomeScale * 2f;
			m_FogOverrideCollider.size = new Vector3(num, skyDomeScale, num);
			m_FogOverrideCollider.center = new Vector3(0f, skyDomeScale / 2f * -1f, 0f);
		}
		else
		{
			PFLog.TechArt.Error("SkyDome prefab in BlueprintRoot not founded");
		}
	}
}
