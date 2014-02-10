using UnityEngine;
using System.Collections;

public class GlobalEnvironment : MonoBehaviour {

    // TODO Create specialized editor view that shows the environments
    // With update button and generate convoluted button

    public Cubemap Environment;
    public Texture2D ConvolutedEnvironment;

    private Cubemap boundEnvironment = null;
    private Texture2D boundConvolutedEnvironment = null;

	void Update() {
        if (Environment != boundEnvironment) {
            Shader.SetGlobalTexture("_GlobalEnvironment", Environment);
            boundEnvironment = Environment;
        }

        if (ConvolutedEnvironment != boundConvolutedEnvironment) {
            if (ConvolutedEnvironment) {
                Shader.SetGlobalTexture("_GlobalConvolutedEnvironment", ConvolutedEnvironment);
                Shader.SetGlobalFloat("_GlobalConvolutedEnvironmentMipmapCount", (float)(ConvolutedEnvironment.mipmapCount - 2));
            } else {
                Shader.SetGlobalTexture("_GlobalConvolutedEnvironment", null);
                Shader.SetGlobalFloat("_GlobalConvolutedEnvironmentMipmapCount", 0.0f);
            }
            boundEnvironment = Environment;
        }
	}
}
