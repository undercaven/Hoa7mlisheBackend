using hoa7mlishe.API.DTO.Cards;

namespace hoa7mlishe.API.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Получает файл
        /// </summary>
        /// <param name="id">ИД файла</param>
        /// <param name="height">Высота в пикселях</param>
        /// <returns>Байты файла</returns>
        public byte[] GetFileBytes(Guid id, int height, ref string extension);

        /// <summary>
        /// Добавляет файл в базу
        /// </summary>
        /// <param name="file">Файл</param>
        /// <returns>Результат выполнения</returns>
        public Guid PostCard(CardDTO file);

        public void CreatePack(CardPackPostDTO packDto);

        /// <summary>
        /// Сохраняет файл локально и обновляет файловую таблицу
        /// </summary>
        /// <param name="fileDto">DTO с файлом</param>
        /// <param name="extension">расширение файла</param>
        public Guid SaveInFileTable(CardDTO fileDto, string extension);

        public Guid SaveInFileTable(IFormFile file, int imgHeight = 0, string filename = null);

        public void DeleteFile(Guid fileId);
    }
}
