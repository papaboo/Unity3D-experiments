////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace BlobOMatic {

public static class ExtensionMethods {

    public static void SetLayerRecursively(this GameObject go, int layer) {
        go.transform.SetLayerRecursively(layer);
    }

    public static void SetLayerRecursively(this Transform t, int layer) {
        t.gameObject.layer = layer;
        foreach (Transform tt in t)
            tt.SetLayerRecursively(layer);
    }

    public static void SetHideFlagsRecursively(this GameObject go, HideFlags flags) {
        go.hideFlags = flags;
        foreach (Transform t in go.transform)
            t.gameObject.SetHideFlagsRecursively(flags);
    }

    /*
    public static void SetActiveRecursively(this GameObject go, bool active) {
        go.SetActive(active);
        foreach (Transform t in go.transform)
            t.gameObject.SetActiveRecursively(active);
    }
    */
}

} // NS BlobOMatic