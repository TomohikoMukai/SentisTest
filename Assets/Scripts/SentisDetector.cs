using UnityEngine;
using Unity.Sentis;

public class SentisDetector : MonoBehaviour
{
    [SerializeField] ModelAsset _sentisModelAsset;
    [SerializeField] RenderTexture _renderTexture;
    Model _sentisRuntimeModel;
    IWorker _engine;

    void Start()
    {
        _sentisRuntimeModel = ModelLoader.Load(_sentisModelAsset);
        _engine = WorkerFactory.CreateWorker(BackendType.GPUCompute, _sentisRuntimeModel);
    }

    public void Execute()
    {
        TensorFloat inputTensor = TextureConverter.ToTensor(_renderTexture);
        _engine.Execute(inputTensor);
        inputTensor.Dispose();
        TensorFloat outputTensor = _engine.PeekOutput() as TensorFloat;
        outputTensor.MakeReadable();
        float[] results = outputTensor.ToReadOnlyArray();
        outputTensor.Dispose();
        bool visible = results[0] >= 0.5f;
        Debug.Log($"Result: {visible.ToString()}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Execute();
        }
    }

    private void OnDestroy()
    {
        _engine.Dispose();
    }
}
