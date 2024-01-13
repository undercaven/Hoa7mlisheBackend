namespace hoa7mlishe.Interfaces
{
    public interface IPlugin
    {
        public void Load(IServiceProvider serviceProvider);
        public void Unload();
        public static string Name { get; }
    }
}