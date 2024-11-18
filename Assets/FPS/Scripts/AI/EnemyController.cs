using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 렌더러 데이터: 메터리얼 정보
    /// </summary>
    [System.Serializable]
    public struct RendererIndexData
    {
        public Renderer renderer;
        public int materialIndex;

        public RendererIndexData(Renderer _renderer, int index)
        {
            renderer = _renderer;
            materialIndex = index;
        }
    }

    /// <summary>
    /// Enemy를 관리하는 클래스
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;

        //death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpwanPosition;

        //damage
        public UnityAction Damaged;

        //sfx
        public AudioClip damageclip;

        //Vfx
        public Material bodyMaterial;                                   //데미지를 줄 메터리얼
        [GradientUsage(true)]
        public Gradient OnHitBoodyGradient;        //데미지를 컬러 그라디언트 효과로 표현
        //bodymaterial을 가지고 있는 렌더러 데이터 리스트
        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField]private float flshOnHitDuration = 0.5f;
        float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagedThisFrame = false;

        //Patrol
        public NavMeshAgent Agent { get; private set; }
        public PatrolPath patrolPath { get; set; }
        private int pathDestinationIndex;               //목표 웨이포인트 위치
        private float pathReachingRadius = 1f;          //도착판정

        //Detection
        private Actor actor;
        private Collider[] selfColliders;

        public DetectionModule detectionModule { get; private set; }

        public GameObject KnownDetectedTarget => detectionModule.KnownDetectedTarget;
        public bool IsSeeingTarget => detectionModule.IsSeeingTraget;
        public bool HadKnownTarget => detectionModule.HadKnowTarget;
        public Material eyeColorMaterial;
        [ColorUsage(true, true)] public Color defaultEyeColor;
        [ColorUsage(true, true)] public Color attackeyeColor;

        //Eye material을 가지고 있는 렌더러 데이터
        private RendererIndexData eyeRendererData;
        private MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public UnityAction OnDetectedTarget;
        public UnityAction OnLostTarget;

        private float orientSpeed = 10f;
        public bool IsTargetInAttackRange => detectionModule.IsTargetInAttackRange;

        public bool swapToNextWeapon = false;
        public float delayAfterWeaponSwap = 0f;
        private float lastTimeWeaponSwapped = Mathf.NegativeInfinity;

        public int currentWeaponIndex;
        private WeaponController currentWeapon;
        private WeaponController[] weapons;
        #endregion

        private void Start()
        {
            //참조
            Agent = GetComponent<NavMeshAgent>();

            actor = GetComponent<Actor>();
            selfColliders = GetComponentsInChildren<Collider>();

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            detectionModule = detectionModules[0];
            detectionModule.OnDetectedTarget += OnDetected;
            detectionModule.OnLostTarget += OnLost;


            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            //무기초기화
            FindAndInitializeAllWeapon();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            //bodymaterial을 가지고 있는 렌더러 정보 리스트 만들기
            foreach (var renderer in renderers)
            {
                for(int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    //body
                    if(renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }
                    //eye
                    if(renderer.sharedMaterials[i] == eyeColorMaterial)
                    {
                        eyeRendererData =new RendererIndexData(renderer, i);
                    }
                }
            }
            //body
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();
            //eye
            if(eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,eyeRendererData.materialIndex);
            }
        }
        private void Update()
        {
            //디텍션
            detectionModule.HandleTargetDetection(actor, selfColliders);

            //데미지 효과
            Color currentColor = OnHitBoodyGradient.Evaluate((Time.time - lastTimeDamaged) / flshOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor",currentColor);
            foreach(var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock,data.materialIndex);
            }


            //
            wasDamagedThisFrame = false;
        }
        void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && damageSource.GetComponent<EnemyController>() == null)
            {
                //등록된 함수 호출
                Damaged?.Invoke();

                //데미지 준 시간
                lastTimeDamaged = Time.time;

                //Sfx
                if(damageclip && wasDamagedThisFrame == false)
                {
                    AudioUtility.CreateSfx(damageclip, this.transform.position, 0f);
                }
                wasDamagedThisFrame = true;
            }
        }
        void OnDie()
        {
            GameObject VfxGO = Instantiate(deathVfxPrefab, deathVfxSpwanPosition.position,Quaternion.identity);
            Destroy(VfxGO,5f);

            //enemy kill
            Destroy(gameObject);
        }

        //패트롤이 유효한지? 패트롤이 가능한지?
        private bool IsPathVaild()
        {
            return patrolPath && patrolPath.wayPoints.Count > 0;
        }
        //가장 가까운 WayPoint 찾기
        private void SetPathDestinationToClosestWayPoint()
        {
            if(IsPathVaild() == false)
            {
                pathDestinationIndex = 0;
                return;
            }
                
            int closestWayPointIndex = 0;

            for (int i = 0; i < patrolPath.wayPoints.Count; i++)
            {
                float distance = patrolPath.GetDistanceToWayPoint(transform.position, i);
                float closestDistance = patrolPath.GetDistanceToWayPoint(transform.position,closestWayPointIndex);
                if(distance < closestDistance)
                {
                    closestWayPointIndex = i;
                }
                pathDestinationIndex = closestWayPointIndex;
            }
        }

        //목표 지점의 위치 값 얻어오기
        public Vector3 GetDestinationOnpath()
        {
            if(IsPathVaild() == false)
            {
                return this.transform.position;
            }

            return patrolPath.GetPositionOfWayPoint(pathDestinationIndex);
        }

        //목표 지점 설정 - Nav 시스템 이용
        public void SetNavDestination(Vector3 destination)
        {
            if(Agent)
            {
                Agent.SetDestination(destination);
            }
        }

        //도착 판정 후 다음 목표 지점 설정
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathVaild() == false)
                return;

            //도착판정
            float distance = (transform.position - GetDestinationOnpath()).magnitude;
            if(distance <= pathReachingRadius)
            {
                pathDestinationIndex = inverseOrder ? (pathDestinationIndex - 1) : (pathDestinationIndex + 1);
                if(pathDestinationIndex < 0)
                {
                    pathDestinationIndex += patrolPath.wayPoints.Count;
                }
                if(pathDestinationIndex >= patrolPath.wayPoints.Count)
                {
                    pathDestinationIndex -= patrolPath.wayPoints.Count;
                }
            }
        }
        public void OrientToward(Vector3 lookPosition)
        {
            Vector3 lookDirect = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirect.sqrMagnitude != 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirect);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, orientSpeed * Time.deltaTime);
            }
        }
        private void OnDetected()
        {
            OnDetectedTarget?.Invoke();

            Debug.Log("OnDetected");
            if(eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", attackeyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock, eyeRendererData.materialIndex);
            }
        }
        private void OnLost()
        {
            OnLostTarget?.Invoke();
            Debug.Log("OnLost");
            if (eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock, eyeRendererData.materialIndex);
            }
        }
        //가지고 있는 무기 찾고 초기화
        private void FindAndInitializeAllWeapon()
        {
            if(weapons == null)
            {
                weapons = this.GetComponentsInChildren<WeaponController>();
                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].Owner = this.gameObject;
                }
            }
        }

        //지정한 인덱스 에 해당하는 무기를 current로 저장
        private void SetCurrentWeapon(int index)
        {
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];
            if(swapToNextWeapon)
            {
                lastTimeWeaponSwapped = Time.time;
            }
            else
            {
                lastTimeWeaponSwapped= Mathf.NegativeInfinity;
            }
        }

        //현재 current Weapon 찾기
        public WeaponController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapon();
            if(currentWeapon == null)
            {
                SetCurrentWeapon(0);
            }
            return currentWeapon;
        }
        //적에게 총구를 돌린다
        public void OrientWeaponsToward(Vector3 lookPosition)
        {
            for(int i = 0;i < weapons.Length;i++)
            {
                Vector3 weaponForward = (lookPosition - weapons[i].transform.position).normalized;
                weapons[i].transform.forward = weaponForward;
            }
        }
        //공격
        public void TryAttack(Vector3 targetPosition)
        {

        }
    }
}