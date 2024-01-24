using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;


// ProfilerMakerでMetadataの埋め込みのサンプルです
public class ProfilerMakerWithMetadata : MonoBehaviour
{

    // int型のMetadataを持つProfielrMakerをGenericで宣言します
    static readonly ProfilerMarker<int> profilerMarker = new ProfilerMarker<int>("Listの処理","要素数");

    // ランダムに値を入れてソートする処理を行うためのListです
    private List<float> m_Items = new List<float>();
    // アイテム数の指定
    public int m_ItemCount = 10000;

    // Update処理
    void Update()
    {
        // アイテムリストの初期化処理
        SetupItems();

        // AutoにItem数を載せることで、アイテム数を載せています
        using (profilerMarker.Auto(m_Items.Count))
        {
            // 実際に行う処理、ランダムに値を入れてソートしています。
            for(int i = 0; i < m_ItemCount; ++i)
            {
                m_Items[i] = Random.Range(0.0f, 9999.9f);
            }
            m_Items.Sort();
            // ※ Profiler上で発見しやすくするために 1msスリープします
            System.Threading.Thread.Sleep(1);
        }
    }

    // アイテム数自体が変動したときに対応するための処理
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
