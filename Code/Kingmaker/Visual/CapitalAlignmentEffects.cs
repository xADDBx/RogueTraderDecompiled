using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual;

[RequireComponent(typeof(PostProcessSettings))]
public class CapitalAlignmentEffects : MonoBehaviour
{
	[SerializeField]
	private List<CapitalAligmentPostProcess> m_CapitalPPList;

	public void SetCurrentPP()
	{
		_ = Game.Instance.TimeOfDay;
		PostProcessSettings component = GetComponent<PostProcessSettings>();
		_ = null != component;
	}
}
