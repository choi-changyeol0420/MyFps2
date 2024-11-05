using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Game;

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
    /// 플레이어가 가진 무기들을 관리하는 클래스
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

        private WeaponSwithState weaponSwithState;          //무기교체 상태

        private PlayerInputHandler playerInputHandler;

        //무기 교체시 계산되는 최종 위치
        private Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;

        private int weaponSwitchNewIndex;                   //새로 바뀌는 무기 인덱스

        private float weaponswitchTimeStarted = 0f;
        [SerializeField]private float weaponswitchDelay = 1f;

        #endregion

        private void Start()
        {
            //참조
            playerInputHandler = GetComponent<PlayerInputHandler>();

            //초기화
            ActiveWeaponIndex = -1;
            weaponSwithState = WeaponSwithState.Down;

            //
            OnswitchToweapon += OnWeaponSwitched;

            //지급 받은 무기 장착
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon);
            }
            SwitchWeapon(true);
        }
        private void Update()
        {
            if (weaponSwithState == WeaponSwithState.Up || weaponSwithState == WeaponSwithState.Down)
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
            }
        }
        private void LateUpdate()
        {
            UpdateWeaponSwitching();

            //무기 최종 위치
            weaponParentSocket.localPosition = weaponMainLocalPosition;
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
                weaponMainLocalPosition = Vector3.Lerp(defaultWeaponPosition.localPosition, downWeaponPosition.localPosition, switchingTimeFactor);
            }
            else if(weaponSwithState == WeaponSwithState.PutUpNew)
            {
                weaponMainLocalPosition = Vector3.Lerp(downWeaponPosition.localPosition, defaultWeaponPosition.localPosition, switchingTimeFactor);
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

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
                
            }
            Debug.Log("weapon full");
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
    }
}