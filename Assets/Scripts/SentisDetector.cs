using UnityEngine;
using Unity.Sentis;

public class SentisDetector : MonoBehaviour
{
    // Sentisモデルアセット
    [SerializeField] ModelAsset _sentisModelAsset;
    // 検出器への入力画像となるrender texture
    [SerializeField] RenderTexture _renderTexture;
    // ランタイムモデル
    Model _sentisRuntimeModel;
    // 推論エンジン
    IWorker _engine;

    void Start()
    {
        // ランタイムモデルと推論エンジンの初期化
        _sentisRuntimeModel = ModelLoader.Load(_sentisModelAsset);
        _engine = WorkerFactory.CreateWorker(BackendType.GPUCompute, _sentisRuntimeModel);
    }

    public void Execute()
    {
        // render textureを入力テンソルに変換
        TensorFloat inputTensor = TextureConverter.ToTensor(_renderTexture);
        // 推論の実行
        _engine.Execute(inputTensor);
        inputTensor.Dispose();
        // 出力テンソルの取得
        TensorFloat outputTensor = _engine.PeekOutput() as TensorFloat;
        outputTensor.MakeReadable();
        // テンソルデータをC#配列に変換
        float[] results = outputTensor.ToReadOnlyArray();
        outputTensor.Dispose();
        // 可視性判定結果：シグモイド関数の出力値の取得
        bool visible = results[0] >= 0.5f;
        Debug.Log($"Result: {visible},  {results[0]}");// {visible.ToString()}");
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
