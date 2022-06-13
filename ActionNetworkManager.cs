using System.Collections;
using System.Collections.Generic;
using System.IO;
using Niantic.ARDK.AR.Networking;
using Niantic.ARDK.AR.Networking.ARNetworkingEventArgs;
using Niantic.ARDK.Networking;
using Niantic.ARDK.Networking.HLAPI.Object.Unity;
using Niantic.ARDK.Networking.MultipeerNetworkingEventArgs;
using Niantic.ARDK.Utilities.BinarySerialization;
using UnityEngine;

namespace Designium.ARDKHandson1.Sample1
{


    public class ActionNetworkManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject ballPrefab;

        [SerializeField]
        private Camera mainCamera;

        private IARNetworking _arNetworking;

        private uint dataTag = 10;

        
        private void Awake()
        {
            ARNetworkingFactory.ARNetworkingInitialized += OnARNetworkingInitialized;
        }

        

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
            if (Input.touchCount > 0)
            {
                Touch touch0 = Input.GetTouch(0);
                if (touch0.phase == TouchPhase.Began)
                {
                    //
                    // 画面がタッチされた時の処理
                    //
                    FireBall();
                }
            }
#endif
        }

        private void OnARNetworkingInitialized(AnyARNetworkingInitializedArgs args)
        {
            //
            // ネットワーク初期化完了した場合
            //
            _arNetworking = args.ARNetworking;
            _arNetworking.Networking.PeerDataReceived += OnDidReceiveDataFromPeer;
        }


        /// <summary>
        /// ボールオブジェクトを生成して飛ばします。
        /// </summary>
        private void FireBall()
        {

            if (_arNetworking == null || !_arNetworking.Networking.IsConnected)
            {
                //ネットワーク系の初期化ができてない場合は、追加処理しない
                return;
            }

            //
            // 全端末に向け手、ボール発射してほしいことを送信
            //
            // ボール発射位置と力をかける方向を送信してます。
            var data = this.SerializeBallInfo(mainCamera.transform.position, mainCamera.transform.forward);
            
            _arNetworking.Networking.BroadcastData(dataTag, data, TransportType.UnreliableUnordered, true);



        }

        private void OnDidReceiveDataFromPeer(PeerDataReceivedArgs args)
        {
            //
            // データ受信
            //


            Debug.Log("データ受信しました。");

            if(args.Tag != dataTag){
                //
                // 意図しないデータだった場合は、追加処理しない
                //
                return;
            }

            Vector3[] recieveDataArray = this.DeserializePositionAndRotation(args.CopyData());
            Vector3 ballPos = recieveDataArray[0];
            Vector3 powerDirection = recieveDataArray[1];


            float power = 2.0f;//ボールを飛ばす強さ
            GameObject ballObject = GameObject.Instantiate(ballPrefab, ballPos, Quaternion.identity);
            ballObject.GetComponent<Rigidbody>().AddForce(powerDirection * power);
        }




        //参考：https://lightship.dev/docs/ardk/multiplayer/serialization.html

        //
        // ↓では、データ送受信用にシリアライズ処理をします。
        //

        
        private  byte[] SerializeBallInfo(Vector3 ballPos, Vector3 powerDirection)
        {
            using (var stream = new MemoryStream())
            {
                using (var serializer = new BinarySerializer(stream))
                {
                    serializer.Serialize(ballPos);
                    serializer.Serialize(powerDirection);
 
                    return stream.ToArray();
                }
            }
        }

        private  Vector3[] DeserializePositionAndRotation(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                using (var deserializer = new BinaryDeserializer(stream))
                {
                    var result = new Vector3[2];
                    result[0] = (Vector3)deserializer.Deserialize(); // position
                    result[1] = (Vector3)deserializer.Deserialize(); // rotation
                                                                   
                    return result;
                }
            }
        }
    }
}
