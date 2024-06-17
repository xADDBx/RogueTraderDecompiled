using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI.Common.UIConfigComponents;

public abstract class EnumSpritesBlueprint<T> : BlueprintScriptableObject
{
	public class Entry : IHashable
	{
		public T Value;

		public Sprite Sprite;

		public virtual Hash128 GetHash128()
		{
			return default(Hash128);
		}
	}

	[SerializeField]
	private Sprite m_DefaultSprite;

	[NonSerialized]
	[CanBeNull]
	private Dictionary<T, Sprite> m_Cache;

	protected abstract IEnumerable<Entry> GetEntries();

	protected E[] CreateEntries<E>() where E : Entry, new()
	{
		return (from T v in Enum.GetValues(typeof(T))
			select new E
			{
				Value = v,
				Sprite = null
			}).ToArray();
	}

	public bool Contains(T val)
	{
		if (m_Cache == null)
		{
			m_Cache = new Dictionary<T, Sprite>();
			GetEntries().ForEach(delegate(Entry e)
			{
				m_Cache[e.Value] = e.Sprite;
			});
		}
		return m_Cache.Get(val) != null;
	}

	public Sprite GetSprite(T val)
	{
		if (m_Cache == null)
		{
			m_Cache = new Dictionary<T, Sprite>();
			GetEntries().ForEach(delegate(Entry e)
			{
				m_Cache[e.Value] = e.Sprite;
			});
		}
		if (m_Cache.TryGetValue(val, out var value))
		{
			return value;
		}
		return m_DefaultSprite;
	}
}
