using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Globalization;

[Serializable]
public struct SpreadPattern
{
    public Vector2[] angles;
    public float spread;
    [TextArea]
    public string patternData;
}

[Serializable]
public struct Weapon
{
    public string name;
    public int baseDamage;
    public float armorRatio;
    public float rangeModifier;
    public int maxRange;
    public float headMultiplier;
    public float chestMultiplier;
    public float stomachMultiplier;
    public float legMultiplier;
    public SpreadPattern[] patterns;
}

public class WeaponLoader : MonoBehaviour
{
    public Material patternMat;
    public Material spreadMat;
    public Material damageMat;
    public bool helmet;
    public bool kevlar;
    public Weapon[] weapons;
    [Range(0, 1)]
    public float time;
    public bool interpolate;
    int spreadPatternIDA = Shader.PropertyToID("_SpreadPatternA");
    int spreadPatternIDB = Shader.PropertyToID("_SpreadPatternB");
    int spreadBlendID = Shader.PropertyToID("_SpreadBlend");
    int countID = Shader.PropertyToID("_Count");
    int spreadID = Shader.PropertyToID("_Radius");
    int helmetRatioID = Shader.PropertyToID("_HelmetRatio");
    int kevlarRatioID = Shader.PropertyToID("_KevlarRatio");
    int damageID = Shader.PropertyToID("_Damage");
    int rangeModID = Shader.PropertyToID("_RangeMod");
    int rangeID = Shader.PropertyToID("_Range");
    int headMulID = Shader.PropertyToID("_HeadMul");
    int chestMulID = Shader.PropertyToID("_ChestMul");
    int stomachMulID = Shader.PropertyToID("_StomachMul");
    int legMulID = Shader.PropertyToID("_LegMul");
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            Weapon sequence = weapons[i];
            for (int j = 0; j < sequence.patterns.Length; j++)
            {
                SpreadPattern pattern = sequence.patterns[j];
                if (pattern.patternData != "")
                {
                    pattern.angles = ParsePatternData(pattern.patternData);
                    Debug.Log(pattern.angles.Length);
                    Debug.Log(sequence.patterns[0].angles.Length);
                    weapons[i].patterns[j].angles = pattern.angles;
                }
            }
        }
    }

    Vector2[] ParsePatternData(string data)
    {
        Debug.Log("Parsing: " + data);
        List<Vector2> output = new List<Vector2>();
        foreach (string line in data.Split('\n'))
        {
            // Debug.Log("Line: " + line + "(" + line.Length + ")");
            string[] angles = line.Split('\t');
            // Debug.Log("Floats: " + angles[0] + ";\t" + angles[0]);
            if (line.Length > 0 && angles.Length == 2)
                output.Add(new Vector2(
                    float.Parse(angles[0], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(angles[1], CultureInfo.InvariantCulture.NumberFormat)));
        }
        return output.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        Weapon weapon = weapons[0];
        float patternTime = (weapon.patterns.Length - 1) * time;
        int patternIndex = (int)patternTime;
        if (patternIndex >= weapon.patterns.Length - 1)
            patternIndex = weapon.patterns.Length - 2;
        float patternBlend = patternTime - patternIndex;
        patternBlend = Mathf.SmoothStep(0, 1, patternBlend);
        if (!interpolate)
            patternBlend = Mathf.Floor(patternBlend);
        SpreadPattern spreadPatternA = weapon.patterns[patternIndex];
        SpreadPattern spreadPatternB = weapon.patterns[patternIndex + 1];
        UpdateSpreadPattern(spreadPatternA, spreadPatternIDA);
        UpdateSpreadPattern(spreadPatternB, spreadPatternIDB);
        patternMat.SetFloat(spreadBlendID, patternBlend);

        spreadMat.SetFloat(spreadID, Mathf.Lerp(spreadPatternA.spread, spreadPatternB.spread, patternBlend));

        UpdateWeaponDamage(weapon);
    }

    void UpdateSpreadPattern(SpreadPattern spreadPattern, int patternID)
    {
        List<Vector4> vec4Array = new List<Vector4>();
        foreach (Vector2 v2 in spreadPattern.angles)
            vec4Array.Add(new Vector4(v2.x, v2.y, 0, 0));
        patternMat.SetVectorArray(patternID, vec4Array);
        patternMat.SetInteger(countID, spreadPattern.angles.Length);
        // Debug.Log("updated spread pattern");
    }

    void UpdateWeaponDamage(Weapon weapon)
    {
        damageMat.SetFloat(helmetRatioID, helmet ? weapon.armorRatio : 2);
        damageMat.SetFloat(kevlarRatioID, kevlar ? weapon.armorRatio : 2);
        damageMat.SetInteger(damageID, weapon.baseDamage);
        damageMat.SetFloat(rangeModID, weapon.rangeModifier);
        damageMat.SetInt(rangeID, weapon.maxRange);
        damageMat.SetFloat(headMulID, weapon.headMultiplier);
        damageMat.SetFloat(chestMulID, weapon.chestMultiplier);
        damageMat.SetFloat(stomachMulID, weapon.stomachMultiplier);
        damageMat.SetFloat(legMulID, weapon.legMultiplier);
    }
}
