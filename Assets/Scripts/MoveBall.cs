using UnityEngine;
using System.Collections;

public class MoveBall : MonoBehaviour {
	float speed=4f;
	public static Vector3 velocity;
	bool isDead=true;
	public static bool moveable=true;
	// Use this for initialization
	void Start () {
		velocity=new Vector3(1,1,0);
	}
	
	// Update is called once per frame
	void Update () {
		if (moveable&&!isDead) transform.position += velocity * speed * Time.deltaTime;
		if (isDead) {
			velocity = Vector3.zero;
			transform.position = new Vector3 (0, 0, 0);
			if (Input.GetMouseButtonDown(0)||(Input.touchCount>0&&Input.touches[0].phase==TouchPhase.Began)) {
				velocity = new Vector3 (1, 1, 0);
				isDead = false;
				moveable=true;
			}
		}
		if (transform.position.y > 5f || transform.position.y < -5f)
			isDead = true;
		Camera.main.transform.position = new Vector3 (transform.position.x,0,-10);
	}
	void OnCollisionEnter(Collision collision) {
		switch(collision.gameObject.name)
		{
		case "borderTop":
			velocity.y=-velocity.y;
			break;
		case "borderBottom":
			velocity.y=-velocity.y;
			break;
		case "borderSide":
			isDead=true;
			break;
		case "borderL":
			isDead=true;
			break;
		case "borderR":
			isDead=true;
			break;
		case "slider":
			velocity.x=-velocity.x;
			break;
		case "sliderL":
			velocity.x=-velocity.x;

			break;
		case "sliderR":
			velocity.x=-velocity.x;
			break;
		}
	}
}
