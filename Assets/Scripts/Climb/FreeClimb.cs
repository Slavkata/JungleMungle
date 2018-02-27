using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climb
{
	public class FreeClimb : MonoBehaviour
	{

		#region variables
		public Animator animator;
		public float positionOffset;
		public float offsetFromWall = 0.3f;
		public float speedMultiplier = 0.5f;
		public float climbSpeed = 3;
		public float rotateSpeed = 2;
		public IKSnapshot baseIKsnapshot;
		public FreeClimbAnimHook hook;

		private bool isClimbing;
		private bool inPosition;
		private bool isLerping;
		private float delta;
		private float t;
		private float hAxis;
		private float vAxis;
		private Vector3 startPos;
		private Vector3 targetPos;
		private Quaternion startRot;
		private Quaternion targetRot;
		private Transform helper;

		public Transform Helper
		{
			get
			{
				return helper;
			}
		}
		#endregion

		#region unity_methods
		private void Start ()
		{
			Init();
		}

		private void Update ()
		{
			delta = Time.deltaTime;

			Tick();
		}
		#endregion

		#region public_methods
		public void CheckForClimb ()
		{
			Vector3 origin = transform.position;
			origin.y += 1.5f;
			Vector3 dir = transform.forward;
			RaycastHit hit;
			if ( Physics.Raycast(origin, dir, out hit, 5) )
			{
				helper.transform.position = PositionWithOffset(origin, hit.point);
				InitForClimb(hit);
			}
		}

		public void Tick ()
		{
			if ( !inPosition )
			{
				GetInPosition();
				return;
			}

			if ( !isLerping )
			{
				hAxis = Input.GetAxis("Horizontal");
				vAxis = Input.GetAxis("Vertical");
				var moveAmount = Mathf.Abs(hAxis) + Mathf.Abs(vAxis);

				Vector3 h = helper.right * hAxis;
				Vector3 v = helper.up * vAxis;
				Vector3 moveDir = (h + v).normalized;

				if(!CanMove(moveDir) || moveDir == Vector3.zero)
				{
					return;
				}

				t = 0;
				isLerping = true;
				startPos = transform.position;
				targetPos = helper.position;
				hook.CreatePositions(targetPos);
			}
			else
			{
				t += delta * climbSpeed;
				if ( t > 1 )
				{
					t = 1;
					isLerping = false;
				}

				transform.position = Vector3.Lerp(startPos, targetPos, t);
				transform.rotation = Quaternion.Lerp(transform.rotation, helper.rotation, delta * rotateSpeed);
			}
		}
		#endregion

		#region private_methods
		private void Init ()
		{
			helper = new GameObject().transform;
			helper.name = "climbHelper";

			hook.Init(this);
			CheckForClimb();
		}

		private void InitForClimb ( RaycastHit hit )
		{
			isClimbing = true;
			helper.transform.rotation = Quaternion.LookRotation(-hit.normal);
			startPos = transform.position;
			targetPos = hit.point + (hit.normal * offsetFromWall);
			t = 0;
			inPosition = false;
			animator.CrossFade("Climb Idle", 2);
		}

		private void GetInPosition ()
		{
			t += delta;

			if ( t > 1 )
			{
				t = 1;
				inPosition = true;

				hook.CreatePositions(targetPos);
			}

			transform.position = Vector3.Lerp(startPos, targetPos, t);
			transform.rotation = Quaternion.Lerp(transform.rotation, helper.rotation, delta * rotateSpeed);
		}

		private Vector3 PositionWithOffset ( Vector3 origin, Vector3 target )
		{
			Vector3 direction = origin - target;
			direction.Normalize();
			Vector3 offset = direction * offsetFromWall;
			return target + offset;
		}

		private bool CanMove ( Vector3 moveDir )
		{
			Vector3 origin = transform.position;
			float dis = positionOffset;
			Vector3 dir = moveDir;

			Debug.DrawRay(origin, dir * dis);

			RaycastHit hit;
			if ( Physics.Raycast(origin, dir, out hit, dis) )
			{
				return false;
			}

			origin += moveDir * dis;
			dir = helper.forward;
			float dis2 = 0.5f;

			Debug.DrawRay(origin, dir * dis2);

			if ( Physics.Raycast(origin, dir, out hit, dis) )
			{
				helper.position = PositionWithOffset(origin, hit.point);
				helper.rotation = Quaternion.LookRotation(-hit.normal);
				return true;
			}

			origin += dir * dis2;
			dir = -Vector3.up;

			Debug.DrawRay(origin, dir);
			if ( Physics.Raycast(origin, dir, out hit, dis2) )
			{
				float angle = Vector3.Angle(helper.up, hit.normal);
				if(angle < 40)
				{
					helper.position = PositionWithOffset(origin, hit.point);
					helper.rotation = Quaternion.LookRotation(-hit.normal);
					return true;
				}
			}

			return false;
		}
		#endregion
	}

	[System.Serializable]
	public class IKSnapshot
	{
		public Vector3 rh, lh, rf, lf;
	}
}