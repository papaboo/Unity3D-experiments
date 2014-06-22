////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

// using UnityEditor;
using UnityEngine;

namespace BlobOMatic {

    public static class Serializer {

        public static void Float(System.IO.MemoryStream s, float v) {
            byte[] bs = System.BitConverter.GetBytes(v);
            s.Write(bs, 0, bs.Length);
        }
        
        public static void Int(System.IO.MemoryStream s, int v) {
            byte[] bs = System.BitConverter.GetBytes(v);
            s.Write(bs, 0, bs.Length);
        }
        
        public static void Bool(System.IO.MemoryStream s, bool v) {
            byte[] bs = System.BitConverter.GetBytes(v);
            s.Write(bs, 0, bs.Length);
        }
        
    }
    
    public static class Deserializer {

        public static void Float(System.IO.MemoryStream s, out float v) {
            byte[] bs = new byte[4];
            s.Read(bs, 0, 4);
            v = System.BitConverter.ToSingle(bs, 0);
        }
        
        public static void Int(System.IO.MemoryStream s, out int v) {
            byte[] bs = new byte[4];
            s.Read(bs, 0, 4);
            v = System.BitConverter.ToInt32(bs, 0);
        }
        
        public static void Bool(System.IO.MemoryStream s, out bool v) {
            byte[] bs = new byte[sizeof(bool)];
            s.Read(bs, 0, bs.Length);
            v = System.BitConverter.ToBoolean(bs, 0);
        }
        
    }
    
} // NS BlobOMatic