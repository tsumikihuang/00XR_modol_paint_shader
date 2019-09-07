using UnityEngine;
using System.Collections;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;
    private float distance = 30.0f;

    [Header("Drag Parameter")]
    public float xSpeed = 70.0f;
    public float ySpeed = 50.0f;
    public float yMinLimit = 0f;
    public float yMaxLimit = 90f;   
    
    private float x = 0.0f;
    private float y = 0.0f;

    private float fx = 0f;
    private float fy = 0f;
    private float fDistance = 0;

    [Header("ScrollWheel Parameter")]
    public float zoomSpeed = 0.5f;
    public float minDistance = 10f;
    public float maxDistance = 50.0f;
    private float m_targetDistance = 0;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;     //當前物體也就是攝影機的角度
        x = angles.y;
        y = angles.x;
        fx = x;
        fy = y; 
        m_targetDistance = distance;
        UpdateRotaAndPos();
        fDistance = distance;
    }
   
    void LateUpdate()   //先执行Updatee然后执行lateUpdate，整脚本执行顺序。例如:当物体在Update里移动时，跟随物体的相机可以在LateUpdate里实现
    {
        //旋轉視野角度
        if (Input.GetMouseButton(1) && Input.touchCount < 2)
        {
            //Debug.Log(Input.touchCount);  //為0
            if (target)
            {
                float dx = Input.GetAxis("Mouse X");    //鼠标沿着屏幕X移动时触发，x之變化值
                float dy = Input.GetAxis("Mouse Y");    //鼠标沿着屏幕Y移动时触发，y之變化值

                //似乎只有平板手機等螢幕觸控才進來
                if (Input.touchCount > 0)
                {
                    dx = Input.touches[0].deltaPosition.x;
                    dy = Input.touches[0].deltaPosition.y;
                }

                x += dx * xSpeed * Time.deltaTime; //*distance
                y -= dy * ySpeed * Time.deltaTime;

                //範圍限制
                y = ClampAngle(y, yMinLimit, yMaxLimit);
                
            }
        }

        OnMouseWheel();

        fx = Mathf.Lerp(fx,x,0.2f);
        fy = Mathf.Lerp(fy,y,0.2f);

        UpdateRotaAndPos();
    }    

    //放大縮小
    private void OnMouseWheel()
    {
        if(target)
        {
            float wheelValue = Input.GetAxis("Mouse ScrollWheel");
            m_targetDistance -= wheelValue * zoomSpeed * 400 * Time.deltaTime;
            m_targetDistance = ClampAngle(m_targetDistance,minDistance,maxDistance);
            distance = ClampAngle(Mathf.Lerp(distance,m_targetDistance,0.2f),minDistance,maxDistance);
        }
    }

    void UpdateRotaAndPos()
    {
        if (target)
        {
            Quaternion rotation = Quaternion.Euler(fy, fx, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);

            /* Quaternion * Vector3    >>  下個位置變化方向，Vector3进行一次Quaternion 旋转
             * 
             * 第一人称射击类，当玩家按住向右移动时，人物的旋转是不变的，只是移动。方向相对于玩家是向右
             * 假设directionVector=(1,0,1);
             * 
             * 假设transform.rotation对应的欧拉角度为 (0,0,0) ; 即没有旋转，
             * 则transform.rotation*directionVector得到的结果还是(1,0,1);
             * 
             * 假设transform.rotation对应的欧拉角度为 (0,45,0); 即当前人物相对于世界坐标系是有一个45度旋转量
             * 即当玩家同时按下向前和向右移动的按键时，人物将向正右前方向移动，显然我们直观判断人物移动的方向向量应该是(1,0,0);
             * 
             * ransform.rotation * directionVector 就是
             * directionVector 绕Y轴旋转45度，(1,0,1)绕Y轴旋转45度不就是(1,0,0)
             * 
             */
            Vector3 position = rotation * negDistance + target.position;    //加上當前位置就是下一個位置

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}