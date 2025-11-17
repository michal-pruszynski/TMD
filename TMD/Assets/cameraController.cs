using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

public class cameraController : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created




	public int mouseButton = 1;

	private Vector3 dragOriginWorld;
	private bool dragging;

	float scrollDelta;

	void Update()
	{
		Camera cam = Camera.main;
		if (cam == null) return;

		// Distance from camera to the world z = 0 plane:
		float z = -cam.transform.position.z;

		if (Input.GetMouseButtonDown(mouseButton))
		{
			// Save the world position under the cursor at the chosen plane.
			dragOriginWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
			dragging = true;
		}

		if (Input.GetMouseButtonUp(mouseButton))
		{
			dragging = false;
		}

		if (dragging && Input.GetMouseButton(mouseButton))
		{
			Vector3 currentWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
			Vector3 diff = dragOriginWorld - currentWorld; // how much to move the camera
			diff.z = 0f;

			cam.transform.position += diff;

			// Optional: update dragOriginWorld so dragging feels like "follow mouse"
			// dragOriginWorld = currentWorld; // (uncomment if you prefer incremental movement)
		}


		scrollDelta = -Input.mouseScrollDelta.y;
		cam.orthographicSize += scrollDelta;
		if(cam.orthographicSize < 6) { 
			cam.orthographicSize = 6;
		}
	}

	


	}
