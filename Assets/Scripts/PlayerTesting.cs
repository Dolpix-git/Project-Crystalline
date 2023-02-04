using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;


namespace FishNet.Example.Prediction.Rigidbodies {

    public class PlayerTesting : NetworkBehaviour {
        #region Types.
        public struct MoveData : IReplicateData {
            public bool Jump;
            public float Horizontal;
            public float Vertical;
            public MoveData(bool jump, float horizontal, float vertical) {
                Jump = jump;
                Horizontal = horizontal;
                Vertical = vertical;
                _tick = 0;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
        public struct ReconcileData : IReconcileData {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;
            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                AngularVelocity = angularVelocity;
                _tick = 0;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
        #endregion

        #region Serialized.
        [SerializeField]
        private float _jumpForce = 15f;
        [SerializeField]
        private float _moveRate = 15f;
        #endregion

        #region Private.
        /// <summary>
        /// Rigidbody on this object.
        /// </summary>
        private Rigidbody _rigidbody;
        /// <summary>
        /// Next time a jump is allowed.
        /// </summary>
        private float _nextJumpTime;
        /// <summary>
        /// True to jump next frame.
        /// </summary>
        private bool _jump;
        #endregion

        private DefaultControl defaultControl;
        private Vector2 movement;

        private void Awake() {

            _rigidbody = GetComponent<Rigidbody>();
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;

            defaultControl = new DefaultControl();
            defaultControl.Player.Enable();
            defaultControl.Player.Jump.performed += Jump;
        }

        private void OnDestroy() {
            if (InstanceFinder.TimeManager != null) {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
            }
        }

        private void Update() {
            movement = defaultControl.Player.Movement.ReadValue<Vector2>();
        }

        private void Jump(InputAction.CallbackContext context) {
            if (base.IsOwner && context.performed) {
                if (Time.time > _nextJumpTime) {
                    _nextJumpTime = Time.time + 1f;
                    _jump = true;
                }
            }
        }

        private void TimeManager_OnTick() {
            if (base.IsOwner) {
                Reconciliation(default, false);
                CheckInput(out MoveData md);
                Move(md, false);
            }
            if (base.IsServer) {
                Move(default, true);
            }
        }


        private void TimeManager_OnPostTick() {
            if (base.IsServer) {
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity);
                Reconciliation(rd, true);
            }
        }

        private void CheckInput(out MoveData md) {
            md = default;

            float horizontal = movement.x;
            float vertical = movement.y;

            if (horizontal == 0f && vertical == 0f && !_jump)
                return;

            md = new MoveData(_jump, horizontal, vertical);
            _jump = false;
        }

        [Replicate]
        private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false) {
            //Add extra gravity for faster falls.
            Vector3 forces = new Vector3(md.Horizontal, Physics.gravity.y, md.Vertical) * _moveRate;
            _rigidbody.AddForce(forces);

            if (md.Jump)
                _rigidbody.AddForce(new Vector3(0f, _jumpForce, 0f), ForceMode.Impulse);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable) {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _rigidbody.velocity = rd.Velocity;
            _rigidbody.angularVelocity = rd.AngularVelocity;
        }


    }
}