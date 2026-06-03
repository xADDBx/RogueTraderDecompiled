using Kingmaker.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker;

public class NintendoTest : MonoBehaviour
{
	public TextMeshProUGUI DebugText;

	private void Update()
	{
		_ = ApplicationHelper.IsRunningOnSwitch2;
	}
}
