using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Game;
using Unity.FPS.UI;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 무기 교체 상태
    /// </summary>
    public enum WeaponSwithState
    {
        Up,
        Down,
        PutDownPrvious,
        PutUpNew
    }

    /// <summary>
    /// 플레이어가 가진 무기(WeaponController)들을 관리하는 클래스
    /// </summary>
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variables
        //무기 지급 - 게임을 시작할 때 처음 유저에게 지급되는 무기 리스트(인벤토리)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        //무기장착
        //무기를 장착하는 오브젝트
        public Transform weaponParentSocket;

        //플레이어가 게임중에 들고 다니는 무기 리스트
        private WeaponController[] weaponSlots = new WeaponController[9];
        //무기 리스트(슬롯)중 활성화된 무기를 관리하는 인덱스
        public int ActiveWeaponIndex {  get; private set; }

        //무기 교체
        public UnityAction<WeaponController> OnswitchToweapon;  //무기 교체시 등록된 함수 호출 
        public UnityAction<WeaponController, int> OnAddedweapon;    //무기 추가할 때마다 등록된 함수 등록
        public UnityAction<WeaponController, int> OnRemoveWeapon;   //장착된 무기를 제거할 때마다 등록된 함수 호출

        private WeaponSwithState weaponSwithState;          //무기교체 상태

        private PlayerInputHandler playerInputHandler;

        //무기 교체시 계산되는 최종 위치
        private Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;
        public Transform aimingWeaponPosition;

        private int weaponSwitchNewIndex;                   //새로 바뀌는 무기 인덱스

        private float weaponswitchTimeStarted = 0f;
        [SerializeField]private float weaponswitchDelay = 1f;

        //적 포착
        public bool IsPointingAtEnemy { get; private set; }         //적 포착 여부
        public Camera weaponCamera;                                 //weaponCamera에서 Ray로 적 확인

        //조준
        //카메라
        private PlayerCharacterController playercharacterController;
        public float defaultFov = 60f;          //카메라 기본 FOV 값
        public float weaponFovMultiplier = 1f;       //FOV 연산 계수

        public bool IsAiming { get; private set; }                      //무기 조준 여부
        [SerializeField] private float aminganimationSpeed = 10f;       //무기 이동, Fov 연출 속도

        //흔들림
        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobSharpness = 10f;
        [SerializeField] private float defaltBobAmount = 0.05f;      //평상시 흔들림 량
        [SerializeField] private float aimingBobAmount = 0.02f;      //조준중 흔들림 량

        private float weaponBobFactor;              //흔들림 계수
        private Vector3 lastCharacterPosition;      //현재 프레임에서의 이동속도를 구하기 위한 변수

        private Vector3 weaponBobLocalPosition;     //흔들린 량 최종 계산 값, 이동하지 않으면 0


        //반동
        [SerializeField] private float recoilSharpness = 50f;       //뒤로 밀리는 이동 속도
        [SerializeField] private float maxRecoilDistance = 0.5f;    //반동시 뒤로 밀릴 수 있는 최대 거리
        private float recoileRePositionSharpness = 10f;             //제자리로 돌아오는 속도
        private Vector3 accumulateRecoil;                           //반동시 뒤로 밀리는 량

        private Vector3 weaponRecoilLocalPosition;                  //반동시 이동한 최종 계산값, 반동 후 제자리에 돌아오면 0

        //저격모드
        private bool isScopeOn =false;
        [SerializeField] private float distanceOnScope = 0.1f;

        public UnityAction OnScopeWeapon;                           //저격모드 시작시 등록된 함수 호출
        public UnityAction OffScopeWeapon;                          //저격모드 끝날시 등록된 함수 호출
        #endregion

        private void Start()
        {
            //참조
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playercharacterController = GetComponent<PlayerCharacterController>();

            //초기화
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            //액티브 무기 show 함수등록
            OnswitchToweapon += OnWeaponSwitched;

            //저격 모드 함수 등록
            OnScopeWeapon += OnScope;
            OffScopeWeapon += OffScope;


            //Fov 초기값 설정
            SetFov(defaultFov);

            //지급 받은 무기 장착
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon);
            }
            SwitchWeapon(true);
        }
        private void Update()
        {
            //현재 액티브 무기
            WeaponController activewepon = GetActiveWeapon();

            if(weaponSwithState == WeaponSwithState.Up)
            {
                //조준 입력값 처리
                IsAiming = playerInputHandler.GetAimInputHeld();

                //저격모드 처리
                if(activewepon.shootType == WeaponShootType.Sniper)
                {
                    if(playerInputHandler.GetAimInputDown())
                    {
                        //저격모드 시작
                        isScopeOn = true;
                    }
                    else if(playerInputHandler.GetAimInputUp())
                    {
                        //저격모든 끝
                        OffScopeWeapon?.Invoke();
                    }
                }

                //슛 처리
                bool isFire = activewepon.HandleShootInputs(playerInputHandler.GetFireInputDown(),
                    playerInputHandler.GetFireInputHeld(),
                    playerInputHandler.GetFireInputReleased());
                if(isFire)
                {
                    //반동효과
                    accumulateRecoil += Vector3.back * activewepon.recoilForce;
                    accumulateRecoil = Vector3.ClampMagnitude(accumulateRecoil, maxRecoilDistance);
                }
            }
            //무기 교체 입력값
            if (!IsAiming && (weaponSwithState == WeaponSwithState.Up || weaponSwithState == WeaponSwithState.Down))
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
            }

            //적 포착
            IsPointingAtEnemy = false;
            if(activewepon)
            {
                RaycastHit hit;
                if(Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out hit, 300f))
                {
                    //콜라이더 체크 - 적(Health)
                    Health health = hit.collider.GetComponent<Health>();
                    if (health)
                    {
                        IsPointingAtEnemy= true;
                    }
                }
            }
            
        }
        private void LateUpdate()
        {
            UpdateWeaponBob();
            UpdateWeaponRecoil();
            UpdateWeaponAiming();
            UpdateWeaponSwitching();

            //무기 최종 위치
            weaponParentSocket.localPosition = weaponMainLocalPosition +
                weaponBobLocalPosition + weaponRecoilLocalPosition;
        }


        //반동
        void UpdateWeaponRecoil()
        {
            if(weaponRecoilLocalPosition.z >= accumulateRecoil.z * 0.99f)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition,
                    accumulateRecoil, recoilSharpness * Time.time);
            }
            else
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition,
                    Vector3.zero, recoileRePositionSharpness * Time.time);
                accumulateRecoil = weaponRecoilLocalPosition;
            }
        }

        //카메라 Fov 값 세팅 : 줌인, 줌 아웃
        private void SetFov(float fov)
        {
            playercharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplier;
        }
        //무기 조준에 따른 연출, 무기위치 조정, Fov값
        void UpdateWeaponAiming()
        {
            //무기를 들고 있을 때만 조준
            if(weaponSwithState == WeaponSwithState.Up)
            {
                WeaponController activewepon = GetActiveWeapon();

                if (IsAiming && activewepon)    //조준시: 디폴트 -> aiming 위치로 이동
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        aimingWeaponPosition.localPosition + activewepon.aimOffset,
                        aminganimationSpeed * Time.deltaTime);
                    if (isScopeOn)
                    {
                        //weaponMainLocalPosition, 목표지점까지의 거리를 구한다
                        float distance = Vector3.Distance(weaponMainLocalPosition, aimingWeaponPosition.localPosition + activewepon.aimOffset);
                        if (distance < distanceOnScope)
                        {
                            OnScopeWeapon?.Invoke();
                            isScopeOn = false;
                        }
                    }
                    else
                    {
                        float fov = Mathf.Lerp(playercharacterController.PlayerCamera.fieldOfView,
                        activewepon.aimZoomRatio * defaultFov, aminganimationSpeed * Time.deltaTime);
                        SetFov(fov);
                    }
                }
                else            //조준이 풀렸을 때 -> aiming 위치 -> 디폴트 위치로 이동
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        defaultWeaponPosition.localPosition, aminganimationSpeed * Time.deltaTime);
                    float fov = Mathf.Lerp(playercharacterController.PlayerCamera.fieldOfView,
                        defaultFov, aminganimationSpeed * Time.deltaTime);
                    SetFov(fov);
                }
            }
        }

        //이동에 의한 무기 흔들린 값 구하기
        void UpdateWeaponBob()
        {
            if(Time.deltaTime > 0)
            {
                //플레이어가 한 프레임동안 이동한 거리
                //playercharacterController.transform.position - lastCharacterPosition;
                //현재 프레임에서 플레이어 이동 속도
                Vector3 playerCharacterVelocity =
                    (playercharacterController.transform.position - lastCharacterPosition)/Time.deltaTime;

                float characterMoveFactor = 0f;
                if(playercharacterController.IsGrounded)
                {
                    characterMoveFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude /
                        (playercharacterController.MaxSpeedOnGround * playercharacterController.SprintSpeedModifier));
                }

                //속도에 의한 흔들림 계수
                weaponBobFactor = Mathf.Lerp(weaponBobFactor, characterMoveFactor, bobSharpness* Time.deltaTime);

                //흔들림 량(조준시, 평상시)
                float bobAmount = IsAiming ? aimingBobAmount : defaltBobAmount;
                float frequency = bobFrequency;
                //좌우 흔들림
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                //상하 흔들림 (좌우 흔들림의 절반)
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f) * bobAmount * weaponBobFactor;

                //흔들림 최종 변수에 적용
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = Mathf.Abs(vBobValue);

                //플레이어의 현재 프레임의 마지막 위치를 저장
                lastCharacterPosition = playercharacterController.transform.position;
            }
        }

        //상태에 따른 무기 연출
        void UpdateWeaponSwitching()
        {
            //Lerp 변수
            float switchingTimeFactor = 0f;
            if (weaponswitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponswitchTimeStarted) / weaponswitchDelay);
            }

            //
            if (switchingTimeFactor >= 1f)
            {
                if(weaponSwithState == WeaponSwithState.PutDownPrvious)
                {
                    //현재 무기 false, 새로운 무기 true
                    WeaponController oldweapon = GetActiveWeapon();
                    if (oldweapon != null)
                    {
                        oldweapon.ShowWeapon(false);
                    }

                    ActiveWeaponIndex = weaponSwitchNewIndex;
                    WeaponController newWeapon = GetActiveWeapon();
                    OnswitchToweapon?.Invoke(newWeapon);

                    switchingTimeFactor = 0f;
                    if(newWeapon != null)
                    {
                        weaponswitchTimeStarted = Time.time;
                        weaponSwithState = WeaponSwithState.PutUpNew;
                    }
                    else
                    {
                        weaponSwithState = WeaponSwithState.Down;
                    }

                }
                else if (weaponSwithState == WeaponSwithState.PutUpNew)
                {
                    weaponSwithState = WeaponSwithState.Up;
                }
            }
            //지연시간동안 무기의 위치 이동
            if (weaponSwithState == WeaponSwithState.PutDownPrvious)
            {
                weaponMainLocalPosition = Vector3.Lerp(defaultWeaponPosition.localPosition,
                    downWeaponPosition.localPosition, switchingTimeFactor);
            }
            else if(weaponSwithState == WeaponSwithState.PutUpNew)
            {
                weaponMainLocalPosition = Vector3.Lerp(downWeaponPosition.localPosition,
                    defaultWeaponPosition.localPosition, switchingTimeFactor);
            }


        }

        //weaponSlots에 무기 프리팹에 생성한 WeaponController 오브젝트 추가
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            //추가하는 무기 소지 여부 체크 - 중복 검사
            if(HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("Has Same Weapon");
                return false;
            }

            for(int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);
                    weaponInstance.transform.localPosition = Vector3.zero;
                    weaponInstance.transform.localRotation = Quaternion.identity;

                    weaponInstance.Owner = this.gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    //무기장착
                    OnAddedweapon?.Invoke(weaponInstance, i);

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
                
            }
            Debug.Log("weapon full");
            return false;
        }
        public bool RemoveWeapon(WeaponController oldweapon)
        {
            for(int i = 0; i < weaponSlots.Length; i++)
            {
                //같은 무기 찾아서 제거
                if(weaponSlots[i] == oldweapon)
                {
                    weaponSlots[i] = null;
                    OnRemoveWeapon?.Invoke(oldweapon,i);
                    Destroy(oldweapon.gameObject);

                    //현재 제거한 무기가 액티브이면
                    if(i == ActiveWeaponIndex)
                    {
                        SwitchWeapon(true);
                    }
                    return true;
                }
            }
            return false;
        }
        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }
        private WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for(int i = 0; i< weaponSlots.Length; i++)
            {
                if(weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab)
                {
                    return weaponSlots[i];
                }
            }
            return null;
        }
        //지정된 슬롯에 무기가 있는지 여부
        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            if(index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }
            return null ;
        }

        //0~9
        //무기 바꾸기, 현재 들고있는 무기 false, 새로운 무기 true
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;        //새로운 액티브할 무기 인덱스
            int closestSlotDistance = weaponSlots.Length;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceDetweenWeaponSlot(ActiveWeaponIndex,i, ascendingOrder);
                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }
            //새로 액티브할 무기 인덱스로 무기 교체
            SwitchToWeaponIndex(newWeaponIndex);
        }

        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            //newWeaponIndex 값 체크
            if(newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;
                weaponswitchTimeStarted = Time.time;

                //현재 액티브한 무기가 있느냐?
                if(GetActiveWeapon() == null)
                {
                    weaponMainLocalPosition = downWeaponPosition.position;
                    weaponSwithState = WeaponSwithState.PutUpNew;
                    ActiveWeaponIndex = newWeaponIndex;

                    WeaponController weaponController = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnswitchToweapon?.Invoke(weaponController);
                }   
                else
                {
                    weaponSwithState = WeaponSwithState.PutDownPrvious;
                }
            }
        }

        //슬롯간 거리
        private int GetDistanceDetweenWeaponSlot(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;
            if(ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = fromSlotIndex - toSlotIndex;
            }

            if(distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = distanceBetweenSlots + weaponSlots.Length;
            }
            return distanceBetweenSlots;
        }

        void OnWeaponSwitched(WeaponController newWeapon)
        {
            if(newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }
        void OnScope()
        {
            weaponCamera.enabled = false;
        }
        void OffScope()
        {
            weaponCamera.enabled = true;
        }
    } 
}