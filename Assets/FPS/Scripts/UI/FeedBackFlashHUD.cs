using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 데미지, 힐 효과 클래스
    /// </summary>
    public class FeedBackFlashHUD : MonoBehaviour
    {
        #region Variables
        private Health playerHealth;

        public Image flashImage;
        public CanvasGroup flashCanvasGroup;

        public Color damageFlashColor;
        public Color healFlashColor;

        [SerializeField] private float flashDuration = 1f;
        [SerializeField] private float flashMaxAlpha = 1f;

        private bool flashActive = false;
        private float lastTimeFlashStarted = Mathf.NegativeInfinity;
        #endregion

        private void Start()
        {
            PlayerCharacterController playerCharacter = GameObject.FindObjectOfType<PlayerCharacterController>();
            playerHealth = playerCharacter.GetComponent<Health>();
            playerHealth.OnDamaged += OnDamaged;
            playerHealth.OnHeald += OnHeal;
        }

        private void Update()
        {
            if(flashActive)
            {
                float normalizedTimeSinceDamage = (Time.time - lastTimeFlashStarted) / flashDuration;
                if(normalizedTimeSinceDamage < 1f)
                {
                    float flashAmount = flashMaxAlpha * (1f - normalizedTimeSinceDamage);
                    flashCanvasGroup.alpha = flashAmount;
                }
                else
                {
                    flashCanvasGroup.gameObject.SetActive(false);
                    flashActive = false;
                }
            }
        }
        //효과 초기화
        void ResetFlsh()
        {
            flashActive = true;
            lastTimeFlashStarted = Time.time;       //효과 시작 시간
            flashCanvasGroup.alpha = 0f;
            flashCanvasGroup.gameObject.SetActive(true);
        }
        //데미지 입을 때 데미지 플레시 시작
        private void OnDamaged(float damage, GameObject damageSource)
        {
            ResetFlsh();
            flashImage.color = damageFlashColor;
        }
        //힐 할때 힐 플레쉬 시작
        private void OnHeal(float amount)
        {
            ResetFlsh();
            flashImage.color = healFlashColor;
        }
    }
}