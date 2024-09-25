using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kingmaker.EntitySystem.Properties;

public static class FormulaTargetScope
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Entry : IDisposable
	{
		public Entry(PropertyTargetType type, bool colorize)
		{
			if (type == PropertyTargetType.CurrentEntity && m_TargetStack.Count > 0)
			{
				type = m_TargetStack.Peek().Target;
			}
			m_TargetStack.Push((type, colorize));
		}

		public void Dispose()
		{
			m_TargetStack.Pop();
		}
	}

	private static Stack<(PropertyTargetType Target, bool Colorize)> m_TargetStack = new Stack<(PropertyTargetType, bool)>();

	public static string Current
	{
		get
		{
			if (!m_TargetStack.TryPeek(out (PropertyTargetType, bool) result))
			{
				return "(???)";
			}
			if (result.Item2)
			{
				return result.Item1.Colorized();
			}
			return result.Item1.ToString();
		}
	}

	public static PropertyTargetType CurrentTarget
	{
		get
		{
			if (!m_TargetStack.TryPeek(out (PropertyTargetType, bool) result))
			{
				return PropertyTargetType.CurrentEntity;
			}
			return result.Item1;
		}
	}

	public static bool NeedColorization
	{
		get
		{
			if (m_TargetStack.Count == 0)
			{
				return false;
			}
			return m_TargetStack.Peek().Colorize;
		}
	}

	public static Entry Enter(PropertyTargetType type, bool colorize)
	{
		return new Entry(type, colorize);
	}
}
