using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        #region Variables
        private Health playerHealth;
        public Image healthFillImage;
        #endregion
        private void Start()
        {
            PlayerCharacterController playerCharacter = GameObject.FindObjectOfType<PlayerCharacterController>();
            playerHealth = playerCharacter.GetComponent<Health>();
        }

        private void Update()
        {
            healthFillImage.fillAmount = playerHealth.GetRatio();
        }
    }
}