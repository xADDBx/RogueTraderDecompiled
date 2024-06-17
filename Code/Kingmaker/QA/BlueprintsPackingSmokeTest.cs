using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.BinaryFormat;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.CodeTimer;
using RogueTrader.SharedTypes;
using UnityEngine;

namespace Kingmaker.QA;

public class BlueprintsPackingSmokeTest : MonoBehaviour
{
	private struct BlueprintCacheEntry
	{
		public string Guid;

		public uint Offset;

		public SimpleBlueprint Blueprint;
	}

	[NotNull]
	private readonly List<BlueprintCacheEntry> m_LoadedBlueprints = new List<BlueprintCacheEntry>();

	private FileStream m_PackFile;

	private ReflectionBasedSerializer m_PackSerializer;

	private string m_Id;

	private void Start()
	{
		LoggingConfiguration.Configure();
		GuidClassBinder.StartWarmingUp();
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(30f, 30f, 300f, 50f), "Run"))
		{
			Run();
			Application.Quit();
		}
		m_Id = GUI.TextField(new Rect(30f, 90f, 300f, 18f), m_Id);
		if (int.TryParse(m_Id, out var result) && GUI.Button(new Rect(30f, 130f, 300f, 25f), "test " + result))
		{
			Run(result);
			Application.Quit();
		}
	}

	public void Run(int checkid = -1)
	{
		BlueprintReferencedAssets referencedAssets = ScriptableObject.CreateInstance<BlueprintReferencedAssets>();
		string path = "blueprints-pack.bbp";
		using (m_PackFile = new FileStream(path, FileMode.Open, FileAccess.Read))
		{
			byte[] array = new byte[16];
			using (BinaryReader binaryReader = new BinaryReader(m_PackFile, Encoding.UTF8, leaveOpen: true))
			{
				using (CodeTimer.New("Loading TOC"))
				{
					int num = binaryReader.ReadInt32();
					for (int i = 0; i < num; i++)
					{
						binaryReader.Read(array, 0, 16);
						string guid = new Guid(array).ToString("N");
						uint offset = binaryReader.ReadUInt32();
						m_LoadedBlueprints.Add(new BlueprintCacheEntry
						{
							Guid = guid,
							Offset = offset
						});
					}
				}
			}
			PFLog.Default.Log($"Loaded TOC: {m_LoadedBlueprints.Count} entries");
			m_PackSerializer = new ReflectionBasedSerializer(new PrimitiveSerializer(new BinaryReader(m_PackFile), referencedAssets));
			using StreamWriter streamWriter = new StreamWriter("result");
			if (checkid < 0)
			{
				for (int j = 0; j < m_LoadedBlueprints.Count; j++)
				{
					BlueprintCacheEntry blueprintCacheEntry = m_LoadedBlueprints[j];
					if (blueprintCacheEntry.Offset != 0)
					{
						try
						{
							Debug.Log("Reading " + blueprintCacheEntry.Guid);
							streamWriter.WriteLine(blueprintCacheEntry.Guid + " " + j);
							SimpleBlueprint bp = null;
							m_PackFile.Seek(blueprintCacheEntry.Offset, SeekOrigin.Begin);
							m_PackSerializer.Blueprint(ref bp);
						}
						catch (Exception ex)
						{
							streamWriter.WriteLine(ex.ToString());
							Debug.LogError($"When loading blueprint ({blueprintCacheEntry.Guid}) with offset {blueprintCacheEntry.Offset}");
							throw;
						}
					}
				}
				return;
			}
			BlueprintCacheEntry blueprintCacheEntry2 = m_LoadedBlueprints[checkid];
			try
			{
				Debug.Log("Reading " + blueprintCacheEntry2.Guid);
				streamWriter.WriteLine(blueprintCacheEntry2.Guid + " " + checkid);
				SimpleBlueprint bp2 = null;
				m_PackFile.Seek(blueprintCacheEntry2.Offset, SeekOrigin.Begin);
				m_PackSerializer.Blueprint(ref bp2);
			}
			catch (Exception ex2)
			{
				streamWriter.WriteLine(ex2.ToString());
				Debug.LogError($"When loading blueprint ({blueprintCacheEntry2.Guid}) with offset {blueprintCacheEntry2.Offset}");
			}
		}
	}
}
