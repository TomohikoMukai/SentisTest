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
    Worker _engine;

    void Start()
    {
        // ランタイムモデルと推論エンジンの初期化
        _sentisRuntimeModel = ModelLoader.Load(_sentisModelAsset);
        _engine = new Worker(_sentisRuntimeModel, BackendType.GPUPixel);
    }

    public void Inference()
    {
        // render textureを入力テンソルに変換
        using Tensor inputTensor = TextureConverter.ToTensor(
            _renderTexture,
            width:64,
            height:64,
            channels:4);
        // 推論の実行
        _engine.Schedule(inputTensor);
        inputTensor.Dispose();
        // 出力テンソルの取得
        Tensor<float> outputTensor = _engine.PeekOutput() as Tensor<float>;
        // テンソルデータをC#配列に変換
        float[] results = outputTensor.DownloadToArray();
        outputTensor.Dispose();
        // 可視性判定結果：シグモイド関数の出力値の取得
        bool visible = results[0] >= 0.5f;
        Debug.Log($"Result: {visible},  {results[0]}");// {visible.ToString()}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Inference();
        }
    }

    private void OnDestroy()
    {
        _engine.Dispose();
    }
}
