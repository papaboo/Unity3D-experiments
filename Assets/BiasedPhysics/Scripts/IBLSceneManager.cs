using UnityEngine;
using System.Collections;

public class IBLSceneManager : MonoBehaviour {

    [System.Serializable]
    public class Environment {
        public Texture2D Icon;
        public Cubemap Specular;
        public Texture2D Convoluted;
        public Light[] Lights;
    }

    public Environment[] Environments;
    private int activeEnvIndex = 0;

    // Snow speeder material vars
    public Material SpeederMat;
    Color speederBaseColor;
    float speederBaseRoughness;
    Color speederCoatColor;
    float speederCoatSpecularity;
    float speederFresnelBias;

    // T-Rex material vars
    public Material tRexMat;
    Color tRexColor;
    float tRexRoughness;

    public Orbitor Orbitor;

    // Hack to get the GUI rects initialized in OnGUI and use them in Update
    Rect envGUIRect;
    Rect speederGUIRect;
    Rect tRexGUIRect;

	void Awake() {
        ApplyNewEnvironment(Environments[activeEnvIndex]);
        speederBaseColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        speederBaseRoughness = 0.45f;
        speederCoatColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        speederCoatSpecularity = 1.0f;
        speederFresnelBias = 0.04f;
        
        tRexColor = (Color)new Color32(225, 212, 192, 255);
        tRexRoughness = 0.85f;
        
        envGUIRect = speederGUIRect = tRexGUIRect = new Rect(0,0,0,0);
	}

