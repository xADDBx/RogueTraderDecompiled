using System.Diagnostics;
using System.IO;
using System.Text;

namespace StateHasher.Core;

public static class HasherProvider
{
	public static class DebugLogger
	{
		private static readonly StringBuilder _sb = new StringBuilder(1024);

		[Conditional("STATE_HASHER_DEBUG")]
		public static void Clear()
		{
			_sb.Clear();
		}

		[Conditional("STATE_HASHER_DEBUG")]
		public static void BeginObject()
		{
			_sb.Append("{");
		}

		[Conditional("STATE_HASHER_DEBUG")]
		public static void AddName(string name)
		{
			_sb.Append("\"" + name + "\":");
		}

		[Conditional("STATE_HASHER_DEBUG")]
		public static void AddValue(string name, string value, bool last = false)
		{
			_sb.Append("\"" + name + "\":\"" + value + "\"" + (last ? "," : ""));
		}

		[Conditional("STATE_HASHER_DEBUG")]
		public static void EndObject()
		{
			_sb.Append("}");
		}

		[Conditional("STATE_HASHER_DEBUG")]
		public static void AppendLine(string line)
		{
			_sb.AppendLine(line);
		}

		[Conditional("STATE_HASHER_DEBUG")]
		public static void Flush(int id)
		{
			string text = Path.Combine("Library", "StateHasher", "Debug");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllText(Path.Combine(text, $"{id}.log"), _sb.ToString());
		}
	}

	private static IHasherLogger _logger;

	static HasherProvider()
	{
		RegisterDefault();
	}

	private static void RegisterDefault()
	{
	}

	public static void Initialize(IHasherLogger hasherLogger)
	{
		_logger = hasherLogger;
	}
}
