using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class CrossHairManager : MonoBehaviour
    {
        #region Variables
        public Image crosshairImage;                    //크로스 헤어 UI 이미지
        public Sprite nullCrosshairSprite;              //액티브한 무기가 없을 때

        private RectTransform crosshairRectTranform;
        private CrossHairData crosshairDefault;         //평상시, 기본
        private CrossHairData crosshairTarget;          //타겟팅 되었을 때

        private CrossHairData crosshairCurrent;         //실질적으로 그리는 크로스헤어
        [SerializeField]private float crosshairUpdateShrpness = 5.0f;          //Lerp 변수

        private PlayerWeaponsManager weaponsManager;

        private bool wasPointingAtEnemy;
        #endregion

        private void Start()
        {
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            //액티브한 무기 크로스 헤어 보이기
            OnWeaponChanged(weaponsManager.GetActiveWeapon());

            weaponsManager.OnswitchToweapon += OnWeaponChanged;
        }
        private void Update()
        {
            UpdateCrosshairPointingAtEnemy(false);

            wasPointingAtEnemy = weaponsManager.IsPointingAtEnemy;
        }
        //크로스헤어 그리기
        void UpdateCrosshairPointingAtEnemy(bool force)
        {
            if (crosshairDefault.CrosshairSprite == null) return;

            //평상시?.타겟팅?
            if ((force|| wasPointingAtEnemy == false) && weaponsManager.IsPointingAtEnemy == true)    //적을 포착하는 순간
            {
                crosshairCurrent = crosshairTarget;
                crosshairImage.sprite = crosshairCurrent.CrosshairSprite;
                crosshairRectTranform.sizeDelta = crosshairCurrent.CrosshairSize * Vector2.one;
            }
            else if ((force || wasPointingAtEnemy == true) && weaponsManager.IsPointingAtEnemy == false)   //적을 놓치는 순간
            {
                crosshairCurrent = crosshairDefault;
                crosshairImage.sprite = crosshairCurrent.CrosshairSprite;
                crosshairRectTranform.sizeDelta = crosshairCurrent.CrosshairSize * Vector2.one;
            }
            crosshairImage.color = Color.Lerp(crosshairImage.color, crosshairCurrent.CrosshairColor,
                crosshairUpdateShrpness * Time.deltaTime);
            crosshairRectTranform.sizeDelta = Mathf.Lerp(crosshairRectTranform.sizeDelta.x,
                crosshairCurrent.CrosshairSize, crosshairUpdateShrpness * Time.deltaTime) * Vector2.one;
        }

        //무기가 바뀔 때마다 crosshairImage를 각각의 무기 CrossHair 이미지로 바꾸기
        void OnWeaponChanged(WeaponController newWeapon)
        {
            if (newWeapon)
            {
                crosshairImage.enabled = true;
                crosshairRectTranform = crosshairImage.GetComponent<RectTransform>();
                //액티한 무기의 크로스 헤어 정보 가져오기
                crosshairDefault = newWeapon.crosshairdefault;
                crosshairTarget = newWeapon.crosshairTargetInSight;
            }
            else
            {
                if(nullCrosshairSprite)
                {
                    crosshairImage.sprite = nullCrosshairSprite;
                }
                else
                {
                    crosshairImage.enabled = false;
                }
            }
            UpdateCrosshairPointingAtEnemy(true);
        }
    }
}