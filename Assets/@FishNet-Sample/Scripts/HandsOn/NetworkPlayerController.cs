using System.Collections;
using FishNet.Component.Animating;
using FishNet.Component.Transforming;
using FishNet.Object;
using UnityEngine;

namespace xrdnk.FishNet_Sample.Scripts.HandsOn
{
    /// <summary>
    /// NetworkPlayer 操作用のコンポーネント
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NetworkAnimator))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkPlayerController : NetworkBehaviour
    {
        [SerializeField, Tooltip("移動係数")] float _moveRate = 4f;
        [SerializeField, Tooltip("ジャンプ係数")] float _jumpPower = 2f;

        #region プレイベートフィールド（主にキャッシュ用）
        Animator _animator;
        NetworkAnimator _networkAnimator;
        Rigidbody _rigidbody;
        #endregion

        #region 定数定義
        static readonly int MoveHorizontalAnimation = Animator.StringToHash("MoveX");
        static readonly int MoveVerticalAnimation = Animator.StringToHash("MoveZ");
        static readonly int JumpAnimation = Animator.StringToHash("Jump");
        static readonly string HorizontalInput = "Horizontal";
        static readonly string VerticalInput = "Vertical";
        #endregion

        /// <summary>
        /// 接続時の初期化処理
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            ValidateComponents();

            // NOTE: TimeManager 側の OnUpdate の際に実行する処理を登録する
            // MonoBehaviour の Update() で呼び出すことも可能だが，
            // OnStartClient の処理が完了後に OnUpdate の処理を実行することをおすすめする
            // OnStartClient と TimeManager.OnUpdate は NetworkBehaviour 側のライフサイクルに従っていて，
            // Update() は MonoBehaviour 側のライフサイクルに従っているため，タイミングが崩れることがある
            // NetworkBehaviour を継承している時は MonoBehaviour の Start()，Update() を利用するのではなく，
            // NetworkBehaviour.OnStartClient() や TimeManager.OnUpdate() を利用しよう
            // TODO: TimeManager.OnUpdate を利用して NetworkBehaviour サイフサイクルに準拠した Update 処理を登録しよう

        }

        /// <summary>
        /// コンポーネントアタッチとバリデーション
        /// </summary>
        void ValidateComponents()
        {
            if (!TryGetComponent(out _animator))
            {
                Debug.Log("Animator コンポーネントがアタッチされていません");
            }

            if (!TryGetComponent(out _networkAnimator))
            {
                Debug.Log("NetworkAnimator コンポーネントがアタッチされていません");
            }

            if (!TryGetComponent(out _rigidbody))
            {
                Debug.Log("Rigidbody コンポーネントがアタッチされていません");
            }
        }

        /// <summary>
        /// 切断時の終端処理
        /// </summary>
        public override void OnStopClient()
        {
            // TODO: TimeManager.OnUpdate を利用して NetworkBehaviour サイフサイクルに準拠した Update 処理を解除しよう

            base.OnStopClient();
        }

        /// <summary>
        /// OnUpdate の際に実行する処理
        /// </summary>
        void OnUpdate()
        {
            // オーナー以外の場合は以下の処理は実行させない
            if (!IsOwner)
            {
                return;
            }

            Locomote();

            // スペース入力時にジャンプ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(JumpRoutine());
            }
        }

        /// <summary>
        /// 歩行移動処理
        /// </summary>
        void Locomote()
        {
            var hor = Input.GetAxisRaw(HorizontalInput);
            var ver = Input.GetAxisRaw(VerticalInput);

            // NOTE: NetworkAnimator コンポーネントがアタッチされている場合，Trigger 型以外の Animation の SetXXXX メソッドは自動的に同期される
            _animator.SetFloat(MoveHorizontalAnimation, hor);
            _animator.SetFloat(MoveVerticalAnimation, ver);

            var direction = new Vector3(hor, 0, ver) * _moveRate * Time.deltaTime;

            // NOTE: NetworkTransform コンポーネントがアタッチされている場合，自動的に Transform の同期処理が走る
            transform.position += transform.TransformDirection(direction);
        }

        /// <summary>
        /// ジャンプ処理
        /// </summary>
        IEnumerator JumpRoutine()
        {
            // 1フレーム待機
            yield return null;

            // 現状のアニメーションステートが "Jump" になっていないのを待つ
            yield return new WaitWhile(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"));

            // FixedUpdate のタイミングで Rigidbody 関連の処理が出来るようにライフサイクルのタイミング調整
            yield return new WaitForFixedUpdate();

            _rigidbody.AddForce(transform.up * _jumpPower, ForceMode.Impulse);

            // TODO: NetworkAnimator.SetTrigger を用いて Animation の Trigger 型の同期を行おう

        }
    }
}