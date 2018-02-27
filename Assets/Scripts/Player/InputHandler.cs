using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class InputHandler : MonoBehaviour
{

	#region variables
	[SerializeField]
	private AICharacterControl m_AIController;
	#endregion

	#region unity_methods
	private void Start ()
	{
		if ( !m_AIController )
		{
			m_AIController = GetComponent<AICharacterControl>();
		}
	}

	private void Update ()
	{
		if ( Input.GetMouseButtonDown(0) )
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if ( Physics.Raycast(ray, out hit, 100) )
			{
				if(hit.transform.CompareTag("JumpPlace"))
				{
					m_AIController.SetJump(hit.transform);
				}
				else
				{
					m_AIController.SetTargetPosition(hit.point);
				}
			}
		}
	}
	#endregion

	#region public_methods

	#endregion

	#region private_methods

	#endregion
}
