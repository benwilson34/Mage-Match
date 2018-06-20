using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TileGFX : MonoBehaviour {

    public enum GlowState { None = 0, Glowing, Faded };
    public GlowState glowState = GlowState.None;

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
    private const float TRANS_DUR = .12f;
    private const float RANGE_MIN = 1.5f, RANGE_MAX = 0.75f;
    private const float TINT_DUR = .15f;

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
    private bool _spriteColorChanged = false;
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

    public void ChangeGlow(GlowState newState) {
        if (newState == glowState) // if it's the same that it already is, return
            return;

        StartCoroutine(_ChangeGlow(newState));
        glowState = newState;
    }

    // TODO I'd probably rather have "in" and "out" blocks
    IEnumerator _ChangeGlow(GlowState newState) {
        switch (newState) {
            case GlowState.None:
                // reset glow
                DOTween.To(() => GlowInnerColor, (x) => GlowInnerColor = x, Color.clear, TRANS_DUR);
                DOTween.To(() => GlowRange, (x) => GlowRange = x, RANGE_MIN, TRANS_DUR);
                if (glowState == GlowState.Faded) // if it was just faded, reset color
                    DOTween.To(() => SpriteFadeColor, (x) => SpriteFadeColor = x, 
                        Color.white, TRANS_DUR);
                _rend.sortingOrder = NORMALTB_SORTINGORDER;
                break;

            case GlowState.Glowing:
                DOTween.To(() => GlowInnerColor, (x) => GlowInnerColor = x, _targetingAvailColor, TRANS_DUR);
                DOTween.To(() => GlowRange, (x) => GlowRange = x, RANGE_MAX, TRANS_DUR); 
                //if (glowState == GlowState.Faded) // if it was just faded, reset color
                //    _rend.color = _origColor; // tween?
                _rend.sortingOrder = NORMALTB_SORTINGORDER;
                break;

            case GlowState.Faded:
                GlowInnerColor = Color.clear; // tween?
                _rend.sortingOrder = FADEDTB_SORTINGORDER;
                DOTween.To(() => SpriteFadeColor, (x) => SpriteFadeColor = x,
                        _fadeColor, TRANS_DUR);
                break;

            default:
                break;
        }

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

    public static void SetGlowingTiles(List<TileSeq> seqs) {
        var tiles = new Dictionary<string, TileBehav>();
        foreach (var seq in seqs) {
            foreach (var tile in seq.sequence) {
                string coord = tile.PrintCoord();
                if (!tiles.ContainsKey(coord))
                    tiles.Add(coord, HexGrid.GetTileBehavAt(tile.col, tile.row));
            }
        }
        SetGlowingTiles(new List<TileBehav>(tiles.Values));
    }

    public static void SetGlowingTiles(List<TileBehav> glowTBs) {
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
                glowDriver.ChangeGlow(TileGFX.GlowState.Glowing);
            } else {
                glowDriver.ChangeGlow(TileGFX.GlowState.Faded);
            }

        }
    }

    public static void ClearGlowingTiles() {
        foreach (var tb in HexGrid.GetPlacedTiles()) {
            var driver = tb.GetComponent<TileGFX>();
            driver.ChangeGlow(TileGFX.GlowState.None);
        }
    }
}
