using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// credit to VR with Andrew: https://www.youtube.com/c/VRwithAndrew

public class VRController : MonoBehaviour
{
    public float m_Gravity = 500.0f;
    public float m_Sensitivity = 0.5f;
    public float m_MaxSpeed = 3.0f;
    public float m_RotateIncrement = 90;

    public SteamVR_Action_Boolean m_RotatePressLeft = null;
    public SteamVR_Action_Boolean m_RotatePressRight = null;
    public SteamVR_Action_Boolean m_MovePress = null;
    public SteamVR_Action_Vector2 m_MoveValue = null;

    private float m_Speed = 0.0f;

    private CharacterController m_CharacterController = null;
    private Transform m_CameraRig = null;
    private Transform m_Head = null;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        m_CameraRig = SteamVR_Render.Top().origin;
        m_Head = SteamVR_Render.Top().head;
    }

    private void Update()
    {
        HandleHeight();
        CalculateMovement();
        SnapRotation();
    }

    private void HandleHeight()
    {
        // Get the head in local space
        float headHeight = Mathf.Clamp(m_Head.localPosition.y, 1, 2);
        m_CharacterController.height = headHeight;

        // Cut in half
        Vector3 newCenter = Vector3.zero;
        newCenter.y = m_CharacterController.height / 2;
        newCenter.y += m_CharacterController.skinWidth;

        // Move capsule in local space
        newCenter.x = m_Head.localPosition.x;
        newCenter.z = m_Head.localPosition.z;

        // Apply
        m_CharacterController.center = newCenter;
    }

    private void CalculateMovement()
    {
        // Figure out movement orientation
        Quaternion orientation = CalculateOrientation();
        Vector3 movement = Vector3.zero;

        // If not moving
        if (m_MovePress.GetStateUp(SteamVR_Input_Sources.LeftHand))
        {
            m_Speed = 0;
        }

        // If button pressed
        if (m_MovePress.GetState(SteamVR_Input_Sources.LeftHand))
        {
           // Debug.Log(m_MoveValue.axis);

            // Add, clamp
            m_Speed += m_MoveValue.axis.magnitude * m_Sensitivity;
            m_Speed = Mathf.Clamp(m_Speed, -m_MaxSpeed, m_MaxSpeed);

            // Orientation
            movement += orientation * (m_Speed * Vector3.forward);
        }

        // Gravity
        movement.y -= m_Gravity * Time.deltaTime;

        // Apply
        m_CharacterController.Move(movement * Time.deltaTime);
    }

    private Quaternion CalculateOrientation()
    {
        float rotation = Mathf.Atan2(m_MoveValue.axis.x, m_MoveValue.axis.y);
        rotation *= Mathf.Rad2Deg;

        Vector3 orientationEuler = new Vector3(0, m_Head.eulerAngles.y + rotation, 0);
        return Quaternion.Euler(orientationEuler);
    }

    private void SnapRotation()
    {
        float snapValue = 0.0f;

        if (m_RotatePressLeft.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            snapValue = -Mathf.Abs(m_RotateIncrement);
        }

        if (m_RotatePressRight.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            snapValue = Mathf.Abs(m_RotateIncrement);
        }

        transform.RotateAround(m_Head.position, Vector3.up, snapValue);
    }
}
