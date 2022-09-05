using Mirror;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerControllerOLD : NetworkBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private NetworkTransform _networkTransform;
        [SerializeField] private Animator _animator;
        

        private void OnValidate()
        {
            _characterController.enabled = false;
            _rigidbody.isKinematic = true;
            _networkTransform.clientAuthority = true;
        }

        public override void OnStartLocalPlayer()
        {
            _characterController.enabled = true;
        }

        [Header("Movement Settings")] public float _moveSpeed = 2f;
        public float _turnSensitivity = 5f;
        public float _maxTurnSpeed = 100f;

        [Header("Diagnostics")] public float _horizontal;
        public float _vertical;
        public float _turn;
        public float _jumpSpeed;
        public bool _isGrounded = true;
        public bool _isFalling;
        public Vector3 _velocity;

        private static readonly int AnimatorInputX = Animator.StringToHash("InputX");
        private static readonly int AnimatorInputY = Animator.StringToHash("InputY");

        private void Update()
        {
            if (!isLocalPlayer || _characterController == null || !_characterController.enabled)
                return;

            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
            
            if (Input.GetKey(KeyCode.Q))
            {
                _turn = Mathf.MoveTowards(_turn, -_maxTurnSpeed, _turnSensitivity);
            }

            if (Input.GetKey(KeyCode.E))
            {
                _turn = Mathf.MoveTowards(_turn, _maxTurnSpeed, _turnSensitivity);
            }

            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
            {
                _turn = Mathf.MoveTowards(_turn, 0, _turnSensitivity);
            }

            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
            {
                _turn = Mathf.MoveTowards(_turn, 0, _turnSensitivity);
            }


            if (_isGrounded)
            {
                _isFalling = false;
            }

            if ((_isGrounded || !_isFalling) && _jumpSpeed < 3f && Input.GetKey(KeyCode.Space))
            {
                _jumpSpeed = Mathf.Lerp(_jumpSpeed, 3f, 0.5f);
            }
            else if (!_isGrounded)
            {
                _isFalling = true;
                _jumpSpeed = 0;
            }
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer || _characterController == null || !_characterController.enabled)
                return;
            
            _animator.SetFloat("InputX", _horizontal);
            _animator.SetFloat("InputY", _vertical);
            transform.Rotate(0f, _turn * Time.fixedDeltaTime, 0f);

            var direction = new Vector3(_horizontal, _jumpSpeed, _vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= _moveSpeed;

            if (_jumpSpeed > 0)
                _characterController.Move(direction * Time.fixedDeltaTime);
            else
                _characterController.SimpleMove(direction);

            _isGrounded = _characterController.isGrounded;
            _velocity = _characterController.velocity;
        }
    }
}