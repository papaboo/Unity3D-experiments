////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;

namespace BlobOMatic {

public class LocalSettings : MonoBehaviour {
    
    // Blob rendering
    public float Distance = 50.0f;
    public float Falloff = 2.0f;
    public float Strength = 50.0f;
    
    // Blurring
    public int BlurIterations = 6;
    public float BlurFactor = 0.05f;

    // Blob mesh
    public Vector2 MarginSize = new Vector2(30.0f, 30.0f);

    // Blob texture
    public float TextureSize = 128;

    public LocalSettings CopyGlobalSettings() {
        Distance = Settings.Distance;
        Falloff = Settings.Falloff;
        Strength = Settings.Strength;
        BlurIterations = Settings.BlurIterations;
        BlurFactor = Settings.BlurFactor;
        MarginSize = new Vector2(Settings.MarginSize, Settings.MarginSize);
        TextureSize = Settings.TextureSize;
        return this;
    }

    public Vector2 ComputeBlobSize(Bounds modelBounds) {
        return new Vector2(modelBounds.size.x * (1.0f + MarginSize.x / 100.0f), 
                           modelBounds.size.z * (1.0f + MarginSize.y / 100.0f));
    }
}

} // NS BlobOMatic

#endif