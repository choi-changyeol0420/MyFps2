using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// WeaponController 무기의 Ammo 카운트 UI
    /// </summary>
    public class AmmoUI : MonoBehaviour
    {
        #region Variables
        private PlayerWeaponsManager weaponsManager;

        private WeaponController weaponController;
        private int weaponIndex;
        private float ammoCount;

        //UI
        public TextMeshProUGUI weaponIndexText;
        public TextMeshProUGUI weaponammoText;

        public Image ammoFillImage;         //ammo rate에 따른 게이지
        [SerializeField]private float ammofillSharpness = 10;   //게이지 채우는(비우는) 속도
        private float weaponswitchSharpness = 10f;              //무기 교체시 UI가 바뀌는 속도

        public CanvasGroup canvasGroup;
        [SerializeField] [Range(0,1)] private float unSelectedOpacity = 0.5f;
        private Vector3 unSelectedScale = Vector3.one * 0.8f;

        //게이지 바 색 변경
        public FillBarColorChange fillBarColorChange;
        #endregion

        //UI 값 초기화
        public void Intialzie(WeaponController weapon, int _weaponIndex)
        {
            weaponController = weapon;
            weaponIndex = _weaponIndex;
            ammoCount = weapon.currentAmmo;

            //무기 인덱스
            weaponIndexText.text = (weaponIndex + 1).ToString();

            //게이지 배경색 초기화
            fillBarColorChange.Initialize(1f, 0.1f);

            //참조
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
        }
        private void Update()
        {
            float currentFillRate = weaponController.currentAmmoRatio;
            ammoFillImage.fillAmount = Mathf.Lerp(ammoFillImage.fillAmount,currentFillRate,Time.deltaTime * ammofillSharpness);
            //ammo 남은 갯수 UI
            weaponammoText.text = weaponController.currentAmmo.ToString("F1");

            //액티브 무기 구분
            bool isActiveWeapon = weaponController == weaponsManager.GetActiveWeapon();
            float currentOpacity = isActiveWeapon ? 1.0f : unSelectedOpacity;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, currentOpacity,
                Time.deltaTime * weaponswitchSharpness);
            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale;
            transform.localScale = Vector3.Lerp(transform.localScale ,currentScale,weaponswitchSharpness*Time.deltaTime);

            //배경색 값
            fillBarColorChange.UpdateVisual(currentFillRate);
        }
    }
}