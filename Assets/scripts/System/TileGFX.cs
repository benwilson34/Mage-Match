using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TileGFX : MonoBehaviour {

    public enum GFXState { None = 0, PrereqGlowing, TargetAvailGlowing, TargetChosenGlowing, Faded };
    public GFXState glowState = GFXState.None; // private?

    public float GlowRange {
        get { return _glowMat.GetFloat("_Range"); }
        set { _glowMat.SetFloat("_Range", value); }
    }
    public Color GlowInnerColor {
        get { return _glowMat.GetColor("_InnerColor"); }
        set { _glowMat.SetColor("_InnerColor", value); }
    }

    private const int GLOW_SORTINGORDER = -1; // will be changed to something like "background glow"
    private const int NORMALTB_SORTINGORDER = 0, 
                      FADEDTB_SORTINGORDER = -2;
    private const float GLOW_RANGE_MIN = 1.5f, GLOW_RANGE_MAX = 0.75f;
    private const float TINT_DUR = .15f;
    private const float TRANS_DUR = .12f;

    private Color SpriteTintColor {
        get { return _spriteMat.GetColor("_TintColor"); }
        set { _spriteMat.SetColor("_TintColor", value); }
    }
    private Color SpriteFadeColor {
        get { return _spriteMat.GetColor("_FadeColor"); }
        set { _spriteMat.SetColor("_FadeColor", value); }
    }

    //private static MageMatch _mm;
    private Material _spriteMat, _glowMat;

    private const string PREREQ_AVAIL_COLOR = "#003CB065", 
                         TARGET_AVAIL_COLOR = "#9C9948A5", 
                         TARGET_CHOSEN_COLOR = "#C15715B0", 
                         FADE_COLOR = "#A1A1A1A1";
    private Color _prereqAvailColor, _targetingAvailColor, _targetingChosenColor, _fadeColor;
    //private bool _spriteColorChanged = false;
    private SpriteRenderer _rend;

    void Start() {
        _glowMat = transform.Find("Glow").GetComponent<SpriteRenderer>().material;
        _prereqAvailColor = new Color();
        ColorUtility.TryParseHtmlString(PREREQ_AVAIL_COLOR, out _prereqAvailColor);
        _targetingAvailColor = new Color();
        ColorUtility.TryParseHtmlString(TARGET_AVAIL_COLOR, out _targetingAvailColor);
        _targetingChosenColor = new Color();
        ColorUtility.TryParseHtmlString(TARGET_CHOSEN_COLOR, out _targetingChosenColor);
        _fadeColor = new Color();
        ColorUtility.TryParseHtmlString(FADE_COLOR, out _fadeColor);
        _rend = GetComponent<SpriteRenderer>();
        _spriteMat = _rend.material;
    }

    public void ChangeState(GFXState newState) {
        if (newState == glowState) // if it's the same that it already is, return
            return;

        StartCoroutine(_ChangeState(newState));
        glowState = newState;
    }

    IEnumerator _ChangeState(GFXState newState) {
        // default values are what hex with GFX.None would have
        Color glowColor = Color.clear, spriteFadeColor = Color.white;
        float glowRange = GLOW_RANGE_MIN;
        int sortingOrder = NORMALTB_SORTINGORDER;

        // IN block - assume each is coming from GFXState.None
        switch (newState) {
            //case GFXState.None:
                // reset glow
                //glowColor = Color.clear;
                //glowRange = GLOW_RANGE_MIN;
                //if (glowState == GlowState.Faded) // if it was just faded, reset color
                //    spriteFadeColor = Color.white;
                //sortingOrder = NORMALTB_SORTINGORDER;
                //break;

            case GFXState.PrereqGlowing:
                glowColor = _prereqAvailColor;
                glowRange = GLOW_RANGE_MAX;
                break;

            case GFXState.TargetAvailGlowing:
                glowColor = _targetingAvailColor;
                glowRange = GLOW_RANGE_MAX;
                break;

            case GFXState.TargetChosenGlowing:
                glowColor = _targetingChosenColor;
                glowRange = GLOW_RANGE_MAX;
                break;

            case GFXState.Faded:
                //glowColor = Color.clear; // default...
                sortingOrder = FADEDTB_SORTINGORDER;
                spriteFadeColor = _fadeColor;
                break;

            default:
                break;
        }

        // finally tween the values
        DOTween.To(() => GlowInnerColor, (x) => GlowInnerColor = x, glowColor, TRANS_DUR);
        DOTween.To(() => GlowRange, (x) => GlowRange = x, glowRange, TRANS_DUR);
        DOTween.To(() => SpriteFadeColor, (x) => SpriteFadeColor = x,
                        spriteFadeColor, TRANS_DUR);
        _rend.sortingOrder = sortingOrder;

        yield return null;
    }

    public IEnumerator _AnimateTint(Color tintColor) {
        if (!tintColor.Equals(Color.white)) {
            float[] tintHSV = new float[3];
            const float satOvershoot = .4f;
            Color.RGBToHSV(tintColor, out tintHSV[0], out tintHSV[1], out tintHSV[2]);
            Color overshootColor = Color.HSVToRGB(tintHSV[0], tintHSV[1] + satOvershoot, tintHSV[2]);

            yield return DOTween.To(() => SpriteTintColor, (x) => SpriteTintColor = x, 
                overshootColor, TINT_DUR).WaitForCompletion();
        }

        yield return DOTween.To(() => SpriteTintColor, (x) => SpriteTintColor = x, 
            tintColor, TINT_DUR).WaitForCompletion();
    }

    //public static void Init(MageMatch mm) {
    //    _mm = mm;
    //}

    public static void SetGlowingTiles(List<TileSeq> seqs, GFXState state) {
        var tiles = new Dictionary<string, TileBehav>();
        foreach (var seq in seqs) {
            foreach (var tile in seq.sequence) {
                string coord = tile.PrintCoord();
                if (!tiles.ContainsKey(coord))
                    tiles.Add(coord, HexGrid.GetTileBehavAt(tile.col, tile.row));
            }
        }
        SetGlowingTiles(new List<TileBehav>(tiles.Values), state);
    }

    public static void SetGlowingTiles(List<TileBehav> glowTBs, GFXState state) {
        var tbs = new List<TileBehav>(glowTBs);
        foreach (TileBehav tb in HexGrid.GetPlacedTiles()) {
            bool glowThisTile = false;
            for (int i = 0; i < tbs.Count; i++) {
                var glowTB = tbs[i];
                if (glowTB.hextag == tb.hextag) {
                    glowThisTile = true;
                    tbs.RemoveAt(i);
                    break;
                }
            }

            var glowDriver = tb.GetComponent<TileGFX>();
            if (glowThisTile) {
                glowDriver.ChangeState(state);
            } else {
                glowDriver.ChangeState(GFXState.Faded);
            }

        }
    }

    public static void ClearGlowingTiles() {
        foreach (var tb in HexGrid.GetPlacedTiles()) {
            var driver = tb.GetComponent<TileGFX>();
            driver.ChangeState(GFXState.None);
        }
    }
}
