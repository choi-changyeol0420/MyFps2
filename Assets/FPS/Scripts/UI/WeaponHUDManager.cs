using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        #region Variables
        public RectTransform ammoPanel;
        public GameObject ammoCountPrefab;

        private PlayerWeaponsManager weaponsManager;
        #endregion
        private void Awake()
        {
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();

            weaponsManager.OnAddedweapon += AddWeapon;
            weaponsManager.OnRemoveWeapon += RemoveWeapon;
            weaponsManager.OnswitchToweapon += SwitchWeapon;
        }
        //무기추가 하면 ammoUI 하나 추가
        void AddWeapon(WeaponController newWeapon, int weaponindex)
        {
            GameObject ammoCountGo = Instantiate(ammoCountPrefab, ammoPanel);
            AmmoUI ammoUI = ammoCountGo.GetComponent<AmmoUI>();
            ammoUI.Intialzie(newWeapon, weaponindex);
        }
        //무기 체거 하면 ammoUI 하나 제거
        void RemoveWeapon(WeaponController oldWeapon, int weaponindex)
        {
            Destroy(ammoCountPrefab);
        }
        void SwitchWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);
        }
    }
}