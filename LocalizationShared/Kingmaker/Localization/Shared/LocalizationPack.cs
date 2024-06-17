using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Localization.Enums;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;

namespace Kingmaker.Localization.Shared;

[JsonObject(MemberSerialization.OptIn)]
public class LocalizationPack
{
	private struct StringEntry
	{
		public uint Offset;

		[JsonProperty]
		public string Text;
	}

	[UsedImplicitly]
	private class EntryConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(StringEntry);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string text = (string)reader.Value;
			StringEntry stringEntry = default(StringEntry);
			stringEntry.Text = text;
			return stringEntry;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((StringEntry)value).Text);
		}
	}

	[NotNull]
	[JsonProperty(PropertyName = "strings")]
	private Dictionary<string, StringEntry> m_Strings = new Dictionary<string, StringEntry>();

	private FileStream m_PackFile;

	private readonly object m_Lock = new object();

	public Locale Locale { get; set; }

	[NotNull]
	public string GetText([NotNull] string key, bool reportUnknown = true)
	{
		TryGetText(key, out var text, reportUnknown);
		return text;
	}

	public bool TryGetText([NotNull] string key, out string text, bool reportUnknown = true)
	{
		lock (m_Lock)
		{
			if (m_Strings.TryGetValue(key, out var value))
			{
				if (value.Text == null && value.Offset != 0 && m_PackFile != null)
				{
					m_PackFile.Seek(value.Offset, SeekOrigin.Begin);
					using (BinaryReader binaryReader = new BinaryReader(m_PackFile, Encoding.UTF8, leaveOpen: true))
					{
						value.Text = binaryReader.ReadString();
					}
					m_Strings[key] = value;
				}
				text = value.Text ?? "";
				return value.Text != null;
			}
		}
		text = (reportUnknown ? ("[unknown key: " + key + "]") : "");
		return false;
	}

	public void AddStrings([CanBeNull] LocalizationPack pack)
	{
		if (pack == null)
		{
			return;
		}
		if (pack.Locale != Locale)
		{
			PFLog.Default.Warning($"Adding strings from locale '{pack.Locale}' to LocalizationPack with locale '{Locale}'");
		}
		foreach (var (key, value) in pack.m_Strings)
		{
			m_Strings[key] = value;
		}
	}

	public void PutString(string key, string val)
	{
		m_Strings[key] = new StringEntry
		{
			Text = val
		};
	}

	public void InitFromBinary(string path)
	{
		lock (m_Lock)
		{
			m_PackFile = new FileStream(path, FileMode.Open, FileAccess.Read);
			using BinaryReader binaryReader = new BinaryReader(m_PackFile, Encoding.UTF8, leaveOpen: true);
			using (CodeTimer.New("Loading LOC-TOC"))
			{
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					string key = binaryReader.ReadString();
					uint offset = binaryReader.ReadUInt32();
					m_Strings[key] = new StringEntry
					{
						Offset = offset
					};
				}
			}
		}
	}

	public void Dispose()
	{
		lock (m_Lock)
		{
			m_PackFile?.Close();
		}
	}
}
