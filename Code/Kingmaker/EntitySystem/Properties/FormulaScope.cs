using System;
using System.Text;

namespace Kingmaker.EntitySystem.Properties;

public static class FormulaScope
{
	public struct Entry : IDisposable
	{
		private bool m_Use;

		public Entry(bool use)
		{
			m_Use = use;
		}

		public void Dispose()
		{
			if (m_Use)
			{
				Leave();
			}
		}
	}

	private static int m_Indent;

	private static readonly string[] m_Indents = new string[16]
	{
		"", "\t", "\t\t", "\t\t\t", "\t\t\t\t", "\t\t\t\t\t", "\t\t\t\t\t\t", "\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t",
		"\t\t\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t\t\t\t\t\t", "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t"
	};

	public static string Indent
	{
		get
		{
			if (m_Indent < m_Indents.Length)
			{
				return m_Indents[m_Indent];
			}
			return new string('\t', m_Indent);
		}
	}

	public static Entry Enter(bool use)
	{
		if (use)
		{
			m_Indent++;
		}
		return new Entry(use);
	}

	public static void Leave()
	{
		m_Indent--;
		if (m_Indent < 0)
		{
			PFLog.Default.Error($"FormulaScope m_Indent < 0! {m_Indent}");
			m_Indent = 0;
		}
	}

	public static StringBuilder AppendIndentedFormula(this StringBuilder sb, string text)
	{
		sb.Append(Indent);
		sb.Append(text);
		return sb;
	}

	public static StringBuilder AppendIndentedFormula(this StringBuilder sb, char text)
	{
		sb.Append(Indent);
		sb.Append(text);
		return sb;
	}
}
