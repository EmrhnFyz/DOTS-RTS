using UnityEngine;

public class FoWDiscovered : MonoBehaviour
{
	[SerializeField] private RenderTexture fowRenderTexture;
	[SerializeField] private RenderTexture fowUndiscoveredRenderTexture;
	[SerializeField] private RenderTexture fowUndiscoveredRenderTexture_Cached;

	[SerializeField] private Material fowDiscoveredMaterial;

	private bool _isInitialized;

	private int _updateCount;

	private void Update()
	{
		if (!_isInitialized)
		{
			return;
		}

		Graphics.Blit(fowRenderTexture, fowUndiscoveredRenderTexture, fowDiscoveredMaterial, 0);
		Graphics.CopyTexture(fowUndiscoveredRenderTexture, fowUndiscoveredRenderTexture_Cached);
	}

	private void LateUpdate()
	{
		if (_isInitialized)
		{
			return;
		}

		_updateCount++;

		if (_updateCount < 0)
		{
			return;
		}

		_isInitialized = true;

		Graphics.Blit(fowRenderTexture, fowUndiscoveredRenderTexture);
		Graphics.Blit(fowRenderTexture, fowUndiscoveredRenderTexture_Cached);
	}
}