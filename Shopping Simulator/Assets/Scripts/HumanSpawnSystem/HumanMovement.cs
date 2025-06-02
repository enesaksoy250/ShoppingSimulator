using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanMovement : MonoBehaviour
{
    public float speed = 2f;
    private Waypoint current;
    private Animator animator;

    public Waypoint nearestWaypoint;

    private void Awake()
    {
        animator = GetComponent<Animator>();    
    }

    void Start()
    {
        // En yakýn waypoint'i bul
        //Waypoint[] all = FindObjectsOfType<Waypoint>();
        //current = all.OrderBy(wp => Vector3.Distance(transform.position, wp.transform.position)).FirstOrDefault();

        current = nearestWaypoint;

        if (current != null)
            StartCoroutine(MoveRoutine());
        else
            Debug.LogError("Waypoint bulunamadý.");

        animator.SetBool("IsMoving", true);
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (current == null || current.ConnectedWaypoints.Count == 0)
            {
                HumanCountControl.Instance.DecreaseHumanCount();
                Destroy(gameObject);
                yield break;
            }

            // Sonraki noktayý seç
            Waypoint next = current.ConnectedWaypoints[Random.Range(0, current.ConnectedWaypoints.Count)];

            Vector3 start = transform.position;
            Vector3 end = next.transform.position;
            float dist = Vector3.Distance(start, end);
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation((end - start).normalized);

            float traveled = 0f;
            while (traveled < dist)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, end, step);
                traveled = Vector3.Distance(start, transform.position);

                float t = Mathf.Clamp01(traveled / dist);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);

                yield return null;
            }

            transform.position = end;
            transform.rotation = endRot;
            current = next;

            yield return null;
        }
    }
}
