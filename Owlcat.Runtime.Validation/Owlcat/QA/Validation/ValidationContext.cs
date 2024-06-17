using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Owlcat.QA.Validation;

public class ValidationContext : IDisposable
{
	private readonly List<ValidationError> m_Errors = new List<ValidationError>();

	private readonly List<Action<object>> m_Fixups = new List<Action<object>>();

	private readonly ValidationTree m_Tree;

	private ValidationTreeNode m_CurrentNode;

	private readonly TimeSpan m_GCInterval = TimeSpan.FromSeconds(5.0);

	private readonly TimeSpan m_YieldInterval = TimeSpan.FromMilliseconds(5.0);

	private DateTime m_LastGc = DateTime.MinValue;

	private DateTime m_YieldPeriod;

	private CancellationToken m_Token;

	public bool ValidateEverything;

	public bool InterruptSlowValidations;

	private readonly int m_Progress;

	private readonly ValidationStack<IValidated> m_ValidationStack = new ValidationStack<IValidated>();

	public IEnumerable<string> Errors => m_Errors.Select((ValidationError error) => error.Message).ToList();

	public IEnumerable<ValidationError> ErrorsAdvanced => m_Errors;

	public List<ValidationError> RawErrors => m_Errors;

	public bool HasErrors => m_Errors.Count > 0;

	public List<Action<object>> RawFixups => m_Fixups;

	public bool HasFixups => m_Fixups.Count > 0;

	public IEnumerable<string> ErrorsForHook => (from error in m_Errors.FindAll((ValidationError x) => x.IsShownInHook)
		select error.Message + "\n\tValidation Author: " + error.Owner).ToList();

	public string AssetPath { get; private set; }

	public string AssetId { get; private set; }

	public bool IsSceneObject { get; private set; }

	public bool IsValidated { get; private set; }

	public int CurrentIndex => m_CurrentNode.Index;

	public string ObjectName => m_CurrentNode.Name;

	public string ObjectPath => m_CurrentNode.ToString();

	public string FullPath => AssetPath + "/" + ObjectPath;

	public string FilePath
	{
		get
		{
			if (string.IsNullOrEmpty(AssetPath) || !File.Exists(AssetPath))
			{
				return string.Empty;
			}
			return new FileInfo(AssetPath).FullName;
		}
	}

	public string FormattedValidationStack => m_ValidationStack.FormatValidationStack();

	public bool HasCircularDependencies => m_ValidationStack.HasCircularDependencies;

	[Obsolete("Use AddError with ErrorLevel and responsible or with ValidationType and responsible")]
	[StringFormatMethod("errorFormat")]
	public void AddError(string errorFormat, params object[] args)
	{
		AddError(ErrorLevel.Unprioritized, "Unknown", errorFormat, args);
	}

