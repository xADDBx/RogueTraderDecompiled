using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.DxtCompressor;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterAtlasService : IService
{
	private struct AtlasRebuildRequest
	{
		public List<CharacterAtlas> Atlases;

		public Material Material;

		public Action<CharacterAtlas, Texture2D> OnTextureCompressed;

		public Action<CharacterAtlas> OnTextureNotCompressed;

		public string ContextString;

		public int CurrentAtlasIndex;
	}

	private List<AtlasRebuildRequest> m_Requests = new List<AtlasRebuildRequest>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public int RequestsCount => m_Requests.Count;

	public void QueueAtlasRebuild(List<CharacterAtlas> atlases, Material material, Action<CharacterAtlas, Texture2D> onTextureCompressed, Action<CharacterAtlas> onTextureNotCompressed, string contextString)
	{
		if (m_Requests.FindIndex((AtlasRebuildRequest r) => r.Material == material) != -1)
		{
			for (int i = 0; i < atlases.Count; i++)
			{
				onTextureNotCompressed?.Invoke(atlases[i]);
			}
			return;
		}
		if (material == null)
		{
			PFLog.Default.Error("CharacterAtlasService.QueueAtlasRebuild: material is null for " + contextString);
			for (int j = 0; j < atlases.Count; j++)
			{
				onTextureNotCompressed?.Invoke(atlases[j]);
			}
			return;
		}
		AtlasRebuildRequest atlasRebuildRequest = default(AtlasRebuildRequest);
		atlasRebuildRequest.Atlases = new List<CharacterAtlas>(atlases);
		atlasRebuildRequest.Material = material;
		atlasRebuildRequest.OnTextureCompressed = onTextureCompressed;
		atlasRebuildRequest.OnTextureNotCompressed = onTextureNotCompressed;
		atlasRebuildRequest.ContextString = contextString;
		AtlasRebuildRequest item = atlasRebuildRequest;
		m_Requests.Add(item);
		Update();
	}

	public void Update()
	{
		DxtCompressorService instance = Services.GetInstance<DxtCompressorService>();
		if (instance == null)
		{
			return;
		}
		instance.Update();
		if (m_Requests.Count == 0)
		{
			return;
		}
		AtlasRebuildRequest request = m_Requests[0];
		if (instance.RequestsCount > 0)
		{
			return;
		}
		m_Requests.RemoveAt(0);
		if (request.Material == null)
		{
			PFLog.Default.Error("CharacterAtlasService.Update: Material is null in request for " + request.ContextString);
			foreach (CharacterAtlas atlase in request.Atlases)
			{
				atlase.ClearTempValues();
				request.OnTextureNotCompressed?.Invoke(atlase);
			}
			request.Atlases.Clear();
		}
		else if (request.Atlases.Count != 0)
		{
			request.Atlases[0].CompressAsync(instance, delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
	}

	private void OnOneAtlasCompressed(AtlasRebuildRequest request, CharacterAtlas atlas, Texture2D texture)
	{
		DxtCompressorService instance = Services.GetInstance<DxtCompressorService>();
		if (instance == null)
		{
			return;
		}
		request.OnTextureCompressed(atlas, texture);
		if (request.CurrentAtlasIndex < request.Atlases.Count - 1)
		{
			CharacterAtlas characterAtlas = request.Atlases[++request.CurrentAtlasIndex];
			if (characterAtlas.Destroyed)
			{
				ClearRequestBecauseAtlasDestroyed(request);
				return;
			}
			characterAtlas.CompressAsync(instance, delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
		else
		{
			request.Atlases.Clear();
		}
	}

	private void OnOneAtlasNotCompressed(AtlasRebuildRequest request, CharacterAtlas atlas)
	{
		DxtCompressorService instance = Services.GetInstance<DxtCompressorService>();
		if (instance == null)
		{
			return;
		}
		request.OnTextureNotCompressed(atlas);
		if (request.CurrentAtlasIndex < request.Atlases.Count - 1)
		{
			CharacterAtlas characterAtlas = request.Atlases[++request.CurrentAtlasIndex];
			if (characterAtlas.Destroyed)
			{
				ClearRequestBecauseAtlasDestroyed(request);
				return;
			}
			characterAtlas.CompressAsync(instance, delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
		else
		{
			request.Atlases.Clear();
		}
	}

	private void ClearRequestBecauseAtlasDestroyed(AtlasRebuildRequest request)
	{
		foreach (CharacterAtlas atlase in request.Atlases)
		{
			atlase.ClearTempValues();
		}
		request.Atlases.Clear();
	}
}
