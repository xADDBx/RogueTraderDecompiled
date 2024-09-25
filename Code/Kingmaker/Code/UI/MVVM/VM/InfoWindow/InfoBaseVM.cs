using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.InfoWindow;

public abstract class InfoBaseVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private TooltipBaseTemplate m_MainTemplate;

	public readonly List<TooltipBaseBrickVM> HeaderBricks = new List<TooltipBaseBrickVM>();

	public readonly List<TooltipBaseBrickVM> BodyBricks = new List<TooltipBaseBrickVM>();

	public readonly List<TooltipBaseBrickVM> FooterBricks = new List<TooltipBaseBrickVM>();

	public readonly List<TooltipBaseBrickVM> HintBricks = new List<TooltipBaseBrickVM>();

	public TooltipBaseTemplate MainTemplate => m_MainTemplate;

	public IEnumerable<TooltipBaseTemplate> Templates { get; private set; }

	protected abstract TooltipTemplateType TemplateType { get; }

	public bool IsPrimitive
	{
		get
		{
			if (HeaderBricks.Count == 1 && BodyBricks.Empty())
			{
				return FooterBricks.Empty();
			}
			return false;
		}
	}

	public float ContentSpacing => m_MainTemplate.ContentSpacing;

	protected InfoBaseVM(TooltipData data)
	{
		if (data is CombinedTooltipData combinedTooltipData)
		{
			InitWithTemplates(combinedTooltipData.Templates);
		}
		else
		{
			NewTemplate(data.MainTemplate);
		}
	}

	protected InfoBaseVM(TooltipBaseTemplate template)
	{
		NewTemplate(template);
	}

	protected InfoBaseVM(IEnumerable<TooltipBaseTemplate> templates)
	{
		InitWithTemplates(templates);
	}

	public void SetNewTemplate(TooltipBaseTemplate template)
	{
		HeaderBricks.Clear();
		BodyBricks.Clear();
		FooterBricks.Clear();
		HintBricks.Clear();
		NewTemplate(template);
	}

	private void NewTemplate(TooltipBaseTemplate template)
	{
		m_MainTemplate = template;
		try
		{
			template.Prepare(TemplateType);
			CollectBricks(template.GetHeader(TemplateType), HeaderBricks);
			CollectBricks(template.GetBody(TemplateType), BodyBricks);
			CollectBricks(template.GetFooter(TemplateType), FooterBricks);
			CollectBricks(template.GetHint(TemplateType), HintBricks);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create tooltip template: {arg}");
		}
	}

	private static void CollectBricks(IEnumerable<ITooltipBrick> bricks, List<TooltipBaseBrickVM> bricksList)
	{
		foreach (ITooltipBrick brick in bricks)
		{
			if (brick != null)
			{
				TooltipBaseBrickVM vM = brick.GetVM();
				bricksList.Add(vM);
			}
		}
	}

	private void InitWithTemplates(IEnumerable<TooltipBaseTemplate> templates)
	{
		Templates = templates;
		m_MainTemplate = templates.FirstOrDefault();
		foreach (TooltipBaseTemplate template in templates)
		{
			template.Prepare(TemplateType);
		}
		bool flag = true;
		foreach (TooltipBaseTemplate template2 in templates)
		{
			foreach (ITooltipBrick item in template2.GetHeader(TemplateType))
			{
				if (item != null)
				{
					if (flag)
					{
						HeaderBricks.Add(item.GetVM());
					}
					else
					{
						BodyBricks.Add(item.GetVM());
					}
				}
			}
			CollectBricks(template2.GetBody(TemplateType), BodyBricks);
			flag = false;
		}
		CollectBricks(m_MainTemplate.GetFooter(TemplateType), FooterBricks);
		CollectBricks(m_MainTemplate.GetHint(TemplateType), HintBricks);
	}

	protected override void DisposeImplementation()
	{
		DisposeBricks(HeaderBricks);
		DisposeBricks(BodyBricks);
		DisposeBricks(FooterBricks);
		DisposeBricks(HintBricks);
	}

	private static void DisposeBricks(List<TooltipBaseBrickVM> bricksList)
	{
		bricksList.ForEach(delegate(TooltipBaseBrickVM b)
		{
			b.Dispose();
		});
		bricksList.Clear();
	}
}
