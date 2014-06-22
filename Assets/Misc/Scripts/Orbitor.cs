using UnityEngine;
using System.Collections;

public class Orbitor : MonoBehaviour {

    public Transform Target;
    private float distanceToTarget;

    public float VerticalRotation = 30.0f;
    public float HorizontalRotation = 25.0f;

    private Vector2 previousMousePos;
    
    private Vector3 targetPos;

	void Awake() {
        distanceToTarget = (Target.position - transform.position).magnitude;

        MeshFilter[] mfs = Target.GetComponentsInChildren<MeshFilter>();
        if (mfs.Length == 0)
            targetPos = Vector3.zero;
        else {
            Bounds bounds = mfs[0].sharedMesh.bounds;
            for (int i = 0; i < mfs.Length; ++i)
                bounds.Encapsulate(mfs[i].sharedMesh.bounds);
            targetPos = bounds.center;
        }
        targetPos = Target.TransformPoint(targetPos);
	}
	
	void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            previousMousePos = Input.mousePosition;
        if (Input.GetKey(KeyCode.Mouse0)) {
            Vector2 delta = (Vector2)Input.mousePosition - previousMousePos;
            VerticalRotation += delta.x / 2.0f;
            HorizontalRotation -= delta.y / 3.0f;
            //HorizontalRotation = Mathf.Clamp(HorizontalRotation, -89.0f, 89.0f);
            HorizontalRotation = Mathf.Clamp(HorizontalRotation, 0.0f, 55.0f);
            previousMousePos = Input.mousePosition;
        }

        distanceToTarget *= 1.0f / (1.0f + Input.GetAxis("Mouse ScrollWheel"));

        Quaternion rot = Quaternion.AngleAxis(VerticalRotation, Vector3.up) * Quaternion.AngleAxis(HorizontalRotation, Vector3.right);
        Vector3 offset = - new Vector3(0,0,-distanceToTarget);
        transform.position = targetPos - rot * offset;
        transform.rotation = rot;
	}
}
