using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

public class ProjectileView : MonoBehaviour, IResource
{
	public GameObject[] DestroyOnHit;

	private void OnEnable()
	{
		DestroyOnHit.ForEach(delegate(GameObject go)
		{
			if ((bool)go)
			{
				go.EnsureComponent<FxDestroyOnStop>();
			}
		});
	}
}
