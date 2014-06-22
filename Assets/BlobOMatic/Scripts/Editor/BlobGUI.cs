////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

namespace BlobOMatic {

public class BlobGUI {

    public static float LabelSize = 0.6f;

    public static void Label(Rect r, string label) {
        EditorGUI.LabelField(r, label);
    }

    public static float FloatField(Rect r, string label, float val) {
        Rect labelRect = r;
        labelRect.width *= LabelSize;
        EditorGUI.LabelField(labelRect, label);
        
        Rect valRect = r;
        valRect.width *= 1.0f - LabelSize;
        valRect.x += r.width * LabelSize;
        return EditorGUI.FloatField(valRect, val);
    }

    public static int IntField(Rect r, string label, int val) {
        Rect labelRect = r;
        labelRect.width *= LabelSize;
        EditorGUI.LabelField(labelRect, label);
        
        Rect valRect = r;
        valRect.width *= 1.0f - LabelSize;
        valRect.x += r.width * LabelSize;
        return EditorGUI.IntField(valRect, val);
    }

    public static bool Toggle(Rect r, string label, bool val) {
        float toggleWidth = 14.0f;
        Rect labelRect = r;
        labelRect.width -= toggleWidth;
        EditorGUI.LabelField(labelRect, label);
        
        Rect valRect = r;
        valRect.width = toggleWidth;
        valRect.x += labelRect.width;
        return EditorGUI.Toggle(valRect, val);
    }

}

} // NS BlobOMatic
