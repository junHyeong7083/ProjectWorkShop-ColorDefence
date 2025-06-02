using System;
using UnityEngine;

namespace MagicPigGames
{
    [Serializable]
    public class HorizontalProgressBar : ProgressBar
    {
        /// <summary>
        /// ���� �ڽ�Ʈ (0~10)
        /// </summary>
        public int CurrentCost => Mathf.FloorToInt(Progress * 10f);

        /// <summary>
        /// �ִ� �ڽ�Ʈ
        /// </summary>
        public int MaxCost => 10;

        private void Awake()
        {
            SetProgress(0f);
        }

        /// <summary>
        /// �ܺο��� �ڽ�Ʈ�� ���� �߰��ϰų� ���ҽ�Ű�� ���� ������ ���� ����
        /// GameManager���� �����ϹǷ� ������ ��κ� ȣ����� ����
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
