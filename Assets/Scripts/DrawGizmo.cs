using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGizmo : MonoBehaviour
{

	#region variables
	public Color color = Color.red;
	public float size = 0.3f;
	#endregion

	#region unity_methods
	private void OnDrawGizmosSelected ()
	{
		Draw();
		var currentTransform = transform;
		while ( currentTransform.parent != null )
		{
			currentTransform = currentTransform.parent;
			if ( currentTransform.GetComponent<DrawGizmo>() != null )
			{
				currentTransform.GetComponent<DrawGizmo>().Draw();
			}
		}

		if ( transform.parent == null ) return;
		for ( int i = 0 ; i < transform.parent.childCount ; i++ )
		{
			if(i != transform.GetSiblingIndex())
			{
				transform.parent.GetChild(i).GetComponent<DrawGizmo>().Draw();
			}
		}
	}
	#endregion

	#region public_methods
	public void Draw ()
	{
		Gizmos.color = color;
		Gizmos.DrawSphere(transform.position, size);
	}
	#endregion

	#region private_methods

	#endregion
}
