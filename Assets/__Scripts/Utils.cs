using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    // ===============================������� ��� ������ � �����������===============================\\
    
    // ���������� ������ ���� ���������� � ������ ������� �������
    //  � ��� �������� ��������
    static public Material[] GetAllMaterials(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();

        List<Material> mats = new List<Material>();
        foreach (var item in rends)
        {
            mats.Add(item.material);
        }
        return mats.ToArray();
    }
}
