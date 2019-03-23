using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EdgeSobelFilter : MonoBehaviour
{
    [Range(0, 1)]
    public float Intensity = 0.25f;

	[Range(1, 10)]
	public float Threshold = 1.5f;

    private Texture2D texLena;

    private RawImage original;
    private RawImage combined;
    private RawImage horizontal;
    private RawImage vertical;

    private ComputeShader shader;

    private int kernel;

    private RenderTexture rtCombined;
    private RenderTexture rtHorizontal;
    private RenderTexture rtVertical;

    private void Start()
    {
        texLena = Resources.Load<Texture2D>("Images/lena");

        original = GameObject.Find("original").GetComponent<RawImage>();
        combined = GameObject.Find("combined").GetComponent<RawImage>();
        horizontal = GameObject.Find("horizontal").GetComponent<RawImage>();
        vertical = GameObject.Find("vertical").GetComponent<RawImage>();

        shader = Resources.Load<ComputeShader>("Shaders/EdgeSobelShader");

        kernel = shader.FindKernel("CSMain");

		rtCombined = new RenderTexture(256, 256, 24);
		rtCombined.enableRandomWrite = true;
		rtCombined.Create();

        rtHorizontal = new RenderTexture(256, 256, 24);
        rtHorizontal.enableRandomWrite = true;
        rtHorizontal.Create();

        rtVertical = new RenderTexture(256, 256, 24);
		rtVertical.enableRandomWrite = true;
		rtVertical.Create();

        float w = Screen.width;
        Vector2 anchor = new Vector2(0.5f, 0.5f);
        Vector2 sizeDelta = new Vector2(w * 0.5f,w * 0.5f);

        original.rectTransform.anchorMin = anchor;
        original.rectTransform.anchorMax = anchor;
        original.rectTransform.pivot = anchor;
        original.rectTransform.sizeDelta = sizeDelta;
        original.rectTransform.anchoredPosition = new Vector2(-w * 0.25f, w * 0.25f);

		combined.rectTransform.anchorMin = anchor;
		combined.rectTransform.anchorMax = anchor;
		combined.rectTransform.pivot = anchor;
		combined.rectTransform.sizeDelta = sizeDelta;
		combined.rectTransform.anchoredPosition = new Vector2(w * 0.25f, w * 0.25f);

		horizontal.rectTransform.anchorMin = anchor;
		horizontal.rectTransform.anchorMax = anchor;
		horizontal.rectTransform.pivot = anchor;
		horizontal.rectTransform.sizeDelta = sizeDelta;
		horizontal.rectTransform.anchoredPosition = new Vector2(-w * 0.25f, -w * 0.25f);

		vertical.rectTransform.anchorMin = anchor;
		vertical.rectTransform.anchorMax = anchor;
		vertical.rectTransform.pivot = anchor;
		vertical.rectTransform.sizeDelta = sizeDelta;
		vertical.rectTransform.anchoredPosition = new Vector2(w * 0.25f, -w * 0.25f);

        shader.SetTexture(kernel, "src", texLena);
        shader.SetTexture(kernel, "res", rtCombined);
        shader.SetTexture(kernel, "resH", rtHorizontal);
        shader.SetTexture(kernel, "resV", rtVertical);

        original.texture = texLena;

        //StartCoroutine(Draw());
    }

	private void Update()
	{
		shader.SetFloat("intensity", Intensity);
        shader.SetFloat("threshold", Threshold);

		shader.Dispatch(kernel, 32, 32, 1);

        combined.texture = rtCombined;
		horizontal.texture = rtHorizontal;
		vertical.texture = rtVertical;
	}

    private IEnumerator Draw()
    {
        shader.SetFloat("intensity", Intensity);
        shader.SetFloat("threshold", Threshold);

        shader.Dispatch(kernel, 32, 32, 1);

        combined.texture = rtCombined;
		horizontal.texture = rtHorizontal;
		vertical.texture = rtVertical;

        yield return new WaitForSeconds(0.25f);
        StartCoroutine(Draw());
    }

}
