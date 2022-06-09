using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Designium.ARDKHandson1.Sample1
{

    
    public class ActionManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject ballPrefab;

        [SerializeField]
        private Camera mainCamera;

        void Start()
        {   
        }

        void Update()
        {


    #if UNITY_EDITOR
            //
            // UnityEditorで動かしてるとき
            //
            if(Input.GetMouseButtonDown(0)){
                //
                // 画面がクリックされた時の処理
                //
                FireBall();
            }                
    #else
            //
            // ビルドして実機で動かしてるとき
            //
            if(Input.touchCount>0){
                Touch touch0 = Input.GetTouch(0);
                if(touch0.phase == TouchPhase.Began){
                    //
                    // 画面がタッチされた時の処理
                    //
                    FireBall();
                }
            }
    #endif
        }

        
        /// <summary>
        /// ボールオブジェクトを生成して飛ばします。
        /// </summary>
        private void FireBall(){
            float power = 2.0f;//ボールを飛ばす強さ
            GameObject ballObject = GameObject.Instantiate(ballPrefab, mainCamera.transform.position, Quaternion.identity);
            ballObject.GetComponent<Rigidbody>().AddForce(mainCamera.transform.forward* power);
        }
    }
}