using FishNet.Object;
using UnityEngine;

namespace FishNet_Sample.Scripts
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkPlayerController : NetworkBehaviour
    {
        [SerializeField]
        float _moveRate = 4f;
        [SerializeField]
        float _jumpPower = 2f;

        // GameObject _camera;
        Animator _animator;
        Rigidbody _rigidbody;
        bool _canJump;

        static readonly int MoveAnimation = Animator.StringToHash("Move");
        static readonly int JumpAnimation = Animator.StringToHash("Jump");

        public override void OnStartClient()
        {
            base.OnStartClient();
            TryGetComponent(out _animator);
            TryGetComponent(out _rigidbody);
        }

        void Update()
        {
            if (!base.IsOwner)
            {
                return;
            }

            var hor = Input.GetAxisRaw("Horizontal");
            var ver = Input.GetAxisRaw("Vertical");

            Move(hor, ver);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                JumpServerRpc();
            }
        }

        [ServerRpc]
        void JumpServerRpc()
        {
            JumpObserversRpc();
        }

        [ObserversRpc(IncludeOwner = true)]
        void JumpObserversRpc()
        {
            _canJump = true;
        }

        void FixedUpdate()
        {
            if (!base.IsOwner)
            {
                return;
            }

            if (_canJump)
            {
                Jump();
                _canJump = false;
            }
        }

        void Move(float hor, float ver)
        {
            var direction = new Vector3(0f, 0, ver * _moveRate * Time.deltaTime);

            _animator.SetFloat(MoveAnimation, direction.magnitude);
            transform.position += transform.TransformDirection(direction);

            transform.Rotate(new Vector3(0f, hor * 0.5f, 0f));
        }

        void Jump()
        {
            _rigidbody.AddForce(transform.up * _jumpPower, ForceMode.Impulse);
            _animator.SetTrigger(JumpAnimation);
        }

        // bool IsGrounded()
        // {
        //     var ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        //     return Physics.Raycast(ray, 0.5f, LayerMask.NameToLayer("Ground"));
        // }
    }
}