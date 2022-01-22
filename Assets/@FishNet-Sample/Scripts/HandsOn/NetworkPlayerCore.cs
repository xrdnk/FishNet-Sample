using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace xrdnk.FishNet_Sample.Scripts.HandsOn
{
    /// <summary>
    /// NetworkPlayer コアコンポーネント
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkPlayerCore : NetworkBehaviour
    {
        [SerializeField, Tooltip("ネームプレート表示用のテキスト")]
        TextMesh _namePlate;

        // TODO: [SyncVar] を使ってフルネームの同期を行おう
        // TODO: SyncVar に設定した変数が更新された時のコールバック（フック関数）を実装しよう
        // TODO : ServerRpc を用いた同期変数であるフルネームを変更する処理を実装しよう
        FullName _name;

        /// <summary>
        /// メインカメラオブジェクトのキャッシュ変数
        /// </summary>
        Camera _mainCamera;

        /// <summary>
        /// GameManager のキャッシュ変数
        /// </summary>
        GameManager _gameManager;

        /// <summary>
        /// 接続時の初期化処理
        /// </summary>
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            // 自分自身のみ以下のブロック処理を実行する
            if (IsOwner)
            {
                _gameManager = FindObjectOfType<GameManager>();
                _gameManager.SetLocalPlayer(this);

                // TODO : ServerRpc を用いた同期変数であるフルネームを変更する処理を実装しよう

            }

            _mainCamera = Camera.main;
            _namePlate.text = _name.ToString();

            // NOTE: TimeManager 側の OnTick の際に実行する処理を登録する
            // OnUpdate とは違い，Tick Rate 側で設定された頻度で更新がかかることに注意する
            // OnUpdate は ServerManager の FrameRate と ClientManager の FrameRate で数値が大きい方のレートが利用される
            // OnTick は TimeManager の Tick Rate が利用される．
            // TODO: TimeManager.OnTick を用いたサーバ側の Tick Rate を用いた更新イベントに OnTick 処理を登録しよう

        }

        /// <summary>
        /// 切断時の終端処理
        /// </summary>
        public override void OnStopNetwork()
        {
            // TODO: TimeManager.OnTick を用いたサーバ側の Tick Rate を用いた更新イベントに OnTick 処理を解除しよう

            base.OnStopNetwork();
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