using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сериализуемый класс для хранения данных
/// </summary>
[System.Serializable]
public class Part
{
    // Значения этих трёх полей должны определяться в инспекторе
    public string name; // Имя этой части
    public float health; // Степень стойкости этой части
    public string[] protectedBy; // Другие части, защищающие эту

    // Эти два поля инициализируются автоматически в Start().
    // Кэширование, как здесь, ускоряет получение необходимых данных.
    [HideInInspector] // Не позволяет следующему полю появиться в инспекторе
    public GameObject go; // Игровой объект этой части
    [HideInInspector]
    public Material mat;  // Материал для отображения повреждений
}


/// <summary>
/// Enemy_4 создаётся за верхней границей, выбирает случайную точку на экране
///    и перемещается к ней. Добравшись до места, выбирает другую случайную точку
///    и продолжает двигаться, пока игрок не уничтожит его. 
/// </summary>
public class Enemy_4 : Enemy
{
    [Header("Set In Inspector: Enemy_4")]
    public Part[] parts; // Массив частей, составляющих корабль

    private Vector3 p0, p1; // Две точки для интерполяции
    private float timeStart;  // Время создания этого корабля
    private float duration = 4f;  // Продолжительность перемещения
    // Start is called before the first frame update
    void Start()
    {
        // Начальная позиция уже выбрана в Main.SpawnEnemy(),
        //   поэтому запишем её как начальные значения в p0 и p1
        p0 = p1 = pos;
        InitMovement();

        // Записать в кэш игровой объект и материал каждой части в parts
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
        // Выбрать новую точку p1 на экране
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Сбросить время
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

        u = 1 - Mathf.Pow(1 - u, 2); // Применить плавное замедление
        pos = (1 - u) * p0 + u * p1; // Простая линейная интерполяция
    }

    // Эти две функции выполняют поиск части в массиве parts n
    //  по имени или по ссылке на игровой объект
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

    // Эти функции возвращают true, если данная часть уничтожена
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
