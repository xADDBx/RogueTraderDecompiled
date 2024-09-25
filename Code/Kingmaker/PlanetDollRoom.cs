using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Visual.SceneHelpers;
using UnityEngine;

namespace Kingmaker;

public class PlanetDollRoom : DollRoomBase
{
	private GameObject m_SimpleAvatar;

	public void SetupPlanet(GameObject planet)
	{
		Cleanup();
		CreateSimpleAvatar(planet);
	}

	private void CreateSimpleAvatar(GameObject planet)
	{
		MeshRenderer componentInChildren = planet.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("Can't find MeshRenderer component in {0}", planet.name);
			return;
		}
		m_SimpleAvatar = Object.Instantiate(componentInChildren.transform.parent.gameObject, m_TargetPlaceholder);
		Object.Destroy(m_SimpleAvatar.GetComponent<TransformFreezer>());
		m_SimpleAvatar.hideFlags = HideFlags.None;
		m_SimpleAvatar.transform.position = m_TargetPlaceholder.position;
		m_SimpleAvatar.transform.rotation = m_TargetPlaceholder.rotation;
		m_SimpleAvatar.transform.localScale = m_TargetPlaceholder.localScale;
		ChangeLayer(m_SimpleAvatar);
		Material material = m_SimpleAvatar.GetComponentInChildren<MeshRenderer>().material;
		if (material != null)
		{
			material.SetInt("_Rim_light", 0);
			material.SetInt("_IsPlanetDollRoom", 1);
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (!(m_SimpleAvatar == null))
		{
			Object.Destroy(m_SimpleAvatar);
		}
	}

	private void ChangeLayer(GameObject avatar)
	{
		foreach (Transform item in avatar.transform)
		{
			item.gameObject.layer = 15;
		}
	}
}
