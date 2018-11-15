using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
	public float Y_ANGLE_MIN = -80.0f;
	public float Y_ANGLE_MAX = 80.0f;

	private Transform lookAt;
    private PController pController;

    private Vector3 lastArmRot;

	public float distance = 5.0f;
	private float currentX = 0.0f;
	private float currentY = 0.0f;
    private float sens=7;

    

	private void Start()
	{
    }

    private void Update()
    {
        currentX += Input.GetAxis("Mouse X") * sens;
        currentY -= Input.GetAxis("Mouse Y") * sens;
    
        currentY = Mathf.Clamp(currentY,Y_ANGLE_MIN,Y_ANGLE_MAX);
	}

    public void setLookAt(Transform l){
        lookAt=l;
        pController=l.GetComponent<PController>();
    }


	private void LateUpdate()
	{
            if(lookAt!=null){
                Vector3 offset=new Vector3(0,1.3f,0);
                RaycastHit hit;
                Quaternion rotation=Quaternion.Euler(currentY,currentX,0);
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + lookAt.position + offset;
                
                if(Physics.Linecast(lookAt.position + offset,position,out hit)){
                    if(hit.transform.gameObject.tag!="shot"){
                        Vector3 hitPoint=new Vector3(hit.point.x+hit.normal.x * .2f,hit.point.y,hit.point.z+hit.normal.z * .2f);
                        position=new Vector3(hitPoint.x,hitPoint.y+0.2f,hitPoint.z);
                    }
                }
                
                transform.rotation = rotation;
                transform.position = position;
                lookAt.rotation=Quaternion.Euler(lookAt.rotation.eulerAngles.x,rotation.eulerAngles.y,lookAt.rotation.z);
                
                if(lastArmRot!=rotation.eulerAngles && !Input.GetKey(KeyCode.LeftShift)){
                    pController.setArmRotation(rotation.eulerAngles.x);
                    lastArmRot=rotation.eulerAngles;
                }

            }
    }

    public void setSens(Slider s){
        sens=s.value;
    }


}
