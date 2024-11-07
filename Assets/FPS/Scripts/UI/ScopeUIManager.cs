using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class ScopeUIManager : MonoBehaviour
    {
        #region Variables
        public GameObject scopeUI;

        private PlayerWeaponsManager weaponsManager;
        #endregion

        private void Start()
        {
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();

            //Action 함수 등록
            weaponsManager.OnScopeWeapon += OnScope;
            weaponsManager.OffScopeWeapon += OffScope;

        }
        public void OnScope()
        {
            scopeUI.SetActive(true);
        }
        public void OffScope()
        {
            scopeUI.SetActive(false);
        }
    }
}