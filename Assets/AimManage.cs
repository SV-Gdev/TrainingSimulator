using UnityEngine;
using Cinemachine;

public class AimManage : MonoBehaviour
{
    public Cinemachine.AxisState xAxis, yAxis;

    [SerializeField] Transform camerafollowpos;
    public Transform aimPos;

    [SerializeField] float aimsmoothSpeed = 20;
    [SerializeField] LayerMask aimMask;

    

    void Update()
    {
        

        xAxis.Update(Time.deltaTime);
        yAxis.Update(Time.deltaTime);

        Vector2 screencentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screencentre);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(
                aimPos.position,
                hit.point,
                aimsmoothSpeed * Time.deltaTime
            );
        }
    }

    private void LateUpdate()
    {
        

        camerafollowpos.localEulerAngles =
            new Vector3(yAxis.Value,
            camerafollowpos.localEulerAngles.y,
            camerafollowpos.localEulerAngles.z);

        transform.eulerAngles =
            new Vector3(transform.eulerAngles.x,
            xAxis.Value,
            transform.eulerAngles.z);
    }
}