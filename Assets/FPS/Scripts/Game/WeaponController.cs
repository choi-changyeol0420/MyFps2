using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 크로스헤어를 그리기 위한 데이터
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrosshairSprite;
        public float CrosshairSize;
        public Color CrosshairColor;
    }
    /// <summary>
    /// 무기 슛 타입
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Autimatic,
        Charge,
        Sniper
    }
    /// <summary>
    /// 무기(총기)를 관리하는 클래스
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        //무기 활성화, 비활성
        public GameObject weaponRot;

        public GameObject Owner { get; set; }               //무기의 주인
        public GameObject SourcePrefab {  get; set; }       //무기를 생성한 오리지널 프리팹
        public bool IsWeaponActive { get; private set; }    //무기 활성화 여부

        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;

        //슛
        public WeaponShootType shootType;

        [SerializeField]private float maxAmmo = 8f;         //장전할 수 있는 최대 총알의 갯수
        public float currentAmmo;

        [SerializeField] private float delayBeweenShots = 0.5f;     //슛 간격
        private float lastTimeShot;                                 //마지막으로 슛한 시간

        //VFX, SFX
        public Transform weaponMuzzle;                              //총구 위치
        public GameObject MuzzlePrefab;                             //총알 발사 이펙트 효과
        public AudioClip shootSfx;                                  //총 발사 사운드

        //CrossHair
        public CrossHairData crosshairdefault;              //기본, 평상시
        public CrossHairData crosshairTargetInSight;        //적을 포착했을 때, 타겟팅을 되었을 때

        //조준
        public float aimZoomRatio = 1f;             //조준시 줌인 설정값
        public Vector3 aimOffset;                   //조준시 무기 위치 조정값

        public float recoilForce = 0.5f;

        //Projectile
        public ProjectileBase projectilePrefab;

        public Vector3 MuzzleVWorldVelocity {  get; private set; }              //현재 프레임의 속도
        private Vector3 lastMuzzlePosition;
        public float CurrentCharge { get; private set; }

        [SerializeField]private int bulletsPerShot = 1;                         //한번 슛하는데 발사되는 탄환의 갯수
        [SerializeField]private float bulletSpreadAngle = 0f;                                   //불렛이 퍼져 나가는 각도
        #endregion

        public float currentAmmoRatio => currentAmmo / maxAmmo;

        private void Awake()
        {
            //참조
            shootAudioSource = GetComponent<AudioSource>();
        }
        private void Start()
        {
            //초기화
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time;
            lastMuzzlePosition = weaponMuzzle.position;
        }

        private void Update()
        {
            //MuzzleVWorldVelocity
            if(Time.deltaTime > 0)
            {
                MuzzleVWorldVelocity = (weaponMuzzle.position - lastMuzzlePosition)/Time.deltaTime;

                lastMuzzlePosition = weaponMuzzle.position;
            }
        }
        //무기 활성화, 비활성
        public void ShowWeapon(bool show)
        {
            weaponRot.SetActive(show);
            if(show == true && switchWeaponSfx != null)
            {
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }

            IsWeaponActive = show;
        }

        //키 입력에 따른 슛 타입 구현
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            switch(shootType)
            {
                case WeaponShootType.Manual:
                    if(inputDown)
                    {
                        //슛
                        return TryShoot();
                    }
                    break;
                case WeaponShootType.Autimatic:
                    if(inputHeld)
                    {
                        return TryShoot();
                    }
                    break;
                case WeaponShootType.Charge:
                    break;
                case WeaponShootType.Sniper:
                    if(inputDown)
                    {
                        return TryShoot();
                    }
                    break;
            }
            return false;
        }
        bool TryShoot()
        {
            //
            if(currentAmmo >= 1f && (lastTimeShot + delayBeweenShots) < Time.time)
            {
                currentAmmo -= 1f;
                Debug.Log($"currentAmmo: {currentAmmo}");

                HandleShot();
                return true;
            }
            if(currentAmmo == 0f)
            {
                currentAmmo = maxAmmo;
            }
            return false;
        }
        //슛 연출
        void HandleShot()
        {
            //project tile 생성
            for (int i =0;  i < bulletsPerShot; i++)
            {
                Vector3 shotDirection = GetShotDirectionWithSpread(weaponMuzzle);
                ProjectileBase ProjectileInstance = Instantiate(projectilePrefab, weaponMuzzle.position, Quaternion.LookRotation(shotDirection));
                ProjectileInstance.Shoot(this);
            }
            //VFX
            if(MuzzlePrefab)
            {
                GameObject muzzlego = Instantiate(MuzzlePrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle);
                Destroy(muzzlego, 2f);
            }

            //SFX
            if(shootSfx)
            {
                shootAudioSource.PlayOneShot(shootSfx);
            }
            lastTimeShot = Time.time;
        }
        //project tile 날아가는 방향
        Vector3 GetShotDirectionWithSpread(Transform shootTransform)
        {
            float spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Lerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }
    }
}