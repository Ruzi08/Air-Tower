public interface Interactable
{
    // Этот метод будет вызываться, когда игрок кликает ЛКМ по объекту
    void Interact();
    
    // (Опционально) Текст, который можно выводить на экран при наведении
    string GetDescription(); 
}