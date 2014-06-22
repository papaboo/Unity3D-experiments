////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

namespace BlobOMatic {

/**
 * Setttings serializer. Not part of the engine Settings class so I can use
 * System.IO.File.WriteAllBytes.
 */
public class SettingsSaver {

    static string localSavePath = "/BlobOMatic/Settings.bin";
    static string SavePath { get { return Application.dataPath + localSavePath; }}

    public static void Save(System.IO.MemoryStream stream) {
        System.IO.File.WriteAllBytes(SavePath, stream.ToArray());
    }

    public static bool DoesSaveFileExist() {
        return System.IO.File.Exists(SavePath);
    }

    public static System.IO.MemoryStream Restore() {
        try {
            byte[] bytes = System.IO.File.ReadAllBytes(SavePath);
            return new System.IO.MemoryStream(bytes);
            
        } catch (System.Exception e) {
            Debug.LogError("Could not load settings from file at '" + SavePath + "'. Exception: " + e.ToString());
        }
        
        return null;
    }
    
}

} // NS BlobOMatic