using FishNet;
using FishNet.Managing;
using UnityEngine;

namespace xrdnk.FishNet_Sample.Scripts.HandsOn
{
    /// <summary>
    /// ゲームマネージャクラス
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        /// <summary>
        /// NetworkManager 参照キャッシュ用のフィールド
        /// </summary>
        NetworkManager _networkManager;

        /// <summary>
        /// ローカルプレイヤー（自分自身が操作するプレイヤー）参照キャッシュ用のフィールド
        /// </summary>
        NetworkPlayerCore _localPlayer;

        string _firstName = "ユニティ";
        string _lastName = "ちゃん";

        /// <summary>
        /// プレイヤー名の取得
        /// </summary>
        public FullName PlayerName => new()
        {
            FirstName = _firstName,
            LastName = _lastName
        };

        /// <summary>
        /// ローカルプレイヤー設定用のセッターメソッド
        /// </summary>
        /// <param name="player"></param>
        public void SetLocalPlayer(NetworkPlayerCore player)
        {
            _localPlayer = player;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Start()
        {
            // TODO: InstanceFinder を利用して NetworkManager にアクセスしよう

        }

        /// <summary>
        /// OnGUI を用いた描画処理
        /// </summary>
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 150, 220));

            // 接続時は切断用のボタンを表示する
            if (_networkManager.ClientManager.Started || _networkManager.ServerManager.Started)
            {
                StopGui();
            }
            // 未接続時は接続用のボタンを表示する
            else
            {
                ConnectGui();
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// 接続用の描画処理
        /// </summary>
        void ConnectGui()
        {
            NameFormUI();

            if (GUILayout.Button("Host"))
            {
                StartHost();
            }

            if (GUILayout.Button("Server"))
            {
                StartServer();
            }

            if (GUILayout.Button("Client"))
            {
                StartClient();
            }
        }

        /// <summary>
        /// クライアントとして接続する
        /// </summary>
        void StartClient()
        {
            if (_networkManager == null)
            {
                Debug.Log("NetworkManager がありません．");
                return;
            }

            // TODO: ClientManager を通して接続しよう

        }

        /// <summary>
        /// サーバとして接続する
        /// </summary>
        void StartServer()
        {
            if (_networkManager == null)
            {
                Debug.Log("NetworkManager がありません．");
                return;
            }

            // TODO: ServerManager を通して接続しよう

        }

        /// <summary>
        /// ホストとして接続する
        /// </summary>
        void StartHost()
        {
            // NOTE: 他のネットワークAPI では StartHost() の API が提供されているが，
            // Fish-Net の場合は提供されておらず，StartServer() と StartClient () を双方を呼ぶことで実現できる
            StartServer();
            StartClient();
        }

        /// <summary>
        /// 切断用の描画処理
        /// </summary>
        void StopGui()
        {
            NameFormUI();

            var mode = _networkManager.IsHost ? "Host" : _networkManager.IsServer ? "Server" : "Client";
            GUILayout.Label("Mode: " + mode);

            if (GUILayout.Button("ChangeName"))
            {
                // TODO : NetworkPlayerCore 側でフルネームを変更する処理を実装しよう

            }

            if (GUILayout.Button("StopConnection"))
            {
                StopConnection();
            }
        }

        /// <summary>
        /// プレイヤー名設定用の描画処理
        /// </summary>
        void NameFormUI()
        {
            GUILayout.Label("FirstName");
            _firstName = GUILayout.TextField(_firstName);

            GUILayout.Label("LastName");
            _lastName = GUILayout.TextField(_lastName);
        }

        /// <summary>
        /// 切断処理
        /// </summary>
        void StopConnection()
        {
            if (_networkManager.IsServerOnly || _networkManager.IsHost)
            {
                // TODO: ServerManager を通して切断しよう

            }
            else if (_networkManager.IsClientOnly)
            {
                // TODO: ClientManager を通して切断しよう

            }
        }
    }
}