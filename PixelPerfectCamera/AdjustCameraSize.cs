using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class AdjustCameraSize : MonoBehaviour
{
    [SerializeField]
    private Vector2Int _nativeResolution = Vector2Int.zero;

    [SerializeField]
    private int _pixelsPerUnit = 32;

    [SerializeField]
    private Camera _gameCamera = null;

    [SerializeField]
    private Camera _targetCamera = null;

    [SerializeField]
    private MeshRenderer _targetQuad = null;

    private RenderTexture _renderTexture = null;
    private RenderTextureDescriptor _descriptor
    {
        get => new RenderTextureDescriptor()
        {
            width = _nativeResolution.x,
            height = _nativeResolution.y,
            msaaSamples = 1,
            useMipMap = false,
            useDynamicScale = false,
            volumeDepth = 1,
            dimension = TextureDimension.Tex2D,
            graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm
        };
    }

    public void AdjustCameraObjects()
    {
        SetupRenderTexture();

        _targetQuad.transform.localScale = new Vector3(_nativeResolution.x / 100.0f, _nativeResolution.y / 100.0f, 1.0f);
        _targetCamera.orthographicSize = _targetQuad.transform.localScale.y / 2.0f;
        _gameCamera.orthographicSize = (_nativeResolution.y / _pixelsPerUnit) / 2.0f;
    }

    private void Awake()
    {
        if (_renderTexture == null)
        {
            SetupRenderTexture();
        }
    }

    private void SetupRenderTexture()
    {
        _renderTexture = new RenderTexture(_descriptor);
        _renderTexture.filterMode = FilterMode.Point;
        _targetQuad.material.SetTexture("_MainTex", _renderTexture);
        _gameCamera.targetTexture = _renderTexture;
    }

}
