using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climb
{
	public class FreeClimbAnimHook : MonoBehaviour
	{

		#region variables
		public float wallOffset = 0.1f;
		public float w_rh;
		public float w_lh;
		public float w_rf;
		public float w_lf;

		private Animator animator;
		private IKSnapshot ikBase;
		private IKSnapshot current = new IKSnapshot();
		private IKSnapshot next = new IKSnapshot();
		private Vector3 rh, lh, rf, lf;
		private Transform helper;
		#endregion

		#region unity_methods
		private void OnAnimatorIK ( int layerIndex )
		{
			SetIKPosition(AvatarIKGoal.LeftHand, lh, w_lh);
			SetIKPosition(AvatarIKGoal.RightHand, rh, w_rh);
			SetIKPosition(AvatarIKGoal.LeftFoot, lf, w_lf);
			SetIKPosition(AvatarIKGoal.RightFoot, rf, w_rf);
		}
		#endregion

		#region public_methods
		public void Init ( FreeClimb c )
		{
			animator = c.animator;
			ikBase = c.baseIKsnapshot;
			helper = c.Helper;
		}

		public IKSnapshot CreateSnapshot ( Vector3 o )
		{
			return new IKSnapshot
			{
				lh = GetActualPosition(LocalToWorld(ikBase.lh)),
				rh = GetActualPosition(LocalToWorld(ikBase.rh)),
				lf = GetActualPosition(LocalToWorld(ikBase.lf)),
				rf = GetActualPosition(LocalToWorld(ikBase.rf))
			};
		}

		private Vector3 GetActualPosition(Vector3 origin)
		{
			Vector3 o = origin;
			Vector3 dir = helper.forward;
			o += (dir * 0.2f);

			RaycastHit hit;
			if(Physics.Raycast(o, dir, out hit, 1.5f))
			{
				return hit.point + (hit.normal * wallOffset);
			}

			return origin;
		}

		public void CreatePositions(Vector3 origin)
		{
			IKSnapshot ik = CreateSnapshot(origin);
			CopySnapshot(ref current, ik);

			UpdateIKPoisiton(AvatarIKGoal.LeftFoot, current.lf);
			UpdateIKPoisiton(AvatarIKGoal.RightFoot, current.rf);
			UpdateIKPoisiton(AvatarIKGoal.LeftHand, current.lh);
			UpdateIKPoisiton(AvatarIKGoal.RightHand, current.rh);

			UpdateIKWeight(AvatarIKGoal.LeftFoot, 1);
			UpdateIKWeight(AvatarIKGoal.RightFoot, 1);
			UpdateIKWeight(AvatarIKGoal.LeftHand, 1);
			UpdateIKWeight(AvatarIKGoal.RightHand, 1);
		}

		public void CopySnapshot ( ref IKSnapshot to, IKSnapshot from )
		{
			to.rh = from.rh;
			to.lh = from.lh;
			to.rf = from.rf;
			to.lf = from.lf;
		}

		public void UpdateIKPoisiton ( AvatarIKGoal goal, Vector3 pos )
		{
			switch ( goal )
			{
				case AvatarIKGoal.LeftFoot:
					lf = pos;
					break;
				case AvatarIKGoal.RightFoot:
					rf = pos;
					break;
				case AvatarIKGoal.LeftHand:
					lh = pos;
					break;
				case AvatarIKGoal.RightHand:
					rh = pos;
					break;
				default: throw new System.Exception();
			}
		}

		public void UpdateIKWeight ( AvatarIKGoal goal, float w )
		{
			switch ( goal )
			{
				case AvatarIKGoal.LeftFoot:
					w_lf = w;
					break;
				case AvatarIKGoal.RightFoot:
					w_rf = w;
					break;
				case AvatarIKGoal.LeftHand:
					w_lh = w;
					break;
				case AvatarIKGoal.RightHand:
					w_rh = w;
					break;
			}
		}
		#endregion

		#region private_methods
		private Vector3 LocalToWorld ( Vector3 p )
		{
			Vector3 r = helper.position;
			r += helper.right * p.x;
			r += helper.forward * p.z;
			r += helper.up * p.y;
			return r;
		}

		private void SetIKPosition(AvatarIKGoal goal, Vector3 tp, float w)
		{
			animator.SetIKPositionWeight(goal, w);
			animator.SetIKPosition(goal, tp);
		}
		#endregion
	}
}