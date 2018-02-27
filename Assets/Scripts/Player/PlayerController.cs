using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
public class PlayerController : MonoBehaviour
{
	#region variables
	[SerializeField]
	private Animator m_Animator;
	[SerializeField]
	private NavMeshAgent m_Agent;

	public float turnSmoothing = 15f;
	public float speedDampTime = 0.1f;
	public float slowingSpeed = 0.175f;
	public float turnSpeedThreshold = 0.5f;
	public float inputHoldDelay = 0.5f;

	private Vector3 destinationPosition;
	private bool handleInput = true;
	private WaitForSeconds inputHoldWait;
	private readonly int hashSpeedPara = Animator.StringToHash("Speed");
	private readonly int hashLocomotionTag = Animator.StringToHash("Locomotion");
	public const string startingPositionKey = "starting position";
	private const float stopDistanceProportion = 0.1f;
	private const float navMeshSampleDistance = 4f;
	#endregion

	#region unity_variables
	private void Start ()
	{
		if ( !m_Animator )
		{
			m_Animator = GetComponent<Animator>();
		}
		if( !m_Agent )
		{
			m_Agent = GetComponent<NavMeshAgent>();
		}
	}

	private void OnAnimatorMove ()
	{
		m_Agent.velocity = m_Animator.deltaPosition / Time.deltaTime;
	}

	#endregion
	private void Update ()
	{
		if ( m_Agent.pathPending )
			return;
		float speed = m_Agent.desiredVelocity.magnitude;

		if ( m_Agent.remainingDistance <= m_Agent.stoppingDistance * stopDistanceProportion )
			Stopping(out speed);
		else if ( m_Agent.remainingDistance <= m_Agent.stoppingDistance )
			Slowing(out speed, m_Agent.remainingDistance);
		else if ( speed > turnSpeedThreshold )
			Moving();

		m_Animator.SetFloat(hashSpeedPara, speed, speedDampTime, Time.deltaTime);
	}
	private void Stopping ( out float speed )
	{
		m_Agent.isStopped = true;
		transform.position = destinationPosition;
		speed = 0f;
	}
	private void Slowing ( out float speed, float distanceToDestination )
	{
		m_Agent.isStopped = true;
		float proportionalDistance = 1f - distanceToDestination / m_Agent.stoppingDistance;
		Quaternion targetRotation = transform.rotation;
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, proportionalDistance);
		transform.position = Vector3.MoveTowards(transform.position, destinationPosition, slowingSpeed * Time.deltaTime);
		speed = Mathf.Lerp(slowingSpeed, 0f, proportionalDistance);
	}
	private void Moving ()
	{
		Quaternion targetRotation = Quaternion.LookRotation(m_Agent.desiredVelocity);
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);
	}
	public void OnGroundClick ( Vector3 point )
	{
		if ( !handleInput )
			return;

		NavMeshHit hit;
		if ( NavMesh.SamplePosition(point, out hit, navMeshSampleDistance, NavMesh.AllAreas) )
			destinationPosition = hit.position;
		else
			destinationPosition = point;
		m_Agent.SetDestination(destinationPosition);
		m_Agent.isStopped = false;
	}

	private IEnumerator WaitForInteraction ()
	{
		handleInput = false;
		yield return inputHoldWait;
		while ( m_Animator.GetCurrentAnimatorStateInfo(0).tagHash != hashLocomotionTag )
		{
			yield return null;
		}
		handleInput = true;
	}
}