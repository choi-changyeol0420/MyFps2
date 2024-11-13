using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldSpaceHealthbar : MonoBehaviour
    {
        #region Variables
        public Health health;
        public Image healthbar;
        //UI
        public Transform healthbarobject;

        [SerializeField]private bool hideFullHealthBar = true;
        #endregion
        private void Update()
        {
            
            healthbar.fillAmount = health.GetRatio();
            healthbarobject.LookAt(healthbarobject.position + Camera.main.transform.rotation * Vector3.forward,Camera.main.transform.rotation * Vector3.up);
            if(hideFullHealthBar)
            {
                healthbarobject.gameObject.SetActive(healthbar.fillAmount != 1);
            }
        }
    }
}