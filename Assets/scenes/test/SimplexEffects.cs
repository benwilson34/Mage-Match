using UnityEngine;
using System.Collections;
using System.Linq;

public class SimplexEffects : MonoBehaviour {

    private Material MyMaterial;
    public float AnimSpeed;
    public Vector3 NoiseOffset;
    public Vector3 NoiseScale;
    public Gradient Palette;

    void Start() {
        MyMaterial = GetComponent<Renderer>().material;
        UpdatePalette();
    }

    void Update() {
        MyMaterial.SetVector("_Offset", NoiseOffset);
        MyMaterial.SetVector("_Scale", NoiseScale);
        NoiseOffset.z += AnimSpeed;

        //NoiseOffset = Quaternion.Euler (new Vector3 (0.0f, AnimSpeed, 0.0f)) * NoiseOffset;

        UpdatePalette();
    }

    void UpdatePalette() {
        var mp = new MaterialPropertyBlock();
        Vector4[] colors = (from gck in Palette.colorKeys
                            select new Vector4(gck.color.r, gck.color.g, gck.color.b, 1.0f)).ToArray();

        float[] times = (from gck in Palette.colorKeys
                         select gck.time).ToArray();

        mp.SetVectorArray("_Palette", colors);
        mp.SetFloatArray("_ColourTimes", times);

        GetComponent<Renderer>().SetPropertyBlock(mp);

    }
}