using UnityEngine;

[ExecuteInEditMode] // Позволяет выполнять скрипт в редакторе
public class AddCollidersToChildren : MonoBehaviour
{
    // Атрибут добавляет кнопку в инспекторе
    [ContextMenu("Добавить Mesh Collider всем детям")]
    private void AddMeshCollidersToChildren()
    {
        // Ищем все компоненты MeshFilter у объекта и его детей
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        
        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Пропускаем родительский объект, если на нем нет MeshFilter
            if (meshFilter == null) continue;

            // Получаем игровой объект
            GameObject childObject = meshFilter.gameObject;
            
            // Проверяем, есть ли уже коллайдер, чтобы не дублировать
            if (childObject.GetComponent<Collider>() == null)
            {
                // Добавляем Mesh Collider и используем Mesh из MeshFilter
                MeshCollider collider = childObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilter.sharedMesh;
                Debug.Log($"Коллайдер добавлен на: {childObject.name}");
            }
            else
            {
                Debug.Log($"На объекте {childObject.name} уже есть коллайдер, пропускаем.");
            }
        }
        
        Debug.Log("Готово! Коллайдеры добавлены всем дочерним объектам.");
    }
}