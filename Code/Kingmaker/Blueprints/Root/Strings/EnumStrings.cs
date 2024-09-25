using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public abstract class EnumStrings<T> : StringsContainer
{
	public class Entry : IHashable
	{
		public T Value;

		public LocalizedString Text;

		public LocalizedString Description;

		public virtual Hash128 GetHash128()
		{
			return default(Hash128);
		}
	}

	[NonSerialized]
	[CanBeNull]
	private Dictionary<T, LocalizedString> m_Cache;

	protected abstract IEnumerable<Entry> GetEntries();

	protected E[] CreateEntries<E>() where E : Entry, new()
	{
		return (from T v in Enum.GetValues(typeof(T))
			select new E
			{
				Value = v,
				Text = null
			}).ToArray();
	}

	public bool Contains(T val)
	{
		if (m_Cache == null)
		{
			m_Cache = new Dictionary<T, LocalizedString>();
			GetEntries().ForEach(delegate(Entry e)
			{
				m_Cache[e.Value] = e.Text;
			});
		}
		return m_Cache.Get(val)?.IsSet() ?? false;
	}

	public string GetText(T val)
	{
		if (m_Cache == null)
		{
			m_Cache = new Dictionary<T, LocalizedString>();
			GetEntries().ForEach(delegate(Entry e)
			{
				m_Cache[e.Value] = e.Text;
			});
		}
		if (m_Cache.TryGetValue(val, out var value))
		{
			return value;
		}
		return val.ToString();
	}

	public string GetTextFlags(T val, string separator = ", ")
	{
		if (m_Cache == null)
		{
			m_Cache = new Dictionary<T, LocalizedString>();
			GetEntries().ForEach(delegate(Entry e)
			{
				m_Cache[e.Value] = e.Text;
			});
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = Convert.ToInt32(val);
		bool flag = false;
		for (int i = 0; i < 32; i++)
		{
			int num2 = 1 << i;
			if ((num & num2) != 0)
			{
				string text = GetText((T)(object)num2);
				if (flag)
				{
					stringBuilder.Append(separator);
				}
				stringBuilder.Append(text ?? "");
				flag = true;
			}
		}
		return stringBuilder.ToString();
	}
}
