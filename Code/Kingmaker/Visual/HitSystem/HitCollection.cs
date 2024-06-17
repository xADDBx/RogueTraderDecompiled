using System;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class HitCollection
{
	public PrefabLink MinorLink;

	public PrefabLink StandardLink;

	public PrefabLink MajorLink;

	public PrefabLink CritLink;

	public GameObject Select(HitLevel level)
	{
		return level switch
		{
			HitLevel.Minor => MinorLink.Load(), 
			HitLevel.Standard => StandardLink.Load(), 
			HitLevel.Major => MajorLink.Load(), 
			HitLevel.Crit => CritLink.Load(), 
			_ => null, 
		};
	}

	public GameObject SelectSnap(HitLevel level)
	{
		if (level != HitLevel.Crit)
		{
			return null;
		}
		return CritLink.Load();
	}

	public void PreloadResources()
	{
		MinorLink.Preload();
		StandardLink.Preload();
		MajorLink.Preload();
		CritLink.Preload();
	}
}
