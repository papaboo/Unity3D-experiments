using UnityEditor;
using UnityEngine;
using System.Collections;

public class CreateIcon {

    [MenuItem("Biased/Create Icon")]
	public static void Create() {
        foreach(Object o in Selection.GetFiltered(typeof(Texture2D),
                                                  SelectionMode.Assets)) {
            
            int IconWidth = 128, IconHeight = 64;

            // Assume power of two highres texture.
            Texture2D highres = o as Texture2D;
            int mipmapEntry = Mathf.RoundToInt(Mathf.Log(highres.width / IconWidth, 2.0f));
            Debug.Log(highres.width + ", " + IconWidth + " => " + mipmapEntry);
            Color32[] pixels = highres.GetPixels32(mipmapEntry);
            Debug.Log("|pixels| = " + pixels.Length);

            Texture2D icon = new Texture2D(IconWidth, IconHeight, TextureFormat.RGB24, false);
            icon.SetPixels32(pixels);
            icon.Apply();

            string texPath = Application.dataPath + "/BiasedPhysics/Textures/Environments/"+highres.name+"_icon.png";
            System.IO.File.WriteAllBytes(texPath, icon.EncodeToPNG());
            AssetDatabase.Refresh();
        }
	}
}
