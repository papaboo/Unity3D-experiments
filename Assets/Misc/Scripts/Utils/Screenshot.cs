using UnityEngine;
using System.Collections;

namespace Utils {

public class Screenshot : MonoBehaviour {

    public static string ScreenshotPath() {
        return Application.dataPath + "/" + 
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
    }

    void LateUpdate() {
        if (Input.GetKeyUp(KeyCode.S)) {
#if !UNITY_WEBPLAYER
            int resWidth = 4096, resHeight = 2048;
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0, false);
            
            camera.targetTexture = null;
            RenderTexture.active = prevActive;
            Destroy(rt);

            screenShot = DownSample(screenShot);

            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenshotPath();
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log("Screenshot stored at: '" + filename + "'");
#else
            Debug.LogWarning("Screen shot not supported in WebPlayer.");
#endif
        }
    }

    private Texture2D DownSample(Texture2D src) {
        int width = 1024, height = 512;
        Texture2D dst = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] cs = new Color[width * height];
        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y) {
                int dstIndex = x + y * width;
                Color dstColor = Color.black;

                for (int subX = 0; subX < 4; ++subX) 
                    for (int subY = 0; subY < 4; ++subY) {
                        Color srcColor = src.GetPixel(x * 4 + subX, y * 4 + subY);
                        dstColor += srcColor;
                    }
                
                dstColor /= 16.0f;
                dstColor.a = 1.0f;
                
                cs[dstIndex] = dstColor;
            }
        dst.SetPixels(cs);
        dst.Apply();
        
        return dst;
    }
    
}

} // NS Utils