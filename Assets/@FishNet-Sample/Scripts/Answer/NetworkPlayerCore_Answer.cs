using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace xrdnk.FishNet_Sample.Scripts.Answer
{
    /// <summary>
    /// NetworkPlayer コアコンポーネント
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkPlayerCore_Answer : NetworkBehaviour
    {
        [SerializeField, Tooltip("ネームプレート表示用のテキスト")]
        TextMesh _namePlate;

        /// <summary>
        /// フルネーム（同期変数）
        /// </summary>
        [SyncVar(
            // Reliable 設定｜確実に伝搬されるのを保証するため，ここでは Realiable に設定
            Channel = Channel.Reliable,
            // 読取権限設定｜全クライアントに伝搬されるのを保証するため，ここでは Observers に設定
            ReadPermissions = ReadPermission.Observers,
            // 送信レート｜デフォルトの値を設定
            SendRate = 0.1f,
            // フック関数
            OnChange = nameof(OnNameChanged)
        )]
        FullName_Answer _name;

        /// <summary>
        /// メインカメラオブジェクトのキャッシュ変数
        /// </summary>
        Camera _mainCamera;

        /// <summary>
        /// GameManager のキャッシュ変数
        /// </summary>
        GameManager_Answer _gameManager;

        /// <summary>
        /// 接続時の初期化処理
        /// </summary>
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            // 自分自身のみ以下のブロック処理を実行する
            if (IsOwner)
            {
                _gameManager = FindObjectOfType<GameManager_Answer>();
                _gameManager.SetLocalPlayer(this);
                ChangeName(_gameManager.PlayerName);
            }

            _mainCamera = Camera.main;
            _namePlate.text = _name.ToString();

            // NOTE: TimeManager 側の OnTick の際に実行する処理を登録する
            // OnUpdate とは違い，Tick Rate 側で設定された頻度で更新がかかることに注意する
            // OnUpdate は ServerManager の FrameRate と ClientManager の FrameRate で数値が大きい方のレートが利用される
            // OnTick は TimeManager の Tick Rate が利用される．
            TimeManager.OnTick += OnTick;
        }

        /// <summary>
        /// 切断時の終端処理
        /// </summary>
        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= OnTick;
            base.OnStopNetwork();
        }

        /// <summary>
        /// 名前が変更された時のフック関数
        /// </summary>
        /// <param name="prev">前の値</param>
        /// <param name="next">次の値</param>
        /// <param name="asServer">サーバかクライアントか</param>
        void OnNameChanged(FullName_Answer prev, FullName_Answer next, bool asServer)
        {
            _namePlate.text = next.ToString();
        }

        /// <summary>
        /// 名前変更処理
        /// </summary>
        /// <param name="fullName"></param>
        public void ChangeName(FullName_Answer fullName)
        {
            // ネットワークオブジェクトの所有者だけが変更出来るようにする
            if (IsOwner)
            {
                // SyncVar (同期変数) の更新権限はサーバ側のみが持つ
                // よってクライアント側が同期変数を行うためには ServerRpc を通して同期変数の変更を行う必要がある
                // サーバ側が同期変数を更新した時，ReadPermissions (ここではすべてのクライアント Observer) に
                // 設定された権限を持つ者に自動的に伝搬される
                ChangeNameServerRpc(fullName);
            }
        }

        /// <summary>
        /// サーバ側で名前の変更処理を実行する
        /// </summary>
        /// <param name="fullName">フルネーム</param>
        [ServerRpc]
        void ChangeNameServerRpc(FullName_Answer fullName)
        {
            _name = fullName;
        }

        /// <summary>
        /// サーバ上におけるシミュレーションレートで実行する処理（ネームプレート表示をカメラに向けて行う）
        /// </summary>
        void OnTick()
        {
            var lookAtPos = transform.position - _mainCamera.transform.position;
            _namePlate.transform.LookAt(lookAtPos);
            _namePlate.transform.localScale = Vector3.one * 0.25f;
        }
    }
}