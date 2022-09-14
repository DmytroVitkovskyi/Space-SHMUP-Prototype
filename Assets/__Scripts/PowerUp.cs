using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerUp : MonoBehaviour
{
    [Header("Set in Inspector")]
    // Необычное, но удобное применение Vector2. x хранит минимальное
    //   значение, а y - максимальное значение для метода Random.Range(),
    //   который будет вызываться позже
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(.25f, 2);
    public float lifeTime = 6f; // Время в секундах существования PowerUp
    public float fadeTime = 4f; // Seconds it will then fade

    [Header("Set Dynamically")]
    public WeaponType type; // Тип бонуса
    public GameObject cube; // Ссылка на вложенный куб
    public TextMeshPro letter; // Ссылка на TextMeshPro
    public Vector3 rotPerSecond;  // Скорость вращения
    public float birthTime;

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Renderer cubeRend;

    private void Awake()
    {
        // Получить ссылку на куб
        cube = transform.Find("Cube").gameObject;
        // Получить ссылки на TextMeshPro и другие компоненты
        letter = GetComponent<TextMeshPro>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeRend = cube.GetComponent<Renderer>();

        // Выбрать случайную скорость
        Vector3 vel = Random.onUnitSphere; // Получить случайную скорость XYZ
        vel.z = 0;
        vel.Normalize();
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        rigid.velocity = vel;

        transform.rotation = Quaternion.identity; // set R:[0,0,0]

        // Выбрать случайную скорость вращения для вложенного куба
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y),
            Random.Range(rotMinMax.x, rotMinMax.y),
            Random.Range(rotMinMax.x, rotMinMax.y));
        birthTime = Time.time;
    }
    // Update is called once per frame
    void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);

        // Эффект растворения куба PowerUp с течением времени
        // Со значениями по умолчанию бонус существует 10 секунд
        //   а затем растворяется в течении 4 секунд
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;
        // в течении lifeTime секунд значение u будет <= 0. Затем оно станет
        //   положительным и через fadeTime секунд станет больше 1.

        // Если u >= 1, уничтожить бонус
        if (u > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        // Использовать u для определения альфа-значения куба и буквы
        if (u > 0)
        {
            Color c = cubeRend.material.color;
            c.a = 1f - u;
            cubeRend.material.color = c;
            // Буква тоже должна растворяться, но медленнее
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
        if (!bndCheck.isOnScreen)
        {
            // Если бонус полностью вышел за границу экрана, уничтожить его
            Destroy(gameObject);
        }
    }

    public void SetType(WeaponType wt)
    {
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        cubeRend.material.color = def.color;
        letter.text = def.letter;
        type = wt;
    }
    /// <summary>
    /// Функция вызывается классом Hero, когда игрок подбирает бонус
    /// </summary>
    /// <param name="target"></param>
    public void AbsorbedBy(GameObject target)
    {
        Destroy(this.gameObject);
    }
}
