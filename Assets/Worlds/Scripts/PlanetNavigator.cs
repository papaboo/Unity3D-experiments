using UnityEngine;
using System.Collections;

public class PlanetNavigator : MonoBehaviour {

    public Planet.Land planet;

    public Quaternion Position = Quaternion.identity;
    public float HorizontalRotation = 30.0f; // Clamped between 15 and 45 degrees

    private Vector2 previousMousePos;
    private float zoomLevel = 0.0f; // between 0 and 1, starts zoomed in
    private enum Zoom { In, Out }
    private Zoom zoomState = Zoom.In;
    private const float ZOOM_TIME = 1.5f;

	void Awake() {
        if (planet == null) // Do expensive search for planet
            planet = Object.FindObjectOfType(typeof(Planet.Land)) as Planet.Land;
	}
	
	void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && 
            (zoomLevel == 0.0f || zoomLevel == 1.0f)) {
            float target = zoomState == Zoom.In ? 1.0f : 0.0f;
            StartCoroutine(ZoomOut(target));
        }
        
        float forward = 20.0f * Time.deltaTime * ((Input.GetKey(KeyCode.UpArrow) ? 1.0f : 0.0f) - 
                                                  (Input.GetKey(KeyCode.DownArrow) ? 1.0f : 0.0f));

        float strafe = 20.0f * Time.deltaTime * ((Input.GetKey(KeyCode.LeftArrow) ? 1.0f : 0.0f) - 
                                                 (Input.GetKey(KeyCode.RightArrow) ? 1.0f : 0.0f));
        
        float roll = 0.0f;
        if (Input.GetKeyDown(KeyCode.Mouse0))
            previousMousePos = Input.mousePosition;
        if (Input.GetKey(KeyCode.Mouse0)) {
            Vector2 delta = (Vector2)Input.mousePosition - previousMousePos;
            delta *= Mathf.Max(0.0f, (1.0f - zoomLevel * 4.0f)); // reduce based on zoomlevel
            roll = delta.x / -2.0f;
            HorizontalRotation += delta.y / 3.0f;
            HorizontalRotation = Mathf.Clamp(HorizontalRotation, 15.0f, 45.0f);
            previousMousePos = Input.mousePosition;
        }
        
        // TODO There has to be a faster way to construct a quaternion from roll pitch and yaw!!
        Quaternion globalRot = Quaternion.AngleAxis(forward, Vector3.right) * Quaternion.AngleAxis(strafe, Vector3.up) * Quaternion.AngleAxis(roll, Vector3.forward);
        Position *= globalRot;

        Quaternion localRot = Quaternion.AngleAxis(-HorizontalRotation, Vector3.right);

        float normDistance = Mathf.Lerp(1.1f, 2.5f, zoomLevel);
        transform.position = Position * new Vector3(0, 0, planet.MaxHeight * -normDistance);
        transform.rotation = Position * Quaternion.Slerp(localRot, Quaternion.identity, zoomLevel);
	}

    IEnumerator ZoomOut(float target) {
        // TODO Enable zoom while zooming while zooming while zooming that 'preserves' velocity

        zoomState = target < 0.5f ? Zoom.In : Zoom.Out;

        float startZoom = zoomLevel;
        float t = 0.0f;
        while (t < 1.0f) {
            zoomLevel = Mathf.SmoothStep(startZoom, target, t);
            t += Time.deltaTime / ZOOM_TIME;
            yield return null;
        }

        zoomLevel = target; 
    }
}
