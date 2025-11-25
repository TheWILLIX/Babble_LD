//credits to http://answers.unity.com/answers/1383657/view.html

using UnityEngine;
using Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class LabelOverride : PropertyAttribute
{
    public string label;
    public float? rangeMin;
    public float? rangeMax;
    public string rangeMinS;
    public string rangeMaxS;
    public bool forceInt;
    public string forceIntS;
    public string fallbackIntProperty;
    public LabelOverride(string label)
    {
        this.label = label;
        rangeMin = null;
        rangeMax = null;
        this.forceInt = false;
        this.rangeMinS = "";
        this.rangeMaxS = "";
        this.fallbackIntProperty = "";
        this.forceIntS = "";
    }
    public LabelOverride(string label, float rangeMin, float rangeMax, bool forceInt = false)
    {
        this.label = label;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
        this.forceInt = forceInt;
        this.rangeMinS = "";
        this.rangeMaxS = "";
        this.fallbackIntProperty = "";
        this.forceIntS = "";
    }
    public LabelOverride(string label, string rangeMin, string rangeMax, string fallbackIntProperty, bool forceInt = false)
    {
        this.label = label;
        this.rangeMinS = rangeMin;
        this.rangeMaxS = rangeMax;
        this.forceInt = forceInt;
        this.fallbackIntProperty = fallbackIntProperty;
        this.rangeMin = null;
        this.rangeMax = null;
        this.forceIntS = "";
    }
    public LabelOverride(string label, string rangeMin, string rangeMax, string fallbackIntProperty, string forceInt)
    {
        this.label = label;
        this.rangeMinS = rangeMin;
        this.rangeMaxS = rangeMax;
        this.forceIntS = forceInt;
        this.fallbackIntProperty = fallbackIntProperty;
        this.rangeMin = null;
        this.rangeMax = null;
        this.forceInt = false;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LabelOverride))]
    public class ThisPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                var propertyAttribute = this.attribute as LabelOverride;
                if (IsItBloodyArrayTho(property) == false)
                {
                    label.text = propertyAttribute.label;

                }
                else
                {
                    Debug.LogWarningFormat(
                        "{0}(\"{1}\") doesn't support arrays ",
                        typeof(LabelOverride).Name,
                        propertyAttribute.label
                    );
                }
                if (propertyAttribute.rangeMin.HasValue)
                {
                    if (property.propertyType == SerializedPropertyType.Float && !propertyAttribute.forceInt)
                        EditorGUI.Slider(position, property, propertyAttribute.rangeMin.Value, propertyAttribute.rangeMax.Value, label);

                    else if(property.propertyType == SerializedPropertyType.Integer || propertyAttribute.forceInt)
                        EditorGUI.IntSlider(position, property, (int)propertyAttribute.rangeMin.Value, (int)propertyAttribute.rangeMax.Value, label);
                } else if (propertyAttribute.rangeMinS != "")
                {
                    if(propertyAttribute.forceIntS != "")
                        propertyAttribute.forceInt = property.serializedObject.FindProperty(propertyAttribute.forceIntS).GetValue<bool>();
                    var min = property.serializedObject.FindProperty(propertyAttribute.rangeMinS).GetValue<float>();
                    var max = property.serializedObject.FindProperty(propertyAttribute.rangeMaxS).GetValue<float>();

                    if(property.propertyType == SerializedPropertyType.Float && propertyAttribute.forceInt)
                    {
                        var fallbackProperty = property.serializedObject.FindProperty(propertyAttribute.fallbackIntProperty);
                        EditorGUI.IntSlider(position, fallbackProperty, (int)min, (int)max, label);
                    }
                    else if (property.propertyType == SerializedPropertyType.Float)
                        EditorGUI.Slider(position, property, min, max, label);
                    else if (property.propertyType == SerializedPropertyType.Integer)
                        EditorGUI.IntSlider(position, property, (int)min, (int)max, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label);
                }
                
            }
            catch (System.Exception ex) { Debug.LogException(ex); }
        }
        bool IsItBloodyArrayTho(SerializedProperty property)
        {
            string path = property.propertyPath;
            int idot = path.IndexOf('.');
            if (idot == -1) return false;
            string propName = path.Substring(0, idot);
            SerializedProperty p = property.serializedObject.FindProperty(propName);
            return p.isArray;
            //CREDITS: https://answers.unity.com/questions/603882/serializedproperty-isnt-being-detected-as-an-array.html
        }
    }
#endif
}