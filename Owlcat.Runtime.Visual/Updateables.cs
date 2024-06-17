using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Highlighting;

public static class Updateables
{
	public static readonly List<Type> IUpdateables = new List<Type>
	{
		typeof(Highlighter),
		typeof(MultiHighlighter)
	};

	public static readonly List<Type> ILateUpdateables = new List<Type>();
}
