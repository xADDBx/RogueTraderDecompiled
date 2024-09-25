using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Kingmaker.Blueprints.Area;
using Kingmaker.Cheats;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine.SceneManagement;

namespace Kingmaker.QA.Arbiter;

public class ArbiterReporter
{
	private InstructionReportEntity m_LastCachedInstruction;

	private string m_ReportFilePath = Path.Combine(Arbiter.PlatformDataPath, "Arbiter/report.xml");

	private string m_ReportCachePath = Path.Combine(Arbiter.PlatformDataPath, "Arbiter/report_cache.json");

	private string m_ExternalLogId;

	private List<TestSuite> m_Testsuites = new List<TestSuite>();

	private ReporterCacheData m_CacheData;

	public bool HasInstructionsToSkip => m_LastCachedInstruction != null;

	public string PausedInstructionName => m_LastCachedInstruction?.Name;

	public bool PausedInstructionNeedsRetry
	{
		get
		{
			InstructionReportEntity lastCachedInstruction = m_LastCachedInstruction;
			if (lastCachedInstruction == null)
			{
				return false;
			}
			return lastCachedInstruction.Status == InstructionStatus.Started;
		}
	}

	public Guid CachedJobGuid => m_CacheData.JobGuid;

	private List<AbstractReportEntity> ReportEntities => m_CacheData?.ReportEntities;

	public ArbiterReporter(ArbiterStartupParameters parameters)
	{
		m_ReportFilePath = parameters.ArbiterReportPath ?? m_ReportFilePath;
		m_ReportCachePath = parameters.ArbiterReportCache ?? m_ReportCachePath;
		m_ExternalLogId = parameters.ArbiterExternalLogId ?? string.Empty;
		m_Testsuites.Add(new TestSuite
		{
			Name = "Instruction",
			Type = typeof(InstructionReportEntity)
		});
		if (parameters.ArbiterSceneReport != null)
		{
			m_Testsuites.Add(new TestSuite
			{
				Name = "Scenes",
				Type = typeof(SceneReportEntity)
			});
		}
		if (parameters.ArbiterAreaReport != null)
		{
			m_Testsuites.Add(new TestSuite
			{
				Name = "Areas",
				Type = typeof(AreaReportEntity)
			});
		}
		SceneManager.sceneLoaded += ReportSceneLoaded;
	}

	public void InitReportData(IEnumerable<string> instructions)
	{
		m_CacheData = ReadOrCreateReportCache();
		if (!ReportEntities.Empty())
		{
			foreach (AbstractReportEntity item in ReportEntities.Where(delegate(AbstractReportEntity entry)
			{
				InstructionReportEntity obj = entry as InstructionReportEntity;
				return obj != null && obj.Status == InstructionStatus.Restarted;
			}).ToList())
			{
				ReportInstructionSkipped((InstructionReportEntity)item);
			}
			return;
		}
		ReportEntities.AddRange(instructions.Select((string i) => new InstructionReportEntity
		{
			Name = i
		}));
		IEnumerable<AreaReportEntity> collection = from x in Utilities.GetBlueprintNames<BlueprintArea>()
			where !x.ToLower().Contains("test")
			select new AreaReportEntity
			{
				Name = x
			};
		ReportEntities.AddRange(collection);
		IEnumerable<SceneReportEntity> collection2 = from x in (from x in (from x in (from x in Utilities.GetBlueprintNames<BlueprintAreaPart>()
						where !x.ToLower().Contains("test")
						select Utilities.GetBlueprintByName<BlueprintAreaPart>(x)).NotNull()
					select new SceneReference[3] { x.MainStaticScene, x.LightScene, x.DynamicScene }).SelectMany((SceneReference[] x) => x).NotNull()
				where !Arbiter.Root.IsSceneIgnoredInReport(x)
				select x?.SceneName).Distinct()
			where !x.IsNullOrEmpty() && !x.ToLower().Contains("test")
			select new SceneReportEntity
			{
				Name = x
			};
		ReportEntities.AddRange(collection2);
		DumpReportCache();
	}

