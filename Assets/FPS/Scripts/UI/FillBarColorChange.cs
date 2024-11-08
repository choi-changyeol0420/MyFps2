using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// 게이지바의 게이지 색, 백그라운드 색 변경 구현
    /// </summary>
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables
        public Image foregroundImage;
        public Color defaultForegroundColor;            //게이지의 기본 컬러
        public Color flashForeGroundColorFull;          //게이지가 풀로 차는 순간 색 플래시 효과

        public Image backgroundImage;
        public Color defaultBackGroundColor;            //백그라운드 기본 컬러
        public Color flashBackGroundColorEmpty;         //백그라운드 게이지값이 0일 때의 컬러값

        private float fullValue = 1f;                   //게이지가 풀일때의 값
        private float emptyValue = 0f;                  //게이지가 없을때의 값

        private float colorChangeSharpness = 5f;        //컬러 변경 속도
        private float prevousValue;                     //게이지가 풀로 차는 순간을 찾는 변수
        #endregion
        //색 변경 관련 값 초기화
        public void Initialize(float fullValueRatio, float emptyValueRation)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRation;

            prevousValue = fullValue;
        }

        public void UpdateVisual(float currentRatio)
        {
            //게이지가 풀로 차는 순간
            if(currentRatio == fullValue && currentRatio != prevousValue)
            {
                foregroundImage.color = flashForeGroundColorFull;
            }
            else if(currentRatio < emptyValue)
            {
                backgroundImage.color = flashBackGroundColorEmpty;
            }
            else
            {
                foregroundImage.color = Color.Lerp(foregroundImage.color, defaultForegroundColor,
                    colorChangeSharpness * Time.deltaTime);
                backgroundImage.color = Color.Lerp(backgroundImage.color,defaultBackGroundColor,
                    colorChangeSharpness* Time.deltaTime);
            }
            
            prevousValue = currentRatio;
        }
    }
}