using UnityEngine;
using System.Collections;

public class VisualizeHemisphere : MonoBehaviour {

	void Start () {
        for (int i = 0; i < 128; ++i) {
            for (int j = 0; j < 128; ++j) {
                uint scrambleU = 5569;
                uint scrambleV = 95597;
                // Vector2 randomSample = new Vector2(Random.value, Random.value);
                // Vector2 randomSample = new Vector2(i / 128.0f, j / 128.0f);
                Vector2 randomSample = RandomSamplers.Utils.Sample02((uint)(2211 + i + j * 128), scrambleU, scrambleV);
                DistributionSample halfwaySample = CosineDistribution.Sample(randomSample);
                //DistributionSample halfwaySample = PowerCosineDistribution.Sample(randomSample, 0);
                
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = halfwaySample.Direction * 10.0f;
                sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
        }

        for (int i = 0; i < 128; ++i) {
            for (int j = 0; j < 128; ++j) {
                uint scrambleU = 5569;
                uint scrambleV = 95597;
                Vector2 randomSample = RandomSamplers.Utils.Sample02((uint)(2211 + 128 * 128 + i + j * 128), scrambleU, scrambleV);

                DistributionSample halfwaySample = CosineDistribution.Sample(randomSample);
                //DistributionSample halfwaySample = PowerCosineDistribution.Sample(randomSample, 0);
                
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(halfwaySample.Direction.x * 10.0f,
                                                        halfwaySample.Direction.y * -10.0f,
                                                        halfwaySample.Direction.z * 10.0f);
                sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
        }
	}
	
}
