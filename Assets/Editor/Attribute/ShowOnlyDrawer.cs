using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string valueStr;
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                valueStr = property.intValue.ToString();
                break;
            case SerializedPropertyType.Float:
                valueStr = property.floatValue.ToString("0.000000");
                break;
            case SerializedPropertyType.AnimationCurve:
                valueStr = property.animationCurveValue.ToString();
                break;
            case SerializedPropertyType.LayerMask:
                valueStr = LayerMask.LayerToName(property.intValue);
                break;
            case SerializedPropertyType.Boolean:
                valueStr = property.boolValue.ToString();
                break;
            case SerializedPropertyType.String:
                valueStr = property.stringValue.ToString();
                break;
            case SerializedPropertyType.Vector2:
                valueStr = property.vector2Value.ToString();
                break;
            case SerializedPropertyType.Vector3:
                valueStr = property.vector3Value.ToString();
                break;
            case SerializedPropertyType.Vector4:
                valueStr = property.vector4Value.ToString();
                break;
            case SerializedPropertyType.Vector2Int:
                valueStr = property.vector2IntValue.ToString();
                break;
            case SerializedPropertyType.Vector3Int:
                valueStr = property.vector3IntValue.ToString();
                break;
            case SerializedPropertyType.Enum:
                valueStr = property.enumValueFlag.ToString();
                break;
            case SerializedPropertyType.Bounds:
                valueStr = property.boundsValue.ToString();
                break;
            case SerializedPropertyType.BoundsInt:
                valueStr = property.boundsIntValue.ToString();
                break;
            case SerializedPropertyType.Color:
                valueStr = property.colorValue.ToString();
                break;
            case SerializedPropertyType.Quaternion:
                valueStr = property.quaternionValue.ToString();
                break;
            default:
                valueStr = property.displayName;
                break;
        }

        valueStr = "<color=grey>" + valueStr + "</color>";
        GUIStyle style = new GUIStyle();
        style.richText = true;
        EditorGUI.LabelField(position, label.text, valueStr, style);
    }
}
