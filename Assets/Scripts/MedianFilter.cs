using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MedianFilter : MonoBehaviour
{
    private RawImage original;
    private RawImage originalGray;
    private RawImage filtered;
    private RawImage filteredGray;

    private ComputeShader shader;
    private Texture2D texture;

    private int kernel;
    private RenderTexture rtGray;
    private RenderTexture rtFiltered;
    private RenderTexture rtFilteredGray;

    private void Start()
    {
        original = GameObject.Find("original").GetComponent<RawImage>();
        originalGray = GameObject.Find("originalGray").GetComponent<RawImage>();
        filtered = GameObject.Find("filtered").GetComponent<RawImage>();
        filteredGray = GameObject.Find("filteredGray").GetComponent<RawImage>();

        shader = Resources.Load<ComputeShader>("Shaders/MedianFilterShader");
        texture = Resources.Load<Texture2D>("Images/man");

        kernel = shader.FindKernel("CSMain");

        rtGray = new RenderTexture(256, 256, 24);
		rtGray.enableRandomWrite = true;
		rtGray.Create();

        rtFiltered = new RenderTexture(256, 256, 24);
        rtFiltered.enableRandomWrite = true;
        rtFiltered.Create();

        rtFilteredGray = new RenderTexture(256, 256, 24);
        rtFilteredGray.enableRandomWrite = true;
        rtFilteredGray.Create();

        float w = Screen.width * 0.5f;
        float q = w * 0.5f;
        Vector2 v = new Vector2(w, w);
        Vector2 a = new Vector2(0.5f, 0.5f);

        original.rectTransform.anchorMin = a;
        originalGray.rectTransform.anchorMin = a;
        filtered.rectTransform.anchorMin = a;
        filteredGray.rectTransform.anchorMin = a;

		original.rectTransform.anchorMax = a;
		originalGray.rectTransform.anchorMax = a;
		filtered.rectTransform.anchorMax = a;
		filteredGray.rectTransform.anchorMax = a;

        original.rectTransform.sizeDelta = v;
        originalGray.rectTransform.sizeDelta = v;
        filtered.rectTransform.sizeDelta = v;
        filteredGray.rectTransform.sizeDelta = v;

        original.rectTransform.anchoredPosition = new Vector2(-q, q);
        originalGray.rectTransform.anchoredPosition = new Vector2(-q, -q);
        filtered.rectTransform.anchoredPosition = new Vector2(q, q);
        filteredGray.rectTransform.anchoredPosition = new Vector2(q, -q);

        shader.SetTexture(kernel, "src", texture);
        shader.SetTexture(kernel, "srcGray", rtGray);
		shader.SetTexture(kernel, "res", rtFiltered);
		shader.SetTexture(kernel, "resGray", rtFilteredGray);

        shader.Dispatch(kernel, 32, 32, 1);

        original.texture = texture;
        originalGray.texture = rtGray;
		filtered.texture = rtFiltered;
		filteredGray.texture = rtFilteredGray;
    }

}