	private T GetOrCreateReportEntity<T>(string name) where T : AbstractReportEntity, new()
	{
		T val = ReportEntities.FirstOrDefault((AbstractReportEntity x) => x is T val2 && val2.Name == name) as T;
		if (val == null)
		{
			val = new T
			{
				Name = name
			};
			ReportEntities.Add(val);
		}
		val.ExternalLogId = m_ExternalLogId;
		return val;
	}

	private void ReportSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		GetOrCreateReportEntity<SceneReportEntity>(scene.name).IsStarted = true;
		DumpReport();
	}

	public void ReportInstructionStarted(string instruction)
	{
		InstructionReportEntity orCreateReportEntity = GetOrCreateReportEntity<InstructionReportEntity>(instruction);
		orCreateReportEntity.Status = ((orCreateReportEntity.Status != InstructionStatus.Started) ? InstructionStatus.Started : InstructionStatus.Restarted);
		string areaName = ArbiterUtils.GetAreaName(instruction);
		GetOrCreateReportEntity<AreaReportEntity>(areaName).IsStarted = true;
		DumpReport();
	}

	public void ReportInstructionPassed(string instruction, TimeSpan testTime)
	{
		InstructionReportEntity orCreateReportEntity = GetOrCreateReportEntity<InstructionReportEntity>(instruction);
		orCreateReportEntity.Status = InstructionStatus.Passed;
		orCreateReportEntity.TestTime = testTime;
		DumpReport();
	}

	public void ReportInstructionError(string instruction, string error, TimeSpan testTime)
	{
		InstructionReportEntity orCreateReportEntity = GetOrCreateReportEntity<InstructionReportEntity>(instruction);
		orCreateReportEntity.Error = error;
		orCreateReportEntity.Status = InstructionStatus.Error;
		orCreateReportEntity.TestTime = testTime;
		foreach (SceneReference scene in ArbiterUtils.GetScenes(instruction))
		{
			GetOrCreateReportEntity<SceneReportEntity>(scene.SceneName).IsFromFailedInstruction = true;
		}
		string areaName = ArbiterUtils.GetAreaName(instruction);
		GetOrCreateReportEntity<AreaReportEntity>(areaName).IsFromFailedInstruction = true;
		DumpReport();
	}

	public void ReportInstructionNotFound(string instruction, string error, TimeSpan testTime)
	{
		InstructionReportEntity orCreateReportEntity = GetOrCreateReportEntity<InstructionReportEntity>(instruction);
		orCreateReportEntity.Error = error;
		orCreateReportEntity.Status = InstructionStatus.Error;
		orCreateReportEntity.TestTime = testTime;
		DumpReport();
	}

	private void ReportInstructionSkipped(InstructionReportEntity instruction)
	{
		PFLog.Arbiter.Log("Skipped " + instruction.Name);
		instruction.Status = InstructionStatus.Skipped;
		foreach (SceneReference scene in ArbiterUtils.GetScenes(instruction.Name))
		{
			GetOrCreateReportEntity<SceneReportEntity>(scene.SceneName).IsFromSkippedInstruction = true;
		}
		string areaName = ArbiterUtils.GetAreaName(instruction.Name);
		GetOrCreateReportEntity<AreaReportEntity>(areaName).IsFromSkippedInstruction = true;
		DumpReport();
	}

	public void ReportInstructionSkipped(string instruction)
	{
		InstructionReportEntity orCreateReportEntity = GetOrCreateReportEntity<InstructionReportEntity>(instruction);
		ReportInstructionSkipped(orCreateReportEntity);
	}

	private void DumpReport()
	{
		DumpReportCache();
		if (m_Testsuites.Empty())
		{
			PFLog.Arbiter.Log("Nothing to dump in report file.");
			return;
		}
		if (!Directory.Exists(Path.GetDirectoryName(m_ReportFilePath)))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(m_ReportFilePath));
		}
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
		xmlWriterSettings.Indent = true;
		using (XmlWriter xmlWriter = XmlWriter.Create(m_ReportFilePath, xmlWriterSettings))
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("testsuites");
			foreach (TestSuite testsuite in m_Testsuites)
			{
				xmlWriter.WriteStartElement("testsuite");
				xmlWriter.WriteAttributeString("name", testsuite.Name);
				IEnumerable<AbstractReportEntity> entities = testsuite.GetEntities(ReportEntities);
				if (entities.FirstOrDefault() is AbstractAggregatedReportEntity)
				{
					new ReportEntityAggregator(entities).WriteTestcaseToXml(xmlWriter);
				}
				else
				{
					foreach (AbstractReportEntity item in entities.OrderBy((AbstractReportEntity x) => x.Name))
					{
						item.WriteTestCaseToXml(xmlWriter);
					}
				}
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			xmlWriter.Close();
		}
		PFLog.Arbiter.Log("Report file dumped at '" + m_ReportFilePath + "'");
	}

	private ReporterCacheData CreateReportCache()
	{
		return new ReporterCacheData();
	}

	private ReporterCacheData ReadOrCreateReportCache()
	{
		if (File.Exists(m_ReportCachePath))
		{
			try
			{
				ReporterCacheData reporterCacheData;
				using (StreamReader streamReader = new StreamReader(m_ReportCachePath))
				{
					reporterCacheData = ArbiterClientIntegration.DeserializeObject<ReporterCacheData>(streamReader.ReadToEnd());
					if (reporterCacheData.JobGuid == Guid.Empty)
					{
						reporterCacheData.JobGuid = Guid.NewGuid();
					}
				}
				PFLog.Arbiter.Error("Scene report cache readed from: " + m_ReportCachePath);
				return reporterCacheData;
			}
			catch (Exception ex)
			{
				PFLog.Arbiter.Error($"Failed to read scene report cache from {m_ReportCachePath}: {ex} \n {ex.StackTrace}");
			}
		}
		else
		{
			PFLog.Arbiter.Log("Scene report cache file not found '" + m_ReportCachePath + "'");
		}
		return CreateReportCache();
	}

	private void DumpReportCache()
	{
		try
		{
			if (!Directory.Exists(Path.GetDirectoryName(m_ReportCachePath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(m_ReportCachePath));
			}
			string text = m_ReportCachePath + "_tmp";
			using (StreamWriter streamWriter = new StreamWriter(text))
			{
				streamWriter.Write(ArbiterClientIntegration.SerializeObject(m_CacheData));
			}
			File.Copy(text, m_ReportCachePath, overwrite: true);
			File.Delete(text);
			PFLog.Arbiter.Log("Scene report file cache saved successfully at '" + m_ReportCachePath + "'");
		}
		catch (Exception ex)
		{
			PFLog.Arbiter.Error($"Failed to write scene report cache: {ex} \n {ex.StackTrace}");
		}
	}

	public void DeleteCache()
	{
		ReportEntities?.Clear();
		if (File.Exists(m_ReportCachePath))
		{
			File.Delete(m_ReportCachePath);
		}
		PFLog.Arbiter.Log("Scene report file cache deleted successfully");
	}

	public string GetStatus()
	{
		List<AbstractReportEntity> list = ReportEntities.Where((AbstractReportEntity x) => x is InstructionReportEntity).ToList();
		AbstractReportEntity abstractReportEntity = list.LastOrDefault((AbstractReportEntity x) => x is InstructionReportEntity instructionReportEntity && instructionReportEntity.Status != InstructionStatus.NotStarted);
		int num = list.IndexOf(abstractReportEntity);
		return $"Processing: [{num}/{list.Count}] {abstractReportEntity?.Name} ({Arbiter.StatusString})";
	}

	public IEnumerable<string> GetPendingInstructions()
	{
		InstructionStatus[] restartable = new InstructionStatus[2]
		{
			InstructionStatus.NotStarted,
			InstructionStatus.Started
		};
		return from instr in ReportEntities
			where instr is InstructionReportEntity && restartable.Contains(((InstructionReportEntity)instr).Status)
			select instr.Name;
	}
}
