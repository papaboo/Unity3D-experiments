using UnityEngine;
using System.Collections;

namespace Utils {

public class Screenshot : MonoBehaviour {

    public static string ScreenshotPath() {
        return Application.dataPath + "/screenshots/" + 
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
    }

    void LateUpdate() {
        if (Input.GetKeyUp(KeyCode.S)) {
            int resWidth = 4096, resHeight = 4096;
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
        }
    }

    private Texture2D DownSample(Texture2D src) {
        int width = 512, height = 512;
        Texture2D dst = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] cs = new Color[width * height];
        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y) {
                int dstIndex = x + y * width;
                Color dstColor = Color.black;

                int blanks = 0;
                for (int subX = 0; subX < 8; ++subX) 
                    for (int subY = 0; subY < 8; ++subY) {
                        Color srcColor = src.GetPixel(x * 8 + subX, y * 8 + subY);
                        if (srcColor == Color.white) 
                            ++blanks;
                        else 
                            dstColor += srcColor;
                    }
                
                dstColor /= 64.0f-blanks;
                dstColor.a = (64.0f-blanks) / 64.0f;
                
                cs[dstIndex] = dstColor;
            }
        dst.SetPixels(cs);
        dst.Apply();
        
        return dst;
    }
    
}

} // NS Utils