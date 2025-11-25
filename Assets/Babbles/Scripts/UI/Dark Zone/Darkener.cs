using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;
using System.Threading;
using System.Threading.Tasks;

public class Darkener : MonoBehaviour
{
    [SerializeField] private ComputeShader _computeShader;
    private RenderTexture _texture;
    private RenderTexture _textureIntermediary;
    int idFill;
    int idArea;
    int idClear;
    int idRadius;
    int idGradient;
    int idLayeredGradient;
    int idApply;
    public Vector2Int TextureSize
    {
        get
        {
            return new Vector2Int(_texture.width, _texture.height);
        }
    }

    public RenderTexture Texture { get => _texture; set => _texture = value; }

    void OnValidate()
    {
        GetComponent<RawImage>().enabled = Application.isPlaying;
    }
    void Awake()
    {
        GetComponent<RawImage>().enabled = Application.isPlaying;
    }

    void Start()
    {
        var t = GetComponent<RawImage>();
        _texture = new RenderTexture(1920, 1080, 24)
        {
            enableRandomWrite = true
        };
        _texture.Create();
        t.texture = _texture;
        _textureIntermediary = new RenderTexture(1920, 1080, 24)
        {
            enableRandomWrite = true
        };
        _textureIntermediary.Create();
        idFill = _computeShader.FindKernel("Fill");
        idArea = _computeShader.FindKernel("Area");
        idClear = _computeShader.FindKernel("Clear");
        idRadius = _computeShader.FindKernel("Radius");
        idGradient = _computeShader.FindKernel("Gradient");
        idLayeredGradient = _computeShader.FindKernel("LayeredGradient");
        idApply = _computeShader.FindKernel("ApplyIntermediary");
        ClearScreen();
    }

    public void UpdateInLayeredGradient(Vector2Int pos, int[] radius, Color32 color, Vector2[] alphaVariation, bool reverse, bool drawOver = true, bool intermediary = false, bool lightBlend = false)
    {
        RenderTexture textureToUse = intermediary ? ref _textureIntermediary : ref _texture;
        Color32[] _ =  GetPixelsInScreen(new Vector2Int(pos.x - radius[0], pos.y - radius[0]), new Vector2Int(radius[0] * 2, radius[0] * 2), out Vector2Int posOut, out Vector2Int areaOut, out Vector2Int firstPixels);
        Vector2Int mid = new Vector2Int(pos.x - posOut.x, pos.y - posOut.y); // radius[0] is already included
        int[] t = new int[8*4];
        for (int i = 0; i < t.Length; i++)
        {
            if(radius.Length * 4 > i)
            {
                if (i % 4 == 0)
                {
                    t[i] = radius[i / 4];
                }
            }
        }
        float[] r = new float[8*4];
        for (int i = 0; i < r.Length; i++)
        {
            if (alphaVariation.Length*4 > i)
            {
                if(i%4 == 0)
                {
                    r[i] = alphaVariation[i / 4].x;
                }
                if(i%4 == 1)
                {
                    r[i] = alphaVariation[i / 4].y;
                }
            }
        }
        _computeShader.SetFloats("CurrentColor", ((Color)color).ToArray());
        _computeShader.SetInts("radiuses", t);
        _computeShader.SetBool("reverse", reverse);
        _computeShader.SetFloats("alphaVariations", r);
        _computeShader.SetInts("mid", new int[] { mid.x + posOut.x, mid.y + posOut.y });
        _computeShader.SetBool("drawOver", drawOver);
        _computeShader.SetBool("lightBlend", lightBlend);
        _computeShader.SetTexture(idLayeredGradient, "Result", textureToUse);
        _computeShader.Dispatch(idLayeredGradient, _texture.width / 8, _texture.height / 8, 1);
    }

    public void Fill(Color32 color, bool drawOver = true, bool intermediary = false)
    {
        RenderTexture textureToUse = intermediary ? ref _textureIntermediary : ref _texture;
        _computeShader.SetFloats("CurrentColor", ((Color)color).ToArray());
        _computeShader.SetBool("drawOver", drawOver);
        _computeShader.SetTexture(idFill, "Result", textureToUse);
        _computeShader.Dispatch(idFill, _texture.width / 8, _texture.height / 8, 1);
    }

    public void UpdateInGradient(Vector2Int pos, int radius, Color32 color, Vector2 alphaVariation, bool reverse, bool drawOver = true, bool intermediary = false)
    {
        RenderTexture textureToUse = intermediary ? ref _textureIntermediary : ref _texture;
        Color32[] _ = GetPixelsInScreen(new Vector2Int(pos.x - radius, pos.y - radius), new Vector2Int(radius * 2, radius * 2), out Vector2Int posOut, out Vector2Int areaOut, out Vector2Int firstPixels);
        Vector2Int mid = new Vector2Int(pos.x - posOut.x, pos.y - posOut.y); // radius is already included
        _computeShader.SetFloats("CurrentColor", ((Color)color).ToArray());
        _computeShader.SetInt("radius", radius);
        _computeShader.SetBool("reverse", reverse);
        _computeShader.SetFloats("alphaVariation", new float[] { alphaVariation.x, alphaVariation.y });
        _computeShader.SetInts("mid", new int[] { mid.x + posOut.x, mid.y + posOut.y });
        _computeShader.SetBool("drawOver", drawOver);
        _computeShader.SetTexture(idGradient, "Result", textureToUse);
        _computeShader.Dispatch(idGradient, _texture.width / 8, _texture.height / 8, 1);
    }

