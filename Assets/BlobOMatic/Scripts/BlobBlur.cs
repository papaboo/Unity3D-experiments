////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace BlobOMatic {

[ExecuteInEditMode]
public class BlobBlur : MonoBehaviour {

    public int BlurIterations = 4;
    public float Strength = 0.5f;
    public float MipOffset = 4;
    private Material blurMat;

    // The percentage of the blob shadow that is margin and should fade out.
    // Since BlobBlur has no idea about the model being rendered this should be
    // set explicitly by the model being rendered.
    public Vector2 MarginStart = Vector2.zero;
    public Vector2 MarginEnd = new Vector2(1,1);
    private Material marginFalloffMat;
    
    public void Awake() {
        blurMat = new Material(Shader.Find("BlobOMatic/BlobBlur"));
        blurMat.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
        marginFalloffMat = new Material(Shader.Find("BlobOMatic/MarginFalloff"));
        marginFalloffMat.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
    }

    public void OnDestroy() {
        DestroyImmediate(blurMat);
    }

	void OnRenderImage(RenderTexture source, RenderTexture destination) {
        marginFalloffMat.SetFloat("_BlobStrength", Strength * 0.01f);

        // Early out
        if (BlurIterations == 0) {
            Graphics.Blit(source, destination, marginFalloffMat);
            return;
        }

        blurMat.SetFloat("_Factor", MipOffset);
        
        RenderTexture buffer1 = RenderTexture.GetTemporary(source.width, source.height, 0);
		RenderTexture buffer2 = RenderTexture.GetTemporary(source.width, source.height, 0);
        buffer1.useMipMap = buffer2.useMipMap = true;
        buffer1.filterMode = buffer2.filterMode = FilterMode.Trilinear;
        
        Graphics.Blit(source, buffer1);

        bool blitToEven = true;
        for(int i = 0; i < BlurIterations; i++) {
            if (blitToEven)
                Graphics.Blit(buffer1, buffer2, blurMat);
            else
                Graphics.Blit(buffer2, buffer1, blurMat);
            blitToEven = !blitToEven;
        }
        
        // Apply margin falloff and blit to final destination
        marginFalloffMat.SetVector("_MarginPercentage", new Vector4(MarginStart.x, MarginStart.y,
                                                                    MarginEnd.x, MarginEnd.y));
        if (blitToEven)
            Graphics.Blit(buffer1, destination, marginFalloffMat);
        else
            Graphics.Blit(buffer2, destination, marginFalloffMat);

        RenderTexture.ReleaseTemporary(buffer1);
        RenderTexture.ReleaseTemporary(buffer2);
    }
}

} // NS BlobOMatic