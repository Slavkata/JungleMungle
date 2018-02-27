using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
	[RequireComponent(typeof(ThirdPersonCharacter))]
	public class AICharacterControl : MonoBehaviour
	{
		public Rigidbody rb;
		public Animator animator;
		public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
		public ThirdPersonCharacter character { get; private set; } // the character we are controlling
		public Transform target;                                    // target to aim for


		private bool isJumping = false;
		private bool isBraced = false;
		private bool animationEnded = false;
		private bool crouched = false;
		private Transform jumpPlace;
		
		public bool IsBraced
		{
			get { return isBraced; }
		}


		private void Start ()
		{
			// get the components on the object we need ( should not be null due to require component so no need to check )
			agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
			character = GetComponent<ThirdPersonCharacter>();

			agent.updateRotation = false;
			agent.updatePosition = true;
		}

		private void Update ()
		{
			if ( isBraced )
			{
				return;
			}

			if ( target != null )
				agent.SetDestination(target.position);

			if ( isJumping )
			{
				if ( !agent.pathPending
				&& agent.remainingDistance <= agent.stoppingDistance
				&& (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) )
				{
					isBraced = true;
					isJumping = false;
					StartCoroutine(BracedAnimation());
					return;
				}
			}

			if ( agent.remainingDistance > agent.stoppingDistance )
			{
				character.Move(agent.desiredVelocity, crouched, false);
			}
			else
			{
				character.Move(Vector3.zero, crouched, false);
			}
		}


		public void SetTarget ( Transform target )
		{
			this.target = target;
		}

		public void SetTargetPosition ( Vector3 position )
		{
			target.position = position;
			target.rotation = Quaternion.LookRotation(position - transform.position);
		}

		public void SetTargetPosition(Transform trans)
		{
			target.position = trans.position;
			target.rotation = trans.rotation;
		}

		public void SetJump ( Transform place )
		{
			target.position = place.GetChild(0).position;
			target.rotation = place.GetChild(0).rotation;
			jumpPlace = place;
			isJumping = true;
		}

		private IEnumerator BracedAnimation ()
		{
			/*
			rb.isKinematic = true;
			SetTargetPosition(jumpPlace.GetChild(1).position);
			agent.enabled = false;
			animator.SetBool("Braced", true);

			yield return new WaitForSeconds(.2f);

			//transform.position = animator.bodyPosition;

			animator.SetBool("Braced", false);

			yield return new WaitForSeconds(55f);

			transform.position = animator.bodyPosition;


			agent.enabled = true;
			jumpPlace = null;
			isBraced = false;
			rb.isKinematic = false;
			*/
			Time.timeScale = 0.4f;
			agent.isStopped = true;
			agent.enabled = false;
			rb.isKinematic = true;
			animator.SetBool("Braced", true);
			yield return new WaitForSeconds(0.2f);
			animator.SetBool("Braced", false);
			yield return new WaitUntil(() => animationEnded);
			transform.position = jumpPlace.GetChild(1).position;
			isBraced = false;
			agent.enabled = true;
			SetTargetPosition(jumpPlace.GetChild(1).position);
			agent.isStopped = false;
			rb.isKinematic = false;
			animationEnded = false;
			yield return new WaitForSeconds(0.5f);
			crouched = false;

		}
		public void AnimationFinished()
		{
			animationEnded = true;
		}
	}
}