    void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Vector2 screenPos = Input.mousePosition;
            screenPos.y = Screen.height - screenPos.y;
            if (envGUIRect.Contains(screenPos) || speederGUIRect.Contains(screenPos) || tRexGUIRect.Contains(screenPos))
                Orbitor.enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
            Orbitor.enabled = true;
    }
	
	void OnGUI() {
        if (Environments.Length > 1){ // Environment switching
            envGUIRect = new Rect(5.0f, 5.0f, 138.0f, 10.0f + 25 + Environments.Length * 69.0f);
            GUI.Box(envGUIRect, "");
            
            GUI.Label(new Rect(15, 10, 90, 25), "Environments:");
            
            // Environment switching
            for (int i = 0; i < Environments.Length; ++i) {
                float y = 35 + i * 69.0f;
                Rect r = new Rect(10f, y, 128.0f, 64.0f);
                GUI.enabled = i != activeEnvIndex;
                GUI.color = GUI.enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1.0f);
                if (GUI.Button(r, Environments[i].Icon)){ 
                    ApplyNewEnvironment(Environments[i]);
                    activeEnvIndex = i;
                }
            }

            GUI.enabled = true;
            GUI.color = Color.white;
        }

        // TODO Sampling VS Approximated Rho VS Plain Lookup

        { // Snow speeder material
            float boxWidth = 200.0f;
            float boxContentWidth = boxWidth - 10;
            float boxControlsX = Screen.width - boxWidth;
            speederGUIRect = new Rect(boxControlsX - 5.0f, 5.0f, boxWidth, 315.0f);
            GUI.Box(speederGUIRect, "");
            GUI.Label(new Rect(boxControlsX+5, 10, 90, 25), "Snow speeder:");

            { // Base
                GUI.Label(new Rect(boxControlsX+5, 35, boxContentWidth, 25), "Oren-Nayar Base");
                GUI.Label(new Rect(boxControlsX + 15, 60, boxContentWidth - 10, 25), "Red");
                speederBaseColor.r = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 65, boxContentWidth - 95, 25), speederBaseColor.r, 0.0f, 1.0f);
                GUI.Label(new Rect(boxControlsX + 15, 85, boxContentWidth - 10, 25), "Green");
                speederBaseColor.g = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 90, boxContentWidth - 95, 25), speederBaseColor.g, 0.0f, 1.0f);
                GUI.Label(new Rect(boxControlsX + 15, 110, boxContentWidth - 10, 25), "Blue");
                speederBaseColor.b = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 115, boxContentWidth - 95, 25), speederBaseColor.b, 0.0f, 1.0f);
                SpeederMat.SetColor("_Color", speederBaseColor);
                
                GUI.Label(new Rect(boxControlsX + 15, 135, boxContentWidth - 10, 25), "Roughness");
                speederBaseRoughness = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 140, boxContentWidth - 95, 25), speederBaseRoughness, 0.0f, 1.0f);
                SpeederMat.SetFloat("_Roughness", speederBaseRoughness);
            }

            { // Coat
                GUI.Label(new Rect(boxControlsX+5, 165, boxContentWidth, 25), "Blinn Coat");
                GUI.Label(new Rect(boxControlsX + 15, 190, boxContentWidth - 10, 25), "Red");
                speederCoatColor.r = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 195, boxContentWidth - 95, 25), speederCoatColor.r, 0.0f, 1.0f);
                GUI.Label(new Rect(boxControlsX + 15, 215, boxContentWidth - 10, 25), "Green");
                speederCoatColor.g = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 220, boxContentWidth - 95, 25), speederCoatColor.g, 0.0f, 1.0f);
                GUI.Label(new Rect(boxControlsX + 15, 240, boxContentWidth - 10, 25), "Blue");
                speederCoatColor.b = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 245, boxContentWidth - 95, 25), speederCoatColor.b, 0.0f, 1.0f);
                SpeederMat.SetColor("_SpecColor", speederCoatColor);
                
                GUI.Label(new Rect(boxControlsX + 15, 265, boxContentWidth - 10, 25), "Specularity");
                speederCoatSpecularity = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 270, boxContentWidth - 95, 25), speederCoatSpecularity, 0.0f, 1.0f);
                SpeederMat.SetFloat("_Shininess", speederCoatSpecularity);
            }

            GUI.Label(new Rect(boxControlsX+5, 295, 90, 25), "Fresnel bias:");
            speederFresnelBias = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 300, boxContentWidth - 95, 25), speederFresnelBias, 0.0f, 1.0f);
            SpeederMat.SetFloat("_UE4_Fresnel_bias", speederFresnelBias);
        }
        
        { // T-Rex material
            float boxWidth = 200.0f;
            float boxContentWidth = boxWidth - 10;
            float boxControlsX = Screen.width - boxWidth;
            tRexGUIRect = new Rect(boxControlsX - 5.0f, 335.0f, boxWidth, 175.0f);
            GUI.Box(tRexGUIRect, "");
            GUI.Label(new Rect(boxControlsX + 5, 340, 90, 25), "T-rex:");

            GUI.Label(new Rect(boxControlsX + 5, 365, boxContentWidth, 25), "Oren-Nayar Base");
            GUI.Label(new Rect(boxControlsX + 15, 390, boxContentWidth - 10, 25), "Red");
            tRexColor.r = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 395, boxContentWidth - 95, 25), tRexColor.r, 0.0f, 1.0f);
            GUI.Label(new Rect(boxControlsX + 15, 415, boxContentWidth - 10, 25), "Green");
            tRexColor.g = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 420, boxContentWidth - 95, 25), tRexColor.g, 0.0f, 1.0f);
            GUI.Label(new Rect(boxControlsX + 15, 440, boxContentWidth - 10, 25), "Blue");
            tRexColor.b = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 445, boxContentWidth - 95, 25), tRexColor.b, 0.0f, 1.0f);
            tRexMat.SetColor("_Color", tRexColor);
                
            GUI.Label(new Rect(boxControlsX + 15, 465, boxContentWidth - 10, 25), "Roughness");
            tRexRoughness = GUI.HorizontalSlider(new Rect(boxControlsX + 95, 470, boxContentWidth - 95, 25), tRexRoughness, 0.0f, 1.0f);
            tRexMat.SetFloat("_Roughness", tRexRoughness);
        }
	}

    // Apply new env maps
    void ApplyNewEnvironment(Environment env) {
        Shader.SetGlobalTexture("_GlobalEnvironment", env.Specular);
        
        if (env.Convoluted) {
            Shader.SetGlobalTexture("_GlobalConvolutedEnvironment", env.Convoluted);
            Shader.SetGlobalFloat("_GlobalConvolutedEnvironmentMipmapCount", (float)(env.Convoluted.mipmapCount - 2));
        } else {
            Shader.SetGlobalTexture("_GlobalConvolutedEnvironment", null);
            Shader.SetGlobalFloat("_GlobalConvolutedEnvironmentMipmapCount", 0.0f);
        }
    }
}
