using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    List<Vector3> points = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();

        
        
        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }

        if (Input.GetButton("Fire2"))
        {
            DisplayProjectilePath();
        }

        if (Input.GetButtonUp("Fire2"))
        {
            
        }
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
        {
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0);
        }
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = Mathf.Abs(gravity.y);
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                return (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
        }
        else
            return null;
    }

    void DisplayProjectilePath()
    {
        //Clear the list of positions each frame and add the barrel end as the first position
        points.Clear();
        points.Add(barrelEnd.position);

        //velocity is speed * direction
        Vector3 velocity = projectileSpeed * barrelEnd.forward;
        //Create a time interval
        float interval = 0.02f;
        //Create a loop in order to get a list of positions along the path
        for (float t = 0; t < 1f; t += interval)
        {
            Vector3 pos = barrelEnd.position;
            Vector3 d = (velocity * t) + (1f / 2f * gravity * Mathf.Pow(t,2));
            pos += d;
            //Add the new location to list
            points.Add(pos);

            //Check to see if our mouse is not colliding with a surface
            if (Physics.Raycast(pos, d, out RaycastHit hit, projectileSpeed*interval))
            {
                break;
            }
        }

        //set the amount of lines for our renderer to the amount of indexes from our list
        line.positionCount = points.Count;
        
        //Set each line index equal to the position from the list
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}
