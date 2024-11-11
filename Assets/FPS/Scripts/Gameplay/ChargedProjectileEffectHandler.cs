using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 충전용 발사체를 발사할 때 충전량에 발사체의 게임오브젝트 크기 결정
    /// </summary>
    public class ChargedProjectileEffectHandler : MonoBehaviour
    {
        #region Variables
        private ProjectileBase projectileBase;

        public GameObject chargeObject;
        public MinMaxVecter3 scale;
        #endregion
        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }
        void OnShoot()
        {
            chargeObject.transform.localScale = scale.GetValueFromRatio(projectileBase.InitialCharge);
        }
    }
}