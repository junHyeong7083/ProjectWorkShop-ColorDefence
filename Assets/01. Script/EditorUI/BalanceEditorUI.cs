using UnityEngine;
using UnityEngine.UI;

public class BalanceEditorUI : MonoBehaviour
{
 //   [BigHeader("SODatas")]
    public TurretData gatlingData;
    public TurretData cannonData;
    public TurretData laserData;

  //  [BigHeader("Turret Data")]
    #region
    [Header("Gatling")]
    public InputField gatlingAttackRange_InputFiled;
    public Button gatlingAttackRange_Button;

    public InputField gatlingAttackRate_InputField;
    public Button gatlingAttackRate_Button;

    public InputField gatlingMinRange_InputFiled;
    public Button gatlingMinRange_Button;

    public InputField gatlingDamage_InputFiled;
    public Button gatlingDamage_Button;

    public InputField gatlingWidth_InputFiled;
    public Button gatlingWidth_Button;

    public InputField gatlingHeight_InputFiled;
    public Button gatlingHeight_Button;

    [Header("Cannon")]
    public InputField cannonAttackRange_InputFiled;
    public Button cannonAttackRange_Button;

    public InputField cannonAttackRate_InputField;
    public Button cannonAttackRate_Button;

    public InputField cannonMinRange_InputFiled;
    public Button cannonMinRange_Button;

    public InputField cannonDamage_InputFiled;
    public Button cannonDamage_Button;

    public InputField cannonWidth_InputFiled;
    public Button cannonWidth_Button;

    public InputField cannonHeight_InputFiled;
    public Button cannonHeight_Button;

    [Header("Laser")]
    public InputField laserAttackRange_InputFiled;
    public Button laserAttackRange_Button;

    public InputField laserAttackRate_InputField;
    public Button laserAttackRate_Button;

    public InputField laserMinRange_InputFiled;
    public Button laserMinRange_Button;

    public InputField laserDamage_InputFiled;
    public Button laserDamage_Button;

    public InputField laserWidth_InputFiled;
    public Button laserWidth_Button;

    public InputField laserHeight_InputFiled;
    public Button laserHeight_Button;
    #endregion
//    [Header("Gatling Text")]
    public Text gatlingAttackRange_Text;
    public Text gatlingAttackRate_Text;
    public Text gatlingMinRange_Text;
    public Text gatlingDamage_Text;
    public Text gatlingWidth_Text;
    public Text gatlingHeight_Text;

    [Header("Cannon Text")]
    public Text cannonAttackRange_Text;
    public Text cannonAttackRate_Text;
    public Text cannonMinRange_Text;
    public Text cannonDamage_Text;
    public Text cannonWidth_Text;
    public Text cannonHeight_Text;

    [Header("Laser Text")]
    public Text laserAttackRange_Text;
    public Text laserAttackRate_Text;
    public Text laserMinRange_Text;
    public Text laserDamage_Text;
    public Text laserWidth_Text;
    public Text laserHeight_Text;


