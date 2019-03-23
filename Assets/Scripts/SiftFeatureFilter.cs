using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SiftFeatureFilter : MonoBehaviour
{
    [Range(0, 0.25f)]
    public float DogThresholdMax = 0.05f;

	[Range(0, 1)]
	public float SobelIntensity = 0.25f;

	[Range(1, 10)]
	public float SobelThreshold = 1.5f;

	[Range(0, 1)]
	public float ThresholdMinEdge = 0.25f;

    private RawImage original;
    private RawImage edges;
    private RawImage vuforia;
    private RawImage sift;

    private ComputeShader shaderCopy;
    private ComputeShader shaderGaussian;
    private ComputeShader shaderSubtractor;
    private ComputeShader shaderDogExtremas;
    private ComputeShader shaderEdgeSobel;

    private int kernelCopy;
    private int kernelGaussian;
    private int kernelSubtractor;
    private int kernelDogExtremas;
    private int kernelEdgeSobel;

    private Texture2D lena;
    private Texture2D lena_features;

    private RenderTexture rtLena;
    private RenderTexture rtLenaGray;

    private RenderTexture rtSift;

    private RenderTexture rtGaussian1;
    private RenderTexture rtGaussian2;
    private RenderTexture rtGaussian3;
    private RenderTexture rtGaussian4;
    private RenderTexture rtGaussian5;

	private RenderTexture rtGaussianGray1;
	private RenderTexture rtGaussianGray2;
	private RenderTexture rtGaussianGray3;
	private RenderTexture rtGaussianGray4;
	private RenderTexture rtGaussianGray5;

	private RenderTexture rtDog1;
	private RenderTexture rtDog2;
    private RenderTexture rtDog3;
    private RenderTexture rtDog4;

	private RenderTexture rtDogGray1;
	private RenderTexture rtDogGray2;
	private RenderTexture rtDogGray3;
	private RenderTexture rtDogGray4;

	private RenderTexture rtCombined;
	private RenderTexture rtHorizontal;
	private RenderTexture rtVertical;

    private void Start()
    {
        lena = Resources.Load<Texture2D>("Images/lena");
        lena_features = Resources.Load<Texture2D>("Images/lena_features");

        original = GameObject.Find("original").GetComponent<RawImage>();
        edges = GameObject.Find("edges").GetComponent<RawImage>();
        vuforia = GameObject.Find("vuforia").GetComponent<RawImage>();
        sift = GameObject.Find("sift").GetComponent<RawImage>();

        float w = Screen.width;
        Vector2 anchor = new Vector2(0.5f, 0.5f);
        Vector2 sizeDelta = new Vector2(w * 0.5f, w * 0.5f);

        original.rectTransform.anchorMin = anchor;
        original.rectTransform.anchorMax = anchor;
        original.rectTransform.pivot = anchor;
        original.rectTransform.sizeDelta = sizeDelta;
        original.rectTransform.anchoredPosition = new Vector2(-w * 0.25f, w * 0.25f);

        edges.rectTransform.anchorMin = anchor;
        edges.rectTransform.anchorMax = anchor;
        edges.rectTransform.pivot = anchor;
        edges.rectTransform.sizeDelta = sizeDelta;
        edges.rectTransform.anchoredPosition = new Vector2(w * 0.25f, w * 0.25f);

        vuforia.rectTransform.anchorMin = anchor;
        vuforia.rectTransform.anchorMax = anchor;
        vuforia.rectTransform.pivot = anchor;
        vuforia.rectTransform.sizeDelta = sizeDelta;
        vuforia.rectTransform.anchoredPosition = new Vector2(-w * 0.25f, -w * 0.25f);

        sift.rectTransform.anchorMin = anchor;
        sift.rectTransform.anchorMax = anchor;
        sift.rectTransform.pivot = anchor;
        sift.rectTransform.sizeDelta = sizeDelta;
        sift.rectTransform.anchoredPosition = new Vector2(w * 0.25f, -w * 0.25f);

        shaderCopy = Resources.Load<ComputeShader>("Shaders/CopyShader");
        shaderGaussian = Resources.Load<ComputeShader>("Shaders/GaussianFilterShader");
        shaderSubtractor = Resources.Load<ComputeShader>("Shaders/SubtractorShader");
        shaderDogExtremas = Resources.Load<ComputeShader>("Shaders/DogExtremasShader");
        shaderEdgeSobel = Resources.Load<ComputeShader>("Shaders/EdgeSobelShader");

        kernelCopy = shaderCopy.FindKernel("CSMain");
        kernelGaussian = shaderGaussian.FindKernel("CSMain");
        kernelSubtractor = shaderSubtractor.FindKernel("CSMain");
        kernelDogExtremas = shaderDogExtremas.FindKernel("CSMain");
        kernelEdgeSobel = shaderEdgeSobel.FindKernel("CSMain");

        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
		rtLena = new RenderTexture(256, 256, 24);
		rtLena.enableRandomWrite = true;
		rtLena.Create();

        rtLenaGray = new RenderTexture(256, 256, 24);
        rtLenaGray.enableRandomWrite = true;
        rtLenaGray.Create();

        rtSift = new RenderTexture(256, 256, 24);
		rtSift.enableRandomWrite = true;
		rtSift.Create();

        rtGaussian1 = new RenderTexture(256, 256, 24);
        rtGaussian1.enableRandomWrite = true;
        rtGaussian1.Create();

        rtGaussian2 = new RenderTexture(256, 256, 24);
        rtGaussian2.enableRandomWrite = true;
        rtGaussian2.Create();

        rtGaussian3 = new RenderTexture(256, 256, 24);
        rtGaussian3.enableRandomWrite = true;
        rtGaussian3.Create();

        rtGaussian4 = new RenderTexture(256, 256, 24);
        rtGaussian4.enableRandomWrite = true;
        rtGaussian4.Create();

        rtGaussian5 = new RenderTexture(256, 256, 24);
        rtGaussian5.enableRandomWrite = true;
        rtGaussian5.Create();

		rtGaussianGray1 = new RenderTexture(256, 256, 24);
		rtGaussianGray1.enableRandomWrite = true;
		rtGaussianGray1.Create();

		rtGaussianGray2 = new RenderTexture(256, 256, 24);
		rtGaussianGray2.enableRandomWrite = true;
		rtGaussianGray2.Create();

		rtGaussianGray3 = new RenderTexture(256, 256, 24);
		rtGaussianGray3.enableRandomWrite = true;
		rtGaussianGray3.Create();

		rtGaussianGray4 = new RenderTexture(256, 256, 24);
		rtGaussianGray4.enableRandomWrite = true;
		rtGaussianGray4.Create();

		rtGaussianGray5 = new RenderTexture(256, 256, 24);
		rtGaussianGray5.enableRandomWrite = true;
		rtGaussianGray5.Create();

        rtDog1 = new RenderTexture(256, 256, 24);
		rtDog1.enableRandomWrite = true;
		rtDog1.Create();

        rtDog2 = new RenderTexture(256, 256, 24);
        rtDog2.enableRandomWrite = true;
        rtDog2.Create();

        rtDog3 = new RenderTexture(256, 256, 24);
        rtDog3.enableRandomWrite = true;
        rtDog3.Create();

        rtDog4 = new RenderTexture(256, 256, 24);
        rtDog4.enableRandomWrite = true;
        rtDog4.Create();

		rtDogGray1 = new RenderTexture(256, 256, 24);
		rtDogGray1.enableRandomWrite = true;
		rtDogGray1.Create();

		rtDogGray2 = new RenderTexture(256, 256, 24);
		rtDogGray2.enableRandomWrite = true;
		rtDogGray2.Create();

		rtDogGray3 = new RenderTexture(256, 256, 24);
		rtDogGray3.enableRandomWrite = true;
		rtDogGray3.Create();

		rtDogGray4 = new RenderTexture(256, 256, 24);
		rtDogGray4.enableRandomWrite = true;
		rtDogGray4.Create();

		rtCombined = new RenderTexture(256, 256, 24);
		rtCombined.enableRandomWrite = true;
		rtCombined.Create();

		rtHorizontal = new RenderTexture(256, 256, 24);
		rtHorizontal.enableRandomWrite = true;
		rtHorizontal.Create();

		rtVertical = new RenderTexture(256, 256, 24);
		rtVertical.enableRandomWrite = true;
		rtVertical.Create();

		rtSift = new RenderTexture(256, 256, 24);
		rtSift.enableRandomWrite = true;
		rtSift.Create();

        yield return Gaussians();
    }

    private IEnumerator Gaussians()
    {
		shaderCopy.SetTexture(kernelCopy, "src", lena);
		shaderCopy.SetTexture(kernelCopy, "res", rtSift);

		shaderCopy.Dispatch(kernelCopy, 32, 32, 1);



        shaderGaussian.SetInt("size", 30);
        shaderGaussian.SetInt("sigma", 1);

        shaderGaussian.SetTexture(kernelGaussian, "src", lena);
        shaderGaussian.SetTexture(kernelGaussian, "srcGray", rtLenaGray);
        shaderGaussian.SetTexture(kernelGaussian, "res", rtGaussian1);
        shaderGaussian.SetTexture(kernelGaussian, "resGray", rtGaussianGray1);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

		shaderGaussian.SetInt("size", 30);
		shaderGaussian.SetInt("sigma", 2);

		shaderGaussian.SetTexture(kernelGaussian, "src", lena);
        shaderGaussian.SetTexture(kernelGaussian, "srcGray", rtLenaGray);
		shaderGaussian.SetTexture(kernelGaussian, "res", rtGaussian2);
        shaderGaussian.SetTexture(kernelGaussian, "resGray", rtGaussianGray2);

		shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

		shaderGaussian.SetInt("size", 30);
		shaderGaussian.SetInt("sigma", 3);

		shaderGaussian.SetTexture(kernelGaussian, "src", lena);
        shaderGaussian.SetTexture(kernelGaussian, "srcGray", rtLenaGray);
		shaderGaussian.SetTexture(kernelGaussian, "res", rtGaussian3);
        shaderGaussian.SetTexture(kernelGaussian, "resGray", rtGaussianGray3);

		shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

		shaderGaussian.SetInt("size", 30);
		shaderGaussian.SetInt("sigma", 4);

		shaderGaussian.SetTexture(kernelGaussian, "src", lena);
        shaderGaussian.SetTexture(kernelGaussian, "srcGray", rtLenaGray);
        shaderGaussian.SetTexture(kernelGaussian, "res", rtGaussian4);
        shaderGaussian.SetTexture(kernelGaussian, "resGray", rtGaussianGray4);

		shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

		shaderGaussian.SetInt("size", 30);
		shaderGaussian.SetInt("sigma", 5);

		shaderGaussian.SetTexture(kernelGaussian, "src", lena);
        shaderGaussian.SetTexture(kernelGaussian, "srcGray", rtLenaGray);
		shaderGaussian.SetTexture(kernelGaussian, "res", rtGaussian5);
        shaderGaussian.SetTexture(kernelGaussian, "resGray", rtGaussianGray5);

		shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);



        shaderSubtractor.SetTexture(kernelSubtractor, "src", rtGaussian2);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", rtGaussian1);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", rtDog1);
        shaderSubtractor.SetTexture(kernelSubtractor, "resGray", rtDogGray1);

        shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", rtGaussian3);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", rtGaussian2);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", rtDog2);
        shaderSubtractor.SetTexture(kernelSubtractor, "resGray", rtDogGray2);

		shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", rtGaussian4);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", rtGaussian3);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", rtDog3);
        shaderSubtractor.SetTexture(kernelSubtractor, "resGray", rtDogGray3);

		shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", rtGaussian5);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", rtGaussian4);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", rtDog4);
        shaderSubtractor.SetTexture(kernelSubtractor, "resGray", rtDogGray4);

        shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);



		shaderEdgeSobel.SetFloat("intensity", SobelIntensity);
		shaderEdgeSobel.SetFloat("threshold", SobelThreshold);

		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "src", rtGaussian1);
		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "res", rtCombined);
		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "resH", rtHorizontal);
		shaderEdgeSobel.SetTexture(kernelEdgeSobel, "resV", rtVertical);

		shaderEdgeSobel.Dispatch(kernelEdgeSobel, 32, 32, 1);



        shaderDogExtremas.SetFloat("thresholdMax", DogThresholdMax);
        shaderDogExtremas.SetFloat("thresholdMinEdge", ThresholdMinEdge);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", rtDogGray1);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", rtDogGray2);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", rtDogGray3);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "srcEdge", rtCombined);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", rtSift);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 32, 32, 1);

		shaderDogExtremas.SetFloat("thresholdMax", DogThresholdMax);
        shaderDogExtremas.SetFloat("thresholdMinEdge", ThresholdMinEdge);

		shaderDogExtremas.SetTexture(kernelDogExtremas, "src", rtDogGray2);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", rtDogGray3);
		shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", rtDogGray4);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "srcEdge", rtCombined);
		shaderDogExtremas.SetTexture(kernelDogExtremas, "res", rtSift);

		shaderDogExtremas.Dispatch(kernelDogExtremas, 32, 32, 1);



        original.texture = lena;
        edges.texture = rtCombined;
        vuforia.texture = lena_features;
        sift.texture = rtSift;



		yield return new WaitForSeconds(0.5f);
		StartCoroutine(Gaussians());
    }

}
