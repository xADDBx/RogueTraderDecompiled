using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Modding;

[Serializable]
public class MaterialsShaderData
{
	[SerializeField]
	public Dictionary<string, string> Data = new Dictionary<string, string>();
}
