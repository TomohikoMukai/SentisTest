using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

public class TrainingDataGenerator : MonoBehaviour
{
    [SerializeField] GameObject _target;
    [SerializeField] RenderTexture _renderTexture;
    [SerializeField] int _numExamples;
    List<byte[]> _images = null;
    List<int> _visibility = null;
    Texture2D _tex = null;

    void Start()
    {
        _tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
        _images = new List<byte[]>();
        _visibility = new List<int>();
    }
    void Update()
    {
        if (Time.time < 1.0f)
        {
            return;
        }
        if (_images.Count < _numExamples || Input.GetKeyDown(KeyCode.Space))
        {
            TryAddData();

            Vector3 p;
            p.x = UnityEngine.Random.Range(-12.0f, 12.0f);
            p.y = UnityEngine.Random.Range(-12.0f, 12.0f);
            p.z = UnityEngine.Random.Range(-7.0f, -13.0f);
            transform.position = p;
        }
    }

    float VisiblityRatio()
    {
        Mesh mesh = _target.GetComponent<MeshFilter>().mesh;
        Transform trs = _target.GetComponent<Transform>();
        int numVisibleVertices = 0;
        foreach (var v in mesh.vertices)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(trs.TransformPoint(v));
            if (vp.x > 0 && vp.x < 1
             && vp.y > 0 && vp.y < 1)
            {
                ++numVisibleVertices;
            }
        }
        if (numVisibleVertices <= 0)
        {
            return 0.0f;
        }
        return (float)numVisibleVertices / mesh.vertices.Length;
    }

    bool TryAddData()
    {
        float vr = VisiblityRatio();
        if (vr < 0)
        {
            return false;
        }
        RenderTexture.active = _renderTexture;
        _tex.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        _tex.Apply();
        RenderTexture.active = null;

        byte[] bytes = _tex.EncodeToPNG();
        _images.Add(bytes);
        _visibility.Add(Convert.ToInt32(vr > 0.9f));
        return true;
    }

    void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    void OnApplicationQuit()
    {
        if (_images == null || _images.Count < _numExamples)
        {
            return;
        }
        using (var fs = File.Open(Application.dataPath + "/../Visibility.csv", FileMode.OpenOrCreate))
        {
            for (int c = 0; c < _images.Count; ++c)
            {
                File.WriteAllBytes(Application.dataPath + "/../Image" + c.ToString("0000") + ".png", _images[c]);
                AddText(fs, _visibility[c].ToString() + '\n');
            }
        }
        UnityEngine.Object.Destroy(_tex);
    }
}