	[Obsolete("Use AddError with ErrorLevel and responsible or with ValidationType and responsible")]
	[StringFormatMethod("errorFormat")]
	public void AddError(string designer, string errorFormat, params object[] args)
	{
		AddError(ErrorLevel.Unprioritized, designer, errorFormat, args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(ErrorLevel level, string errorFormat, params object[] args)
	{
		AddError(level, "Unknown", errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(ErrorLevel level, Action<MetadataCollector> meta, string errorFormat, params object[] args)
	{
		AddError(level, "Unknown", meta, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(ErrorLevel level, string designers, string errorFormat, params object[] args)
	{
		AddErrorInternal(level, designers, "Unknown", isShownInHook: true, null, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(ErrorLevel level, string designers, Action<MetadataCollector> meta, string errorFormat, params object[] args)
	{
		AddErrorInternal(level, designers, "Unknown", isShownInHook: true, meta, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(string type, string designers, string errorFormat, params object[] args)
	{
		AddErrorInternal(ErrorLevel.Unprioritized, designers, type, isShownInHook: true, null, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(string type, string designers, Action<MetadataCollector> meta, string errorFormat, params object[] args)
	{
		AddErrorInternal(ErrorLevel.Unprioritized, designers, type, isShownInHook: true, meta, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(ErrorLevel level, string type, string designers, string errorFormat, params object[] args)
	{
		AddErrorInternal(level, designers, type, isShownInHook: true, null, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddError(ErrorLevel level, string type, string designers, Action<MetadataCollector> meta, string errorFormat, params object[] args)
	{
		AddErrorInternal(level, designers, type, isShownInHook: true, meta, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	private void AddErrorInternal(ErrorLevel level, string designers, string type, bool isShownInHook, Action<MetadataCollector> meta, string errorFormat, params object[] args)
	{
		string text = m_CurrentNode.ToString();
		int num = text.IndexOf(".", StringComparison.Ordinal);
		text = ((num < 0) ? text : text.Substring(num + 1));
		string message = errorFormat;
		if (args.Length != 0)
		{
			message = string.Format(errorFormat, args);
		}
		MetadataCollector metadataCollector = new MetadataCollector(meta);
		m_Errors.Add(new ValidationError(level, isShownInHook, text, type, m_CurrentNode.IsActive, message, designers, metadataCollector.Metadata));
	}

	public void AddFixup(Action<object> fixup)
	{
		m_Fixups.Add(fixup);
	}

	[StringFormatMethod("errorFormat")]
	public void AddErrorHiddenInHook(ErrorLevel level, string designers, string errorFormat, params object[] args)
	{
		AddErrorInternal(level, designers, "Unknown", isShownInHook: false, null, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddErrorHiddenInHook(string designers, string errorFormat, params object[] args)
	{
		AddErrorHiddenInHook(ErrorLevel.Unprioritized, designers, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	[StringFormatMethod("errorFormat")]
	public void AddErrorHiddenInHook(string type, string designers, string errorFormat, params object[] args)
	{
		AddErrorInternal(ErrorLevel.Unprioritized, designers, type, isShownInHook: false, null, errorFormat + "." + m_ValidationStack.FormatValidationStack(), args);
	}

	public ValidationContext(string objectName, bool isSceneObject = false, bool validateEverything = true, CancellationToken token = default(CancellationToken))
	{
		ValidateEverything = validateEverything;
		IsSceneObject = isSceneObject;
		m_Token = token;
		m_Tree = new ValidationTree();
		m_Progress = ValidationProgressCollection.StartNew("Validating " + objectName);
		m_CurrentNode = new ValidationTreeNode(m_Tree, -1, -1, "", ValidationNodeType.Object, active: true);
	}

	public void SetAssetData(string path, string guid)
	{
		if (!string.IsNullOrEmpty(path) && AssetPath != path)
		{
			AssetPath = path;
		}
		if (!string.IsNullOrEmpty(guid) && AssetId != guid)
		{
			AssetId = guid;
		}
	}

	public ValidationContext CreateChild(string name, ValidationNodeType type, int parent = -1, bool active = true)
	{
		m_CurrentNode = ((parent >= 0) ? m_Tree[parent].CreateChild(name, type, active) : m_CurrentNode.CreateChild(name, type, active));
		Step();
		return this;
	}

	public int CreateChildIndex(string name, ValidationNodeType type, int parent = -1, bool active = true)
	{
		CreateChild(name, type, parent, active);
		return CurrentIndex;
	}

	public async Task StepAsync()
	{
		m_YieldPeriod = await YieldPeriodic(m_YieldPeriod, m_Token);
	}

	public void Clear()
	{
		AssetPath = null;
		AssetId = null;
		m_Errors.Clear();
		m_ValidationStack.Clear();
		m_Fixups.Clear();
	}

	public void Copy(ValidationContext vc, bool withoutErrors = false)
	{
		m_Tree.Copy(vc.m_Tree);
		m_CurrentNode = m_Tree[vc.CurrentIndex];
		if (!withoutErrors)
		{
			m_Errors.Clear();
			m_Errors.AddRange(vc.m_Errors);
			m_Fixups.Clear();
			m_Fixups.AddRange(vc.m_Fixups);
		}
	}

	public void Dispose()
	{
		Finish();
		Clear();
	}

	private void GCPeriodic()
	{
		if (!(m_LastGc - DateTime.UtcNow < m_GCInterval))
		{
			GC.Collect();
			m_LastGc = DateTime.UtcNow;
		}
	}

	private async Task<DateTime> YieldPeriodic(DateTime prev, CancellationToken ct)
	{
		GCPeriodic();
		if (DateTime.UtcNow - prev < m_YieldInterval)
		{
			return prev;
		}
		await Task.Yield();
		if (ct.CanBeCanceled)
		{
			ct.ThrowIfCancellationRequested();
		}
		return DateTime.UtcNow;
	}

	public void SetTotalProgress(int total)
	{
		ValidationProgressCollection.SetTotal(m_Progress, total);
	}

	private void Step()
	{
		if (ValidationProgressCollection.IsExists(m_Progress))
		{
			ValidationProgressCollection.Step(m_Progress, m_CurrentNode.Name);
		}
	}

	public void Finish(ValidationResult result = ValidationResult.Success)
	{
		ValidationProgressCollection.Stop(m_Progress, result);
		if (result == ValidationResult.Success)
		{
			IsValidated = true;
		}
	}

	public void RegisterValueChangedCallBack(Action callback)
	{
		ValidationProgressCollection.RegisterValueChangedCallBack(m_Progress, callback);
	}

	public void RegisterStopedCallBack(Action<ValidationResult> callback)
	{
		ValidationProgressCollection.RegisterStopedCallBack(m_Progress, callback);
	}

	public void UnregisterValueChangedCallBacks()
	{
		ValidationProgressCollection.UnregisterValueChangedCallBacks(m_Progress);
	}

	public void UnregisterStopedCallBacks()
	{
		ValidationProgressCollection.UnregisterStopedCallBacks(m_Progress);
	}

	public T FindElementOnValidationStack<T>() where T : class, IValidated
	{
		return m_ValidationStack.FindElementOfType<T>();
	}

	public IDisposable PrepareNestedValidation<T>(T obj) where T : class
	{
		if (obj is IValidated obj2)
		{
			m_ValidationStack.Push(obj2);
		}
		return m_ValidationStack;
	}
}
