////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace BlobOMatic {

public static class Settings {

    // Blob rendering
    public static float Distance = 50.0f;
    public static float Falloff = 2.0f;
    public static float Strength = 50.0f;

    // Blurring
    public static int BlurIterations = 6;
    public static float BlurFactor = 0.05f;

    // Blob mesh
    public static float MarginSize = 30.0f;

    // Blob texture
    public static float TextureSize = 128;

    // Editor
    public static bool RealtimeUpdate = false;
    public static bool ShowBounderies = false;

    public static Vector2 ComputeBlobSize(Bounds modelBounds) {
        return new Vector2(modelBounds.size.x, modelBounds.size.z) * (1.0f + MarginSize / 100.0f);
    }


    //==========================================================================
    // Serialization methods
    // TODO Ideally Serialize and deserialize would be stuffed into the same
    // method and distinguished by a flag. That way SaveToStream and
    // LoadFromStream could be combined into one method and the files would
    // always be in sync. But for now I'll manage ;)
    // ==========================================================================

    public static System.IO.MemoryStream SaveToStream() {
        System.IO.MemoryStream stream = new System.IO.MemoryStream();
        Serializer.Float(stream, Settings.Distance);
        Serializer.Float(stream, Settings.Falloff);
        Serializer.Float(stream, Settings.Strength);

        Serializer.Int(stream, Settings.BlurIterations);
        Serializer.Float(stream, Settings.BlurFactor);

        Serializer.Float(stream, Settings.MarginSize);

        Serializer.Float(stream, Settings.TextureSize);

        return stream;
    }

    public static void LoadFromStream(System.IO.MemoryStream stream) {
        if (stream == null) {
            Debug.LogWarning("Could not restore settings. Input stream was null.");
            return;
        }
        
        Deserializer.Float(stream, out Settings.Distance);
        Deserializer.Float(stream, out Settings.Falloff);
        Deserializer.Float(stream, out Settings.Strength);

        Deserializer.Int(stream, out Settings.BlurIterations);
        Deserializer.Float(stream, out Settings.BlurFactor);

        Deserializer.Float(stream, out Settings.MarginSize);

        Deserializer.Float(stream, out Settings.TextureSize);
    }

}

} // NS BlobOMatic