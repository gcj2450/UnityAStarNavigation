using UnityEngine;
using System.Collections;
namespace SebLaguePathfinding
{
	public class Unit : MonoBehaviour
	{

		const float minPathUpdateTime = .2f;
		const float pathUpdateMoveThreshold = .5f;

		public Transform target;
		public float speed = 20;
		public float turnSpeed = 3;
		public float turnDst = 5;
		public float stoppingDst = 10;

		Path path;

		void Start()
		{
			StartCoroutine(UpdatePath());
		}

		public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
		{
			if (pathSuccessful)
			{
				path = new Path(waypoints, transform.position, turnDst, stoppingDst);

				StopCoroutine("FollowPath");
				StartCoroutine("FollowPath");
			}
		}

		IEnumerator UpdatePath()
		{

			if (Time.timeSinceLevelLoad < .3f)
			{
				yield return new WaitForSeconds(.3f);
			}
			PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

			float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
			Vector3 targetPosOld = target.position;

			while (true)
			{
				yield return new WaitForSeconds(minPathUpdateTime);
				print(((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
				if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
				{
					PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
					targetPosOld = target.position;
				}
			}
		}
        int pathIndex = 0;

        IEnumerator FollowPath()
		{

			bool followingPath = true;
			transform.LookAt(path.lookPoints[0]);

			float speedPercent = 1;

			while (followingPath)
			{
				Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
				while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
				{
					if (pathIndex == path.finishLineIndex)
					{
						followingPath = false;
						break;
					}
					else
					{
						pathIndex++;
					}
				}

				if (followingPath)
				{

					if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
					{
						speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
						if (speedPercent < 0.01f)
						{
							followingPath = false;
						}
					}

					Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
					transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
					transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
				}

				yield return null;

			}
		}

        public void OnDrawGizmos()
        {
            if (path != null )
            {
                for (int i = pathIndex; i < path.lookPoints.Length; i++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(path.lookPoints[i], Vector3.one);

                    if (i == pathIndex)
                    {
                        Gizmos.DrawLine(transform.position, path.lookPoints[i]);
                    }
                    else
                    {
                        Gizmos.DrawLine(path.lookPoints[i - 1], path.lookPoints[i]);
                    }
                }
            }
        }

        //public void OnDrawGizmos()
        //{

        //          if (path != null)
        //          {
        //              for (int i = pathIndex; i < path.lookPoints.Length; i++)
        //              {
        //                  Gizmos.color = Color.black;
        //                  Gizmos.DrawCube(path.lookPoints[i], Vector3.one);

        //                  if (i == pathIndex)
        //                  {
        //                      Gizmos.DrawLine(transform.position, path.lookPoints[i]);
        //                  }
        //                  else
        //                  {
        //                      Gizmos.DrawLine(path.lookPoints[i - 1], path.lookPoints[i]);
        //                  }
        //              }
        //          }

        //          if (path != null)
        //	{
        //		path.DrawWithGizmos();
        //	}
        //}
    }
}