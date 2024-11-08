using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 발사체 표준형
    /// </summary>
    public class ProjectileStandard : ProjectileBase
    {
        #region Variables 
        //생성
        private ProjectileBase ProjectileBase;
        private float maxlifeTime = 5f;

        //이동
        [SerializeField] private float speed = 20f;
        [SerializeField]private float gravityDown = 0f;
        public Transform root;
        public Transform tip;

        private Vector3 velocity;
        private Vector3 lastRootPosition;
        private float shotTime;

        //충돌
        private float radius = 0.01f;               //충돌 검사하는 구체의 반경

        public LayerMask hittableLayers = -1;       //Hit가 가능한 Layer 지정
        private List<Collider> ignoredColliders;    //Hit 판정시 무시하는 충돌체 리스트

        //충돌 연출
        public GameObject impackVfx;                //타격 효과
        [SerializeField]private float impacklifeTime = 5f;
        private float impackVfxSpawnOffset = 0.1f;

        public AudioClip impackSfxClip;                     //타격음

        //공격
        [SerializeField]private float Damage = 15;
        #endregion

        private void OnEnable()
        {
            ProjectileBase = GetComponent<ProjectileBase>();
            ProjectileBase.OnShoot += OnShoot;

            //
            Destroy(gameObject, maxlifeTime);
        }
        //Shoot 값 설정
        new void OnShoot()
        {
            velocity = transform.forward * speed;
            transform.position += ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            lastRootPosition = root.position;

            //무시 충돌 리스트 생성 - projecttile을 발사하는 자신의 모든 충돌체를 가져와서 등록
            ignoredColliders = new List<Collider>();
            Collider[] ownerColliders = ProjectileBase.Owner.GetComponentsInChildren<Collider>();
            ignoredColliders.AddRange(ownerColliders);

            //프로젝타일이 벽을 뚫고 날아가는 버그 수정
            PlayerWeaponsManager weaponsManager = ProjectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if(weaponsManager != null )
            {
                Vector3 cameraToMuzzle = ProjectileBase.InitialPosition - weaponsManager.weaponCamera.transform.position;
                if(Physics.Raycast( weaponsManager.weaponCamera.transform.position,cameraToMuzzle.normalized,
                    out RaycastHit hit,cameraToMuzzle.magnitude,hittableLayers,QueryTriggerInteraction.Collide))
                {
                    if(IsHitValid(hit))
                    {
                        OnHit(hit.point, hit.normal, hit.collider);
                    }
                }
            }
        }
        private void Update()
        {
            //이동
            transform.position += velocity * Time.deltaTime;
            if(gravityDown > 0f)
            {
                velocity += Vector3.down * gravityDown * Time.deltaTime;
            }
            //충돌
            RaycastHit cloesHit = new RaycastHit();
            cloesHit.distance = Mathf.Infinity;
            bool foundHit = false;                      //hit한 충돌체를 찾았는지 여부 체크

            //Sphere Cast
            Vector3 displacementSinceLastFrame = tip.position - lastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition,radius,
                displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude,
                hittableLayers, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if(IsHitValid(hit) && hit.distance < cloesHit.distance)
                {
                    foundHit = true;
                    cloesHit = hit;
                }
            }
            //hit한 충돌체를 찾았다
            if(foundHit)
            {

                if(cloesHit.distance <= 0f)
                {
                    cloesHit.point = root.position;
                    cloesHit.normal = -transform.forward;
                }
                OnHit(cloesHit.point, cloesHit.normal,cloesHit.collider);
            }

            lastRootPosition = root.position;
        }
        //유효한 hit인지 판정
        bool IsHitValid(RaycastHit hit)
        {
            //IgnoreHitDectection 컴포넌트를 가지고 있으면 콜라이더 무시
            if( hit.collider.GetComponent<IgnoreHitDectection>())
            {
                return false;
            }
            //ignoredColliders에 포함된 콜라이더 무시 
            if (ignoredColliders != null && ignoredColliders.Contains(hit.collider))
            {
                return false;
            }
            //trigger Collider가 Damageable이 없어야 된다
            if(hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
            {
                return false;
            }
            return true;
        }
        //Hit 구현, 데미지, VFX, SFX
        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            //damage
            Health health = collider.GetComponent<Collider>().GetComponent<Health>();
            if (health != null )
            {
                health.TakeDamage(Damage, collider.gameObject);
            }

            //Vfx
            if(impackVfx)
            {
                GameObject impackgo = Instantiate(impackVfx, point + (normal * impackVfxSpawnOffset), Quaternion.LookRotation(normal));
                if(impacklifeTime > 0f)
                {
                    Destroy(impackgo, impacklifeTime);
                }
            }
            //Sfx
            if(impackSfxClip)
            {
                //충돌위치에 게임오브젝트를 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이한다
                AudioUtility.CreateSfx(impackSfxClip, point, 1f, 3f);
            }

            //발사체 킬
            Destroy(gameObject);
        }
    }
}