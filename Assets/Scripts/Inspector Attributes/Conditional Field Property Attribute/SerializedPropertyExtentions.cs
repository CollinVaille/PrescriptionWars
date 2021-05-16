using UnityEditor;

namespace Utilities
{
    #region Editor
#if UNITY_EDITOR
    public static class SerializedPropertyExtentions
    {
	    public static T GetValue<T>(this SerializedProperty property)
        {
            return ReflectionUtil.GetNestedObject<T>(property.serializedObject.targetObject, property.propertyPath);
        }
    }
#endif
#endregion
}