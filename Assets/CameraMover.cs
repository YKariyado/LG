using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMover : MonoBehaviour
{
    //cameraのtransform
    private Transform _camTransform;

    //マウス操作の始点
    private Vector3 _startMousePos;

    //カメラ回転の始点情報
    private Vector3 _presentCamRotation;

    void Start()
    {
        _camTransform = this.gameObject.transform;
    }

    void Update()
    {
        //カメラの回転 マウス
        CameraRotationMouseControl();
    }

    //カメラの回転 マウス
    private void CameraRotationMouseControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            _startMousePos = Input.mousePosition;
            _presentCamRotation.x = _camTransform.transform.eulerAngles.x;
            _presentCamRotation.y = _camTransform.transform.eulerAngles.y;
        }

        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
                //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
            float x = (_startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (_startMousePos.y - Input.mousePosition.y) / Screen.height;

            //回転開始角度 ＋ マウスの変化量 * 90
            float eulerX = _presentCamRotation.x + y * 90.0f;
            float eulerY = _presentCamRotation.y + x * 90.0f;

            _camTransform.rotation = Quaternion.Euler(eulerX, eulerY, 0);
        }
    }

}