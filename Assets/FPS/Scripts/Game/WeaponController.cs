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

        //CrossHair
        public CrossHairData crosshairdefault;              //기본, 평상시
        public CrossHairData crosshairTargetInSight;        //적을 포착했을 때, 타겟팅을 되었을 때

        //조준
        public float aimZoomRatio = 1f;             //조준시 줌인 설정값
        public Vector3 aimOffset;                   //조준시 무기 위치 조정값
        #endregion

        private void Awake()
        {
            shootAudioSource = GetComponent<AudioSource>();
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
    }
}