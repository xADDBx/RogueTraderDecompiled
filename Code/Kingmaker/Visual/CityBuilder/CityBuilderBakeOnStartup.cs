using System.Collections.Generic;
using System.Linq;
using Kingmaker.Visual.Decals;
using UnityEngine;

namespace Kingmaker.Visual.CityBuilder;

public class CityBuilderBakeOnStartup : MonoBehaviour
{
	public CityBuilderTerrain Terrain;

	private List<FxDecal> m_Decals = new List<FxDecal>();

	private void Start()
	{
		if (!Terrain)
		{
			PFLog.Default.Log("No terrain. Abort decals bake.");
		}
		else
		{
			m_Decals = ScreenSpaceDecal.All.OfType<FxDecal>().ToList();
		}
	}

	private void Update()
	{
		if (m_Decals.Count <= 0)
		{
			return;
		}
		foreach (FxDecal decal in m_Decals)
		{
			Terrain.BakeDecal(decal);
			decal.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: false);
	}
}
