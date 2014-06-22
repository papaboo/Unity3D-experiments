////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace BlobOMatic {
namespace Editor {

public static class ExtensionMethods {

    public static void SetStaticRecursively(this GameObject go, bool staticVal) {
        go.isStatic = staticVal;
        foreach (Transform t in go.transform)
            t.gameObject.SetStaticRecursively(staticVal);
    }

}

} // NS Editor
} // NS BlobOMatic