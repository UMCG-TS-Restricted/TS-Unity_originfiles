using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public abstract class Hittable : MonoBehaviour
{
	public abstract void Hit(Hitter hitter);
}
