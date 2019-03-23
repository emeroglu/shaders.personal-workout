using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TemplateMatching : MonoBehaviour
{
    public Texture2D Source;
    public Texture2D Template;

    private RawImage lena;

    private ComputeShader shader;

    private int kernel;
    private RenderTexture rtGraph;

    private void Start()
    {
        lena = GameObject.Find("lena").GetComponent<RawImage>();

        shader = Resources.Load<ComputeShader>("Shaders/TemplateMatchingShader");

        kernel = shader.FindKernel("CSMain");

        rtGraph = new RenderTexture(256, 256, 24);
        rtGraph.enableRandomWrite = true;
        rtGraph.Create();

        lena.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        lena.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        lena.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.width);
        lena.rectTransform.anchoredPosition = new Vector2(0, 0);

        StartCoroutine(Draw());
    }

    private IEnumerator Draw()
    {
        shader.SetInt("size", Template.width);

        shader.SetTexture(kernel, "src", Source);
        shader.SetTexture(kernel, "srcTemplate", Template);
        shader.SetTexture(kernel, "resGraph", rtGraph);

        shader.Dispatch(kernel, 32, 32, 1);

        Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        tex.alphaIsTransparency = true;

        RenderTexture.active = rtGraph;

        tex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);

        RenderTexture.active = null;

        float color, maxColor = 0;
        int maxI = 0, maxJ = 0;

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                color = tex.GetPixel(i, j).r;

                if (maxColor < color)
                {
                    maxColor = color;

                    maxI = i;
                    maxJ = j;
                }
            }
        }

        tex.SetPixels(Source.GetPixels());

        int offset = (Template.width - 1) / 2;

        for (int i = maxI - offset; i <= maxI + offset; i++)
        {
            for (int j = maxJ - offset; j <= maxJ + offset; j++)
            {
                if (i < maxI - offset + 2)
                    tex.SetPixel(i, j, Color.green);

				if (maxI + offset - 2 < i)
					tex.SetPixel(i, j, Color.green);

				if (j < maxJ - offset + 2)
					tex.SetPixel(i, j, Color.green);

				if (maxJ + offset - 2 < j)
					tex.SetPixel(i, j, Color.green);
            }
        }

        tex.Apply();

        lena.texture = tex;

        yield return new WaitForSeconds(1);
        StartCoroutine(Draw());
    }

}
