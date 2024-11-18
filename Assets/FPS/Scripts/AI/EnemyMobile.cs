using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Enemy 상태
    /// </summary>
    public enum AIState
    {
        Patrol,
        Follow,
        Attack
    }
    /// <summary>
    /// 이동하는 Enemy의 상태들을 구현하는 클래스
    /// </summary>
    public class EnemyMobile : MonoBehaviour
    {
        #region Variables
        public Animator animator;
        private EnemyController enemyController;
        public AIState AiState {  get; private set; }

        //Sfx
        public AudioClip movementSound;
        public MinMaxFloat pitchMovementSpeed;
        
        private AudioSource audioSource;

        //데미지 - 이펙트
        public ParticleSystem[] randomHitSparks;

        //Detected
        public ParticleSystem[] detectedVfx;
        public AudioClip detectedSfx;

        //animation parameter
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamagedParameter = "OnDamaged";
        const string k_AnimDeathdParameter = "Death";
        #endregion

        private void Start()
        {
            //참조
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;
            enemyController.OnDetectedTarget += OnDetected;
            enemyController.OnLostTarget += OnLost;

            //초기화
            AiState = AIState.Patrol;
        }
        private void Update()
        {
            //상태에 따른 구현
            UpdateAiStateTransition();
            UpdateCurrentAiState();

            //속도에 따른 효과
            float moveSpeed = enemyController.Agent.velocity.magnitude;
            animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);         //이동 애니메이션
            audioSource.pitch = pitchMovementSpeed.GetValueFromRatio(moveSpeed/enemyController.Agent.speed);
        }
        //상태에 따른 Enemy 구현
        private void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Patrol:
                    //이동
                    enemyController.UpdatePathDestination(true);
                    enemyController.SetNavDestination(enemyController.GetDestinationOnpath());
                    break;
                case AIState.Follow:
                    //추적
                    enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    //공격
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.TryAttack(enemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }
        //상태 변경에 따른 구현
        private void UpdateAiStateTransition()
        {
            switch (AiState)
            {
                case AIState.Patrol:
                    //이동
                    break;
                case AIState.Follow:
                    //추적
                    if(enemyController.IsSeeingTarget && enemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Attack;
                        enemyController.SetNavDestination(transform.position);
                    }
                    break;
                case AIState.Attack:
                    //공격
                    if(!enemyController.IsTargetInAttackRange)
                    {
                        AiState= AIState.Follow;
                    }
                    break;
            }
        }

        void OnDamaged()
        {
            //스파크 파티클 - 랜덤하게 하나 선택해서 플레이
            if(randomHitSparks.Length > 0)
            {
                int randomNum = Random.Range(0, randomHitSparks.Length);
                randomHitSparks[randomNum].Play();
            }

            //데미지 애니
            animator.SetTrigger(k_AnimOnDamagedParameter);
        }
        
        //적 감지시 호출되는 함수
        private void OnDetected()
        {
            //상태 변경
            AiState = AIState.Follow;
            //Vfx
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Play();
            }
            //Sfx
            if(detectedSfx)
            {
                AudioUtility.CreateSfx(detectedSfx, this.transform.position, 1f);
            }
            //animator
            animator.SetBool(k_AnimAlertedParameter, true);
        }
        private void OnLost()
        {
            AiState = AIState.Patrol;
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Stop();
            }
            //
            animator.SetBool(k_AnimAlertedParameter, false);
        }
    }
}