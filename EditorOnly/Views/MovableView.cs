namespace GraphFramework.Editor
{
    public interface MovableView
    {
        void OnDirty();
        void Display();

        MovableModel GetModel();
    }
}