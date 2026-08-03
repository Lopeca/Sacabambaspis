using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class LevelRecordPanel : MonoBehaviour
{
    [SerializeField] private UserRuntimeDataSO userData;
    [SerializeField] private List<TMP_Text> recordTextList;

    public void ShowRecords(OriginalLevelData data)
    {
        // 내부에 시간순 정렬 및 최대 5개로 제한되어 들어옴
        IReadOnlyList<float> records = userData.GetRecords(data.levelAddress.AssetGUID);

        for (int i = 0; i < recordTextList.Count; i++)
        {
            // TextMeshPro 컴포넌트 연결이 빠진 경우 방어 코드
            if (recordTextList[i] == null) continue;

            // 기록 데이터가 존재하는 경우
            if (records != null && i < records.Count)
            {
                // float 초(second) 데이터를 TimeSpan으로 변환
                TimeSpan timeSpan = TimeSpan.FromSeconds(records[i]);

                // mm: 분(2자리), ss: 초(2자리), fff: 밀리초(3자리)
                recordTextList[i].text = timeSpan.ToString(@"mm\:ss\.fff");
            }
            else
            {
                // 기록이 없는 빈 슬롯 표기
                recordTextList[i].text = "--:--.---";
            }
        }
    }
}
