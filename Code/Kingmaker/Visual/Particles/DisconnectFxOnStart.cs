using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class DisconnectFxOnStart : MonoBehaviour
{
	private void Start()
	{
		base.transform.SetParent(FxHelper.FxRoot, worldPositionStays: true);
	}
}
