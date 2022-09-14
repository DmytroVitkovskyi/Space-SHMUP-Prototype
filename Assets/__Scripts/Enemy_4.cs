using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������������� ����� ��� �������� ������
/// </summary>
[System.Serializable]
public class Part
{
    // �������� ���� ��� ����� ������ ������������ � ����������
    public string name; // ��� ���� �����
    public float health; // ������� ��������� ���� �����
    public string[] protectedBy; // ������ �����, ���������� ���

    // ��� ��� ���� ���������������� ������������� � Start().
    // �����������, ��� �����, �������� ��������� ����������� ������.
    [HideInInspector] // �� ��������� ���������� ���� ��������� � ����������
    public GameObject go; // ������� ������ ���� �����
    [HideInInspector]
    public Material mat;  // �������� ��� ����������� �����������
}


/// <summary>
/// Enemy_4 �������� �� ������� ��������, �������� ��������� ����� �� ������
///    � ������������ � ���. ���������� �� �����, �������� ������ ��������� �����
///    � ���������� ���������, ���� ����� �� ��������� ���. 
/// </summary>
public class Enemy_4 : Enemy
{
    [Header("Set In Inspector: Enemy_4")]
    public Part[] parts; // ������ ������, ������������ �������

    private Vector3 p0, p1; // ��� ����� ��� ������������
    private float timeStart;  // ����� �������� ����� �������
    private float duration = 4f;  // ����������������� �����������
    // Start is called before the first frame update
    void Start()
    {
        // ��������� ������� ��� ������� � Main.SpawnEnemy(),
        //   ������� ������� � ��� ��������� �������� � p0 � p1
        p0 = p1 = pos;
        InitMovement();

        // �������� � ��� ������� ������ � �������� ������ ����� � parts
        Transform t;
        foreach (var item in parts)
        {
            t = transform.Find(item.name);
            if (t != null)
            {
                item.go = t.gameObject;
                item.mat = item.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        p0 = p1;
        // ������� ����� ����� p1 �� ������
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // �������� �����
        timeStart = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); // ��������� ������� ����������
        pos = (1 - u) * p0 + u * p1; // ������� �������� ������������
    }

    // ��� ��� ������� ��������� ����� ����� � ������� parts n
    //  �� ����� ��� �� ������ �� ������� ������
    Part FindPart(string n)
    {
        foreach (var item in parts)
        {
            if (item.name == n)
            {
                return item;
            }
        }
        return null;
    }

    Part FindPart(GameObject go)
    {
        foreach (var item in parts)
        {
            if (item.go == go)
            {
                return item;
            }
        }
        return null;
    }

    // ��� ������� ���������� true, ���� ������ ����� ����������
    bool Destroyed(GameObject go)
    {
        return (Destroyed(FindPart(go)));
    }
    bool Destroyed(string n)
    {
        return (Destroyed(FindPart(n)));
    }
    bool Destroyed(Part prt)
    {
        if (prt == null)
        {
            return true;
        }
        return (prt.health <= 0);
    }
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }

                GameObject goHit = collision.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null)
                {
                    goHit = collision.contacts[0].thisCollider.gameObject;
                    prtHit = FindPart(goHit);
                }
                if (prtHit.protectedBy != null)
                {
                    foreach (var item in prtHit.protectedBy)
                    {
                        if (!Destroyed(item))
                        {
                            Destroy(other);
                            return;
                        }
                    }
                }

                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0)
                {
                    prtHit.go.SetActive(false);
                }
                bool allDestroyed = true;
                foreach (var item in parts)
                {
                    if (!Destroyed(item))
                    {
                        allDestroyed = false;
                        break;
                    }
                }
                if (allDestroyed)
                {
                    Main.S.ShipDestroyed(this);
                    Destroy(this.gameObject);
                }
                Destroy(other);
                break;
        }
    }
}
