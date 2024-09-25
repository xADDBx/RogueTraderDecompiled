using System;
using System.Text.RegularExpressions;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools.Core;

public class TextTemplateEngineProxy
{
	private static TextTemplateEngineProxy s_Instance;

	private static BaseTextTemplateEngine s_TextTemplateEngine;

	public static TextTemplateEngineProxy Instance => s_Instance ?? (s_Instance = new TextTemplateEngineProxy());

	static TextTemplateEngineProxy()
	{
	}

	public void Initialize(BaseTextTemplateEngine textTemplateEngine)
	{
		s_TextTemplateEngine = textTemplateEngine;
	}

	public string Process(string text)
	{
		if (s_TextTemplateEngine == null)
		{
			PFLog.Default.Error("Trying to use BaseTextTemplateEngine before calling Initialize()");
			throw new NullReferenceException("Trying to use BaseTextTemplateEngine before calling Initialize()");
		}
		return s_TextTemplateEngine.Process(text);
	}

	public TextTemplate GetTemplate(Match match, out string tag, out string[] parameters, out bool capitalized)
	{
		if (s_TextTemplateEngine == null)
		{
			PFLog.Default.Error("Trying to use BaseTextTemplateEngine before calling Initialize()");
			throw new NullReferenceException("Trying to use BaseTextTemplateEngine before calling Initialize()");
		}
		return s_TextTemplateEngine.GetTemplate(match, out tag, out parameters, out capitalized);
	}
}
