using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeatureDetection : MonoBehaviour
{
    [Range(0, 1)]
    public float Threshold = 0.5f;

    [Range(0, 1)]
    public float Sigma = 0.5f;

	[Range(0, 1)]
	public float Edge_Threshold = 0.01f;

    private RawImage riTopLeft;
    private RawImage riTopRight;
    private RawImage riBottomLeft;
    private RawImage riBottomRight;

    private RenderTexture texDummy;
    private RenderTexture texDummy2;

    private Texture2D texBlack;
    private Texture2D texVuforia;
    private List<Texture2D> texLena;
    private List<RenderTexture> texLenaGray;
    private List<List<RenderTexture>> texLenaGaussians;
    private List<List<RenderTexture>> texLenaDiffOfGaussians;
    private RenderTexture texLenaDogExtremas;
    private RenderTexture texLenaEdges;
    private RenderTexture texLenaEdgeEliminated;
    private Texture2D texLenaKeyPoints;

    private ComputeShader shaderGrayScale;
    private ComputeShader shaderGaussian;
    private ComputeShader shaderSubtractor;
    private ComputeShader shaderCopy;
    private ComputeShader shaderDogExtremas;
    private ComputeShader shaderSobel;
    private ComputeShader shaderEdgeElimination;

    private int kernelGrayScale;
    private int kernelGaussian;
    private int kernelSubtractor;
    private int kernelCopy;
    private int kernelDogExtremas;
    private int kernelSobel;
    private int kernelEdgeElimination;

    private void Start()
    {
        Initialize_Objects();

        Initialize_Textures();

        Scale_Down();

        Initialize_Canvas();

        Gray_Scale();

        Gaussian_Textures();

        Gaussians();

        Difference_Of_Gaussian_Textures();

        Difference_Of_Gaussians();

        Dog_Extrema_Textures();

        Dog_Extremas();

        Edges();

        Eliminate_Edges();

        Key_Point_Orientations();

        riTopLeft.texture = texLena[0];
        riTopRight.texture = texLenaEdges;
        riBottomLeft.texture = texVuforia;
        riBottomRight.texture = texLenaEdgeEliminated ;
    }

    private void Initialize_Objects()
    {
        riTopLeft = GameObject.Find("top_left").GetComponent<RawImage>();
        riTopRight = GameObject.Find("top_right").GetComponent<RawImage>();
        riBottomLeft = GameObject.Find("bottom_left").GetComponent<RawImage>();
        riBottomRight = GameObject.Find("bottom_right").GetComponent<RawImage>();

        texLena = new List<Texture2D>();
        texLenaGray = new List<RenderTexture>();
        texLenaGaussians = new List<List<RenderTexture>>();
        texLenaDiffOfGaussians = new List<List<RenderTexture>>();

        texLenaGaussians.Add(new List<RenderTexture>());
        texLenaGaussians.Add(new List<RenderTexture>());
        texLenaGaussians.Add(new List<RenderTexture>());
        texLenaGaussians.Add(new List<RenderTexture>());

        texLenaDiffOfGaussians.Add(new List<RenderTexture>());
        texLenaDiffOfGaussians.Add(new List<RenderTexture>());
        texLenaDiffOfGaussians.Add(new List<RenderTexture>());
        texLenaDiffOfGaussians.Add(new List<RenderTexture>());

        shaderGrayScale = Resources.Load<ComputeShader>("Shaders/GrayScaleShader");
        shaderGaussian = Resources.Load<ComputeShader>("Shaders/GaussianShader");
        shaderSubtractor = Resources.Load<ComputeShader>("Shaders/SubtractorShader");
        shaderCopy = Resources.Load<ComputeShader>("Shaders/CopyShader");
        shaderDogExtremas = Resources.Load<ComputeShader>("Shaders/DogExtremasShader");
        shaderSobel = Resources.Load<ComputeShader>("Shaders/EdgeSobelShader");
        shaderEdgeElimination = Resources.Load<ComputeShader>("Shaders/EdgeEliminationShader");

        kernelGrayScale = shaderGrayScale.FindKernel("CSMain");
        kernelGaussian = shaderGaussian.FindKernel("CSMain");
        kernelSubtractor = shaderSubtractor.FindKernel("CSMain");
        kernelCopy = shaderCopy.FindKernel("CSMain");
        kernelDogExtremas = shaderDogExtremas.FindKernel("CSMain");
        kernelSobel = shaderSobel.FindKernel("CSMain");
        kernelEdgeElimination = shaderEdgeElimination.FindKernel("CSMain");
    }

    private void Initialize_Textures()
    {
        texVuforia = Resources.Load<Texture2D>("Images/lena_features");

        Texture2D tex = Resources.Load<Texture2D>("Images/lena");

        texLena.Add(tex);

        texBlack = new Texture2D(256, 256);

        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                texBlack.SetPixel(i, j, Color.black);
            }
        }

        texDummy = new RenderTexture(256, 256, 24);
        texDummy.enableRandomWrite = true;
        texDummy.Create();

        texDummy2 = new RenderTexture(256, 256, 24);
        texDummy2.enableRandomWrite = true;
        texDummy2.Create();

        texLenaEdges = new RenderTexture(256, 256, 24);
        texLenaEdges.enableRandomWrite = true;
        texLenaEdges.Create();

        texLenaEdgeEliminated = new RenderTexture(256, 256, 24);
        texLenaEdgeEliminated.enableRandomWrite = true;
        texLenaEdgeEliminated.Create();
    }

    private void Scale_Down()
    {
        Texture2D tex = texLena[0];
        Texture2D tex_2 = new Texture2D(128, 128);
        Texture2D tex_4 = new Texture2D(64, 64);
        Texture2D tex_8 = new Texture2D(32, 32);

        int x, y;

        x = 0;
        y = 0;

        for (int i = 0; i < 256; i += 2)
        {
            x++;

            for (int j = 0; j < 256; j += 2)
            {
                y++;

                tex_2.SetPixel(x, y, tex.GetPixel(i, j));
            }
        }

        x = 0;
        y = 0;

        for (int i = 0; i < 256; i += 4)
        {
            x++;

            for (int j = 0; j < 256; j += 4)
            {
                y++;

                tex_4.SetPixel(x, y, tex.GetPixel(i, j));
            }
        }

        x = 0;
        y = 0;

        for (int i = 0; i < 256; i += 8)
        {
            x++;

            for (int j = 0; j < 256; j += 8)
            {
                y++;

                tex_8.SetPixel(x, y, tex.GetPixel(i, j));
            }
        }

        tex_2.Apply();
        tex_4.Apply();
        tex_8.Apply();

        texLena.Add(tex_2);
        texLena.Add(tex_4);
        texLena.Add(tex_8);
    }

    private void Initialize_Canvas()
    {
        Vector2 anchor = new Vector2(0.5f, 0.5f);

        float w = Screen.width;
        float side = w * 0.5f;

        Vector2 size = new Vector2(side, side);
        Vector2 size2 = new Vector2(side * 0.5f, side * 0.5f);
        Vector2 size4 = new Vector2(side * 0.25f, side * 0.25f);
        Vector2 size8 = new Vector2(side * 0.125f, side * 0.125f);

        Vector2 topLeft = new Vector2(w * -0.25f, w * 0.25f);
        Vector2 topRight = new Vector2(w * 0.25f, w * 0.25f);
        Vector2 bottomLeft = new Vector2(w * -0.25f, w * -0.25f);
        Vector2 bottomRight = new Vector2(w * 0.25f, w * -0.25f);

        riTopLeft.rectTransform.anchorMin = anchor;
        riTopLeft.rectTransform.anchorMax = anchor;

        riTopRight.rectTransform.anchorMin = anchor;
        riTopRight.rectTransform.anchorMax = anchor;

        riBottomLeft.rectTransform.anchorMin = anchor;
        riBottomLeft.rectTransform.anchorMax = anchor;

        riBottomRight.rectTransform.anchorMin = anchor;
        riBottomRight.rectTransform.anchorMax = anchor;

        riTopLeft.rectTransform.sizeDelta = size;
        riTopRight.rectTransform.sizeDelta = size;
        riBottomLeft.rectTransform.sizeDelta = size;
        riBottomRight.rectTransform.sizeDelta = size;

        riTopLeft.rectTransform.anchoredPosition = topLeft;
        riTopRight.rectTransform.anchoredPosition = topRight;
        riBottomLeft.rectTransform.anchoredPosition = bottomLeft;
        riBottomRight.rectTransform.anchoredPosition = bottomRight;

        riTopLeft.enabled = true;
        riTopRight.enabled = true;
        riBottomLeft.enabled = true;
        riBottomRight.enabled = true;
    }

    private void Gray_Scale()
    {
        RenderTexture rt;

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGray.Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGray.Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGray.Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGray.Add(rt);

        shaderGrayScale.SetTexture(kernelGrayScale, "src", texLena[0]);
        shaderGrayScale.SetTexture(kernelGrayScale, "res", texLenaGray[0]);

        shaderGrayScale.Dispatch(kernelGrayScale, 32, 32, 1);

        shaderGrayScale.SetTexture(kernelGrayScale, "src", texLena[1]);
        shaderGrayScale.SetTexture(kernelGrayScale, "res", texLenaGray[1]);

        shaderGrayScale.Dispatch(kernelGrayScale, 16, 16, 1);

        shaderGrayScale.SetTexture(kernelGrayScale, "src", texLena[2]);
        shaderGrayScale.SetTexture(kernelGrayScale, "res", texLenaGray[2]);

        shaderGrayScale.Dispatch(kernelGrayScale, 8, 8, 1);

        shaderGrayScale.SetTexture(kernelGrayScale, "src", texLena[3]);
        shaderGrayScale.SetTexture(kernelGrayScale, "res", texLenaGray[3]);

        shaderGrayScale.Dispatch(kernelGrayScale, 4, 4, 1);
    }

    private void Gaussian_Textures()
    {
        RenderTexture rt;

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[0].Add(rt);

        //

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[1].Add(rt);

        //

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[2].Add(rt);

        //

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaGaussians[3].Add(rt);
    }

    private void Gaussians()
    {
        int size = 5;
        float sigma = Sigma;

        sigma = Sigma * Mathf.Pow(2, 0.2f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[0]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[0][0]);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

        sigma = Sigma * Mathf.Pow(2, 0.4f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[0]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[0][1]);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

        sigma = Sigma * Mathf.Pow(2, 0.6f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[0]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[0][2]);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

        sigma = Sigma * Mathf.Pow(2, 0.8f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[0]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[0][3]);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

        sigma = Sigma * Mathf.Pow(2, 1);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[0]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[0][4]);

        shaderGaussian.Dispatch(kernelGaussian, 32, 32, 1);

        //

        sigma = Sigma * Mathf.Pow(2, 1.2f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[1]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[1][0]);

        shaderGaussian.Dispatch(kernelGaussian, 16, 16, 1);

        sigma = Sigma * Mathf.Pow(2, 1.4f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[1]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[1][1]);

        shaderGaussian.Dispatch(kernelGaussian, 16, 16, 1);

        sigma = Sigma * Mathf.Pow(2, 1.6f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[1]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[1][2]);

        shaderGaussian.Dispatch(kernelGaussian, 16, 16, 1);

        sigma = Sigma * Mathf.Pow(2, 1.8f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[1]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[1][3]);

        shaderGaussian.Dispatch(kernelGaussian, 16, 16, 1);

        sigma = Sigma * Mathf.Pow(2, 2);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[1]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[1][4]);

        shaderGaussian.Dispatch(kernelGaussian, 16, 16, 1);

        //

        sigma = Sigma * Mathf.Pow(2, 2.2f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[2]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[2][0]);

        shaderGaussian.Dispatch(kernelGaussian, 8, 8, 1);

        sigma = Sigma * Mathf.Pow(2, 2.4f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[2]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[2][1]);

        shaderGaussian.Dispatch(kernelGaussian, 8, 8, 1);

        sigma = Sigma * Mathf.Pow(2, 2.6f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[2]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[2][2]);

        shaderGaussian.Dispatch(kernelGaussian, 8, 8, 1);

        sigma = Sigma * Mathf.Pow(2, 2.8f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[2]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[2][3]);

        shaderGaussian.Dispatch(kernelGaussian, 8, 8, 1);

        sigma = Sigma * Mathf.Pow(2, 3);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[2]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[2][4]);

        shaderGaussian.Dispatch(kernelGaussian, 8, 8, 1);

        //

        sigma = Sigma * Mathf.Pow(2, 3.2f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[3]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[3][0]);

        shaderGaussian.Dispatch(kernelGaussian, 4, 4, 1);

        sigma = Sigma * Mathf.Pow(2, 3.4f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[3]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[3][1]);

        shaderGaussian.Dispatch(kernelGaussian, 4, 4, 1);

        sigma = Sigma * Mathf.Pow(2, 3.6f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[3]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[3][2]);

        shaderGaussian.Dispatch(kernelGaussian, 4, 4, 1);

        sigma = Sigma * Mathf.Pow(2, 3.8f);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[3]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[3][3]);

        shaderGaussian.Dispatch(kernelGaussian, 4, 4, 1);

        sigma = Sigma * Mathf.Pow(2, 4);

        shaderGaussian.SetInt("size", size);
        shaderGaussian.SetFloat("sigma", sigma);

        shaderGaussian.SetTexture(kernelGaussian, "src", texLenaGray[3]);
        shaderGaussian.SetTexture(kernelGaussian, "res", texLenaGaussians[3][4]);

        shaderGaussian.Dispatch(kernelGaussian, 4, 4, 1);
    }

    private void Difference_Of_Gaussian_Textures()
    {
        RenderTexture rt;

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[0].Add(rt);

        rt = new RenderTexture(256, 256, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[0].Add(rt);

        //

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[1].Add(rt);

        rt = new RenderTexture(128, 128, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[1].Add(rt);

        //

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[2].Add(rt);

        rt = new RenderTexture(64, 64, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[2].Add(rt);

        //

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[3].Add(rt);

        rt = new RenderTexture(32, 32, 24);
        rt.enableRandomWrite = true;
        rt.Create();

        texLenaDiffOfGaussians[3].Add(rt);
    }

    private void Difference_Of_Gaussians()
    {
        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[0][0]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[0][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[0][0]);

        shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[0][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[0][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[0][1]);

        shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[0][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[0][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[0][2]);

        shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[0][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[0][4]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[0][3]);

        shaderSubtractor.Dispatch(kernelSubtractor, 32, 32, 1);

        //

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[1][0]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[1][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[1][0]);

        shaderSubtractor.Dispatch(kernelSubtractor, 16, 16, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[1][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[1][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[1][1]);

        shaderSubtractor.Dispatch(kernelSubtractor, 16, 16, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[1][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[1][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[1][2]);

        shaderSubtractor.Dispatch(kernelSubtractor, 16, 16, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[1][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[1][4]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[1][3]);

        shaderSubtractor.Dispatch(kernelSubtractor, 16, 16, 1);

        //

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[2][0]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[2][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[2][0]);

        shaderSubtractor.Dispatch(kernelSubtractor, 8, 8, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[2][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[2][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[2][1]);

        shaderSubtractor.Dispatch(kernelSubtractor, 8, 8, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[2][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[2][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[2][2]);

        shaderSubtractor.Dispatch(kernelSubtractor, 8, 8, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[2][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[2][4]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[2][3]);

        shaderSubtractor.Dispatch(kernelSubtractor, 8, 8, 1);

        //

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[3][0]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[3][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[3][0]);

        shaderSubtractor.Dispatch(kernelSubtractor, 4, 4, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[3][1]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[3][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[3][1]);

        shaderSubtractor.Dispatch(kernelSubtractor, 4, 4, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[3][2]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[3][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[3][2]);

        shaderSubtractor.Dispatch(kernelSubtractor, 4, 4, 1);

        shaderSubtractor.SetTexture(kernelSubtractor, "src", texLenaGaussians[3][3]);
        shaderSubtractor.SetTexture(kernelSubtractor, "src2", texLenaGaussians[3][4]);
        shaderSubtractor.SetTexture(kernelSubtractor, "res", texLenaDiffOfGaussians[3][3]);

        shaderSubtractor.Dispatch(kernelSubtractor, 4, 4, 1);
    }

    private void Dog_Extrema_Textures()
    {
        texLenaDogExtremas = new RenderTexture(256, 256, 24);
        texLenaDogExtremas.enableRandomWrite = true;
        texLenaDogExtremas.Create();

        shaderCopy.SetTexture(kernelCopy, "src", texLenaGray[0]);
        shaderCopy.SetTexture(kernelCopy, "res", texLenaDogExtremas);

        shaderCopy.Dispatch(kernelCopy, 32, 32, 1);
    }

    private void Dog_Extremas()
    {
        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 1);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[0][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[0][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 32, 32, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 1);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[0][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[0][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[0][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 32, 32, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 1);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[0][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[0][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[0][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 32, 32, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 1);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[0][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[0][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 32, 32, 1);

        //

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 2);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[1][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[1][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 16, 16, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 2);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[1][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[1][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[1][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 16, 16, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 2);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[1][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[1][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[1][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 16, 16, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 2);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[1][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[1][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 16, 16, 1);

        //

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 4);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[2][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[2][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 8, 8, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 4);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[2][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[2][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[2][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 8, 8, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 4);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[2][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[2][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[2][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 8, 8, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 4);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[2][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[2][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 8, 8, 1);

        //

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 8);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[3][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[3][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 4, 4, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 8);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[3][0]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[3][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[3][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 4, 4, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 8);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[3][1]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[3][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texLenaDiffOfGaussians[3][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 4, 4, 1);

        shaderDogExtremas.SetFloat("threshold", Threshold);
        shaderDogExtremas.SetInt("scale", 8);

        shaderDogExtremas.SetTexture(kernelDogExtremas, "src", texLenaDiffOfGaussians[3][2]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src2", texLenaDiffOfGaussians[3][3]);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "src3", texBlack);
        shaderDogExtremas.SetTexture(kernelDogExtremas, "res", texLenaDogExtremas);

        shaderDogExtremas.Dispatch(kernelDogExtremas, 4, 4, 1);
    }

    private void Edges()
    {
        shaderSobel.SetFloat("intensity", 0);
        shaderSobel.SetFloat("threshold", 0);

        shaderSobel.SetTexture(kernelSobel, "src", texLenaGaussians[0][0]);
        shaderSobel.SetTexture(kernelSobel, "res", texLenaEdges);
        shaderSobel.SetTexture(kernelSobel, "resH", texDummy);
        shaderSobel.SetTexture(kernelSobel, "resV", texDummy2);

        shaderSobel.Dispatch(kernelSobel, 32, 32, 1);
    }

    private void Eliminate_Edges()
    {
        shaderEdgeElimination.SetFloat("threshold", Edge_Threshold);

        shaderEdgeElimination.SetTexture(kernelEdgeElimination, "src", texLenaGray[0]);
        shaderEdgeElimination.SetTexture(kernelEdgeElimination, "srcEx", texLenaDogExtremas);
        shaderEdgeElimination.SetTexture(kernelEdgeElimination, "srcEdges", texLenaEdges);
        shaderEdgeElimination.SetTexture(kernelEdgeElimination, "res", texLenaEdgeEliminated);

        shaderEdgeElimination.Dispatch(kernelEdgeElimination, 32, 32, 1);
    }

    private void Key_Point_Orientations()
    {
        RenderTexture.active = texLenaEdgeEliminated;

		texLenaKeyPoints.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);

		RenderTexture.active = null;

        Color color, colorLeft;

        for (int i = 2; i <= 253; i++)
        {
            for (int j = 2; j <= 253; j++)
            {
                color = texLenaKeyPoints.GetPixel(i, j);

                if (color == Color.green)
                {
                    for (int wi = -2; wi <= 2; wi++)
                    {
                        for (int wj = -2; wj <= 2; wj++)
                        {

                        }
                    }
                }
            }
        }
    }
}
