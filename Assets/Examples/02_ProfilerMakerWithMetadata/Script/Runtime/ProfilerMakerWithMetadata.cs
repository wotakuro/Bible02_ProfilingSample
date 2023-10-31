using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class ProfilerMakerWithMetadata : MonoBehaviour
{

    static readonly ProfilerMarker<int> profilerMarker = new ProfilerMarker<int>("Listの処理","要素数");
    private List<float> m_Items = new List<float>();
    public int m_ItemCount = 10000;

    // Update is called once per frame
    void Update()
    {
        SetupItems();

        using (profilerMarker.Auto(m_Items.Count))
        {
            for(int i = 0; i < m_ItemCount; ++i)
            {
                m_Items[i] = Random.Range(0.0f, 9999.9f);
            }
            m_Items.Sort();
            // 
            System.Threading.Thread.Sleep(1);
        }
    }

    // アイテム数の調整
    void SetupItems()
    {
        if( m_Items.Count > m_ItemCount)
        {
            int count =  m_Items.Count - m_ItemCount;
            m_Items.RemoveRange( m_Items.Count - count, count);
        }else if ( m_ItemCount > m_Items.Count)
        {
            int count = m_ItemCount - m_Items.Count;
            for (int i = 0; i < count; ++i)
            {
                m_Items.Add(0.0f);
            }

        }
    }
}
