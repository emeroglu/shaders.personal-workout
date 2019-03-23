using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EdgeGaussianSobelFilter : MonoBehaviour
{
	[Range(3, 49)]
	public int Size = 5;

	[Range(1, 50)]
	public int Sigma = 1;

    [Range(0, 1)]
    public float Intensity = 0.25f;

	[Range(1, 10)]
	public float Threshold = 1.5f;

    private Texture2D texLena;

    private RawImage smooth;
    private RawImage combined;
    private RawImage horizontal;
    private RawImage vertical;

    private ComputeShader shaderGaussian;
    private ComputeShader shaderEdgeSobel;

    private int kernelGaussian;
    private int kernelEdgeSobel;

    private RenderTexture rtDummy;
    private RenderTexture rtDummy2;
    private RenderTexture rtSmooth;
    private RenderTexture rtCombined;
    private RenderTexture rtHorizontal;
    private RenderTexture rtVertical;

    private void Start()
    {
        texLena = Resources.Load<Texture2D>("Images/lena");

        smooth = GameObject.Find("smooth").GetComponent<RawImage>();
        combined = GameObject.Find("combined").GetComponent<RawImage>();
        horizontal = GameObject.Find("horizontal").GetComponent<RawImage>();
        vertical = GameObject.Find("vertical").GetComponent<RawImage>();

        shaderGaussian = Resources.Load<ComputeShader>("Shaders/GaussianFilterShader");
        shaderEdgeSobel = Resources.Load<ComputeShader>("Shaders/EdgeSobelShader");

        kernelGaussian = shaderGaussian.FindKernel("CSMain");
        kernelEdgeSobel = shaderEdgeSobel.FindKernel("CSMain");

		rtDummy = new RenderTexture(256, 256, 24);
		rtDummy.enableRandomWrite = true;
		rtDummy.Create();

		rtDummy2 = new RenderTexture(256, 256, 24);
		rtDummy2.enableRandomWrite = true;
		rtDummy2.Create();

        rtSmooth = new RenderTexture(256, 256, 24);
        rtSmooth.enableRandomWrite = true;
        rtSmooth.Create();

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

        smooth.rectTransform.anchorMin = anchor;
        smooth.rectTransform.anchorMax = anchor;
        smooth.rectTransform.pivot = anchor;
        smooth.rectTransform.sizeDelta = sizeDelta;
        smooth.rectTransform.anchoredPosition = new Vector2(-w * 0.25f, w * 0.25f);

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

		shaderGaussian.SetInt("size", Size);
		shaderGaussian.SetInt("sigma", Sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLena);
        shaderGaussian.SetTexture(kernelGaussian, "srcGray", rtDummy);
        shaderGaussian.SetTexture(kernelGaussian, "res", rtSmooth);
        shaderGaussian.SetTexture(kernelGaussian, "resGray", rtDummy2);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

        shaderEdgeSobel.SetFloat("intensity", Intensity);
        shaderEdgeSobel.SetFloat("threshold", Threshold);

        shaderEdgeSobel.SetTexture(kernelEdgeSobel, "src", rtSmooth);
        shaderEdgeSobel.SetTexture(kernelEdgeSobel, "res", rtCombined);
        shaderEdgeSobel.SetTexture(kernelEdgeSobel, "resH", rtHorizontal);
        shaderEdgeSobel.SetTexture(kernelEdgeSobel, "resV", rtVertical);

        shaderEdgeSobel.Dispatch(kernelEdgeSobel, 32, 32, 1);

        smooth.texture = rtSmooth;
		combined.texture = rtCombined;
		horizontal.texture = rtHorizontal;
		vertical.texture = rtVertical;

        StartCoroutine(Draw());
    }

    private IEnumerator Draw()
    {
		shaderGaussian.SetInt("size", Size);
		shaderGaussian.SetInt("sigma", Sigma);

		shaderGaussian.SetTexture(kernelGaussian, "src", texLena);
		shaderGaussian.SetTexture(kernelGaussian, "res", rtSmooth);

		shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

		shaderEdgeSobel.SetFloat("intensity", Intensity);
        shaderEdgeSobel.SetFloat("threshold", Threshold);

		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "src", rtSmooth);
		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "res", rtCombined);
		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "resH", rtHorizontal);
		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "resV", rtVertical);

        shaderEdgeSobel.Dispatch(kernelEdgeSobel, 32, 32, 1);

		smooth.texture = rtSmooth;
		combined.texture = rtCombined;
		horizontal.texture = rtHorizontal;
		vertical.texture = rtVertical;

        yield return new WaitForSeconds(0.25f);
        StartCoroutine(Draw());
    }

}