    private void Start()
    {
        // Gatling
        gatlingAttackRange_Button.onClick.AddListener(() => ApplyFloat(gatlingAttackRange_InputFiled, f => gatlingData.baseAttackRange = f));
        gatlingAttackRate_Button.onClick.AddListener(() => ApplyFloat(gatlingAttackRate_InputField, f => gatlingData.baseAttackRate = f));
        gatlingMinRange_Button.onClick.AddListener(() => ApplyInt(gatlingMinRange_InputFiled, f => gatlingData.minAttackRange = f));
        gatlingDamage_Button.onClick.AddListener(() => ApplyInt(gatlingDamage_InputFiled, f => gatlingData.baseDamage = f));
        gatlingWidth_Button.onClick.AddListener(() => ApplyInt(gatlingWidth_InputFiled, f => gatlingData.width = f));
        gatlingHeight_Button.onClick.AddListener(() => ApplyInt(gatlingHeight_InputFiled, f => gatlingData.height = f));

        // Cannon
        cannonAttackRange_Button.onClick.AddListener(() => ApplyFloat(cannonAttackRange_InputFiled, f => cannonData.baseAttackRange = f));
        cannonAttackRate_Button.onClick.AddListener(() => ApplyFloat(cannonAttackRate_InputField, f => cannonData.baseAttackRate = f));
        cannonMinRange_Button.onClick.AddListener(() => ApplyInt(cannonMinRange_InputFiled, f => cannonData.minAttackRange = f));
        cannonDamage_Button.onClick.AddListener(() => ApplyInt(cannonDamage_InputFiled, f => cannonData.baseDamage = f));
        cannonWidth_Button.onClick.AddListener(() => ApplyInt(cannonWidth_InputFiled, f => cannonData.width = f));
        cannonHeight_Button.onClick.AddListener(() => ApplyInt(cannonHeight_InputFiled, f => cannonData.height = f));

        // Laser
        laserAttackRange_Button.onClick.AddListener(() => ApplyFloat(laserAttackRange_InputFiled, f => laserData.baseAttackRange = f));
        laserAttackRate_Button.onClick.AddListener(() => ApplyFloat(laserAttackRate_InputField, f => laserData.baseAttackRate = f));
        laserMinRange_Button.onClick.AddListener(() => ApplyInt(laserMinRange_InputFiled, f => laserData.minAttackRange = f));
        laserDamage_Button.onClick.AddListener(() => ApplyInt(laserDamage_InputFiled, f => laserData.baseDamage = f));
        laserWidth_Button.onClick.AddListener(() => ApplyInt(laserWidth_InputFiled, f => laserData.width = f));
        laserHeight_Button.onClick.AddListener(() => ApplyInt(laserHeight_InputFiled, f => laserData.height = f));

        UpdateUI();
    }

    private void ApplyFloat(InputField field, System.Action<float> setter)
    {
        if (float.TryParse(field.text, out float result))
        {
            setter(result);
            Debug.Log($"입력 성공: {result}");
        }
        else
        {
            Debug.LogWarning($"입력 실패: {field.text}");
        }
        UpdateUI();
    }

    private void ApplyInt(InputField field, System.Action<int> setter)
    {
        if (int.TryParse(field.text, out int result))
        {
            setter(result);
            Debug.Log($"입력 성공 (int): {result}");
        }
        else
        {
            Debug.LogWarning($"입력 실패 (int): {field.text}");
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Gatling
        gatlingAttackRange_Text.text = "AttackRange : " + gatlingData.baseAttackRange.ToString("F2");
        gatlingAttackRate_Text.text = "AttackRate : " + gatlingData.baseAttackRate.ToString("F2");
        gatlingMinRange_Text.text = "MinRange : " + gatlingData.minAttackRange.ToString();
        gatlingDamage_Text.text = "Damage : " + gatlingData.baseDamage.ToString();
        gatlingWidth_Text.text = "Width : " + gatlingData.width.ToString();
        gatlingHeight_Text.text = "Height : " + gatlingData.height.ToString();

        // Cannon
        cannonAttackRange_Text.text = "AttackRange : " + cannonData.baseAttackRange.ToString("F2");
        cannonAttackRate_Text.text = "AttackRate : " + cannonData.baseAttackRate.ToString("F2");
        cannonMinRange_Text.text = "MinRange : " + cannonData.minAttackRange.ToString();
        cannonDamage_Text.text = "Damage : " + cannonData.baseDamage.ToString();
        cannonWidth_Text.text = "Width : " + cannonData.width.ToString();
        cannonHeight_Text.text = "Height : " + cannonData.height.ToString();

        // Laser
        laserAttackRange_Text.text = "AttackRange : " + laserData.baseAttackRange.ToString("F2");
        laserAttackRate_Text.text = "AttackRate : " + laserData.baseAttackRate.ToString("F2");
        laserMinRange_Text.text = "MinRange : " + laserData.minAttackRange.ToString();
        laserDamage_Text.text = "Damage : " + laserData.baseDamage.ToString();
        laserWidth_Text.text = "Width : " + laserData.width.ToString();
        laserHeight_Text.text = "Height : " + laserData.height.ToString();
    }


}
