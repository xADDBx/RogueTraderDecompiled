using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.BundlesLoading;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.Blueprints.JsonSystem;

public class BlueprintsCache
{
	private struct BlueprintCacheEntry
	{
		public uint Offset;

		public SimpleBlueprint Blueprint;
	}

	[NotNull]
	private readonly Dictionary<string, BlueprintCacheEntry> m_LoadedBlueprints = new Dictionary<string, BlueprintCacheEntry>();

	private FileStream m_PackFile;

	private ReflectionBasedSerializer m_PackSerializer;

	private IResourceReplacementProvider m_resourceReplacementProvider;

	private object m_Lock = new object();

	public void Init(IResourceReplacementProvider resourceReplacementProvider)
	{
		m_resourceReplacementProvider = resourceReplacementProvider;
		string path = BundlesLoadService.BundlesPath("blueprints-pack.bbp");
		m_PackFile = new FileStream(path, FileMode.Open, FileAccess.Read);
		byte[] array = new byte[16];
		using (BinaryReader binaryReader = new BinaryReader(m_PackFile, Encoding.UTF8, leaveOpen: true))
		{
			using (CodeTimer.New("Loading TOC"))
			{
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					binaryReader.Read(array, 0, 16);
					string key = new Guid(array).ToString("N");
					uint offset = binaryReader.ReadUInt32();
					m_LoadedBlueprints.Add(key, new BlueprintCacheEntry
					{
						Offset = offset
					});
				}
			}
		}
		m_PackSerializer = new ReflectionBasedSerializer(new PrimitiveSerializer(new BinaryReader(m_PackFile), UnityObjectConverter.AssetList));
	}

	public SimpleBlueprint Load(string guid)
	{
		lock (m_Lock)
		{
			if (m_LoadedBlueprints.TryGetValue(guid, out var value))
			{
				if (value.Blueprint == null)
				{
					if (value.Offset == 0)
					{
						return null;
					}
					m_PackFile.Seek(value.Offset, SeekOrigin.Begin);
					SimpleBlueprint bp = null;
					m_PackSerializer.Blueprint(ref bp);
					if (bp == null)
					{
						return null;
					}
					object obj = m_resourceReplacementProvider?.OnResourceLoaded(bp, guid);
					if (obj != null)
					{
						bp = (obj as SimpleBlueprint) ?? bp;
					}
					value.Blueprint = bp;
					value.Blueprint.OnEnable();
					m_LoadedBlueprints[guid] = value;
				}
				return value.Blueprint;
			}
		}
		return null;
	}

	public void AddCachedBlueprint(string guid, SimpleBlueprint bp)
	{
		m_LoadedBlueprints[guid] = new BlueprintCacheEntry
		{
			Blueprint = bp
		};
	}

	public void RemoveCachedBlueprint(string guid)
	{
		m_LoadedBlueprints.Remove(guid);
	}
}
