using System;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class StencilDebug
{
	public StencilDebugType StencilDebugType;

	[EnumFlags]
	public StencilRef Flags;

	[Range(0f, 255f)]
	public int Ref;
}
