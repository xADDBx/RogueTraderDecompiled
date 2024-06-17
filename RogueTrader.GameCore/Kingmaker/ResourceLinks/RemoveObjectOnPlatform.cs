using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

public class RemoveObjectOnPlatform : MonoBehaviour
{
	[InfoBox("If true this object and all children will be remove on resource loading from bundle on Consoles")]
	public bool RemoveOnConsole;

	[InfoBox("If true this object and all children will be remove on resource loading from bundle on PC")]
	public bool RemoveOnPC;
}