    public void UpdateInRadius(Vector2Int pos, int radius, Color32 color, bool drawOver = true, bool intermediary = false)
    {
        RenderTexture textureToUse = intermediary ? ref _textureIntermediary : ref _texture;
        Color32[] _ = GetPixelsInScreen(new Vector2Int(pos.x - radius, pos.y - radius), new Vector2Int(radius * 2, radius * 2), out Vector2Int posOut, out Vector2Int _, out Vector2Int _);
        Vector2Int mid = new Vector2Int(pos.x - posOut.x, pos.y - posOut.y); // radius is already included
        _computeShader.SetFloats("CurrentColor", ((Color)color).ToArray());
        _computeShader.SetInt("radius", radius);
        _computeShader.SetInts("mid", new int[] { mid.x+posOut.x, mid.y+posOut.y });
        _computeShader.SetBool("drawOver", drawOver);
        _computeShader.SetTexture(idRadius, "Result", textureToUse);
        _computeShader.Dispatch(idRadius, _texture.width / 8, _texture.height / 8, 1);
    }

    public void ClearScreen()
    {
        _computeShader.SetTexture(idClear, "Result", _texture);
        _computeShader.Dispatch(idClear, _texture.width / 8, _texture.height / 8, 1);
    }

    public void ClearIntermediaryTexture()
    {
        _computeShader.SetTexture(idClear, "Result", _textureIntermediary);
        _computeShader.Dispatch(idClear, _texture.width / 8, _texture.height / 8, 1);
    }

    public void ApplyIntermediaryTexture()
    {
        _computeShader.SetTexture(idApply, "Result", _texture);
        _computeShader.SetTexture(idApply, "Intermediary", _textureIntermediary);
        _computeShader.Dispatch(idApply, _texture.width / 8, _texture.height / 8, 1);
    }

    public void UpdateInArea(Vector2Int pos, Vector2Int area, Color32 color, bool drawOver = true, bool intermediary = false)
    {
        RenderTexture textureToUse = intermediary ? ref _textureIntermediary : ref _texture;
        IgnoreOutsideScreenArea(ref pos, ref area);
        _computeShader.SetFloats("CurrentColor", ((Color)color).ToArray());
        _computeShader.SetInts("pos", new int[] { pos.x, pos.y });
        _computeShader.SetInts("size", new int[] { area.x, area.y });
        _computeShader.SetBool("drawOver", drawOver);
        _computeShader.SetTexture(idArea, "Result", textureToUse);
        _computeShader.Dispatch(idArea, _texture.width / 8, _texture.height / 8, 1);
    }

    private void IgnoreOutsideScreenArea(ref Vector2Int pos, ref Vector2Int area)
    {
        if (pos.x < 0)
        {
            area.x += pos.x;
            pos.x = 0;
        }
        else if (pos.x > _texture.width - 1)
        {
            area.x -= pos.x - (_texture.width - 1);
            pos.x = _texture.width - 1;
        }
        if (pos.y < 0)
        {
            area.y += pos.y;
            pos.y = 0;
        }
        else if (pos.y > _texture.width - 1)
        {
            area.y -= pos.y - (_texture.width - 1);
            pos.y = _texture.width - 1;
        }
        if (area.x > _texture.width - 1 - pos.x)
        {
            area.x = _texture.width - 1 - pos.x;
        }
        if (area.y > _texture.height - 1 - pos.y)
        {
            area.y = _texture.height - 1 - pos.y;
        }
    }
    private Color32[] GetPixelsInScreen(Vector2Int pos, Vector2Int area, out Vector2Int outPos, out Vector2Int outArea, out Vector2Int outFirstPixels)
    {
        int missingx = 0;
        int missingy = 0;
        if (pos.x < 0)
        {
            missingx -= pos.x;
            area.x += pos.x;
            pos.x = 0;
        }
        else if (pos.x > _texture.width - 1)
        {
            area.x -= pos.x - (_texture.width - 1);
            pos.x = _texture.width - 1;
        }
        if (pos.y < 0)
        {
            missingy -= pos.y;
            area.y += pos.y;
            pos.y = 0;
        }
        else if (pos.y > _texture.width - 1)
        {
            area.y -= pos.y - (_texture.width - 1);
            pos.y = _texture.width - 1;
        }
        if (area.x > _texture.width - 1 - pos.x)
        {
            area.x = _texture.width - 1 - pos.x;
        }
        if (area.y > _texture.height - 1 - pos.y)
        {
            area.y = _texture.height - 1 - pos.y;
        }
        outPos = pos;
        outArea = area;
        outFirstPixels = new Vector2Int(missingx, missingy);
        area.Clamp(new Vector2Int(), new Vector2Int(3840, 2160));
        return new Color32[area.x*area.y];
    }
}
