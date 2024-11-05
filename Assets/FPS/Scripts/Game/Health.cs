using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 체력을 관리하는 클래스
    /// </summary>
    public class Health : MonoBehaviour
    {
        #region Variables
        [SerializeField]private float maxHealth = 100f;     //최대 HP
        public float CurrentHealth { get; private set; }    //현재 HP
        private bool isDeath = false;                       //죽음 체크

        public UnityAction<float,GameObject> OnDamaged;
        public UnityAction OnDie;
        public UnityAction<float> OnHeald;

        //체력 위험 경계율
        [SerializeField]private float criticalHealRatio = 0.3f;

        //무적 상태
        public bool Invincible { get; private set; }
        #endregion

        //힐 아이템을 먹을 수 있는지 체크
        public bool CanPickupHeal() => CurrentHealth < maxHealth;
        //UI HP 게이지 값
        public float GetRatio() => CurrentHealth / maxHealth;
        //위험체크
        public bool IsCritical() => GetRatio() <= criticalHealRatio;

        private void Start()
        {
            //초기화
            CurrentHealth = maxHealth;
            Invincible = false;

        }
        //damagSource: 데미지를 주는 주체
        public void TakeDamage(float damage, GameObject damageSource)
        {
            //무적 체크
            if(Invincible) return;


            float beforeHealth = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            //real Damage 구하기
            float realDamage = beforeHealth - CurrentHealth;
            if(realDamage > 0f)
            {
                //데미지 구현
                OnDamaged?.Invoke(realDamage, damageSource);
            }
            //죽음 처리
            HandleDeath();
        }
        //죽음 처리
        void HandleDeath()
        {
            //죽음 체크
            if (isDeath) return;

            if(CurrentHealth <= 0f)
            {
                isDeath = true;

                //죽음 구현
                OnDie?.Invoke();
            }
        }
        public void Heal(float HealHealth)
        {
            
            float beforeHealth = CurrentHealth;
            CurrentHealth += HealHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            //real Heal 구하기
            float realHeal = CurrentHealth - beforeHealth;
            if(realHeal > 0)
            {
                //힐 구현
                OnHeald?.Invoke(realHeal);
            }
        }
    }
}