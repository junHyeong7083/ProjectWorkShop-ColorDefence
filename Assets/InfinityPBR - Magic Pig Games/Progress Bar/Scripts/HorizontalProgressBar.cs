using System;
using UnityEngine;

namespace MagicPigGames
{
    [Serializable]
    public class HorizontalProgressBar : ProgressBar
    {
        /// <summary>
        /// 현재 코스트 (0~10)
        /// </summary>
        public int CurrentCost => Mathf.FloorToInt(Progress * 10f);

        /// <summary>
        /// 최대 코스트
        /// </summary>
        public int MaxCost => 10;

        private void Awake()
        {
            SetProgress(0f);
        }

        /// <summary>
        /// 외부에서 코스트를 직접 추가하거나 감소시키기 위해 제공할 수도 있음
        /// GameManager에서 관리하므로 이쪽은 대부분 호출되지 않음
        /// </summary>
        public void AddCostUnit(int unit)
        {
            float ratio = (float)unit / MaxCost;
            SetProgress(Mathf.Clamp01(Progress + ratio));
        }

        public void SubtractCostUnit(int unit)
        {
            float ratio = (float)unit / MaxCost;
            SetProgress(Mathf.Clamp01(Progress - ratio));
        }
    }
}
