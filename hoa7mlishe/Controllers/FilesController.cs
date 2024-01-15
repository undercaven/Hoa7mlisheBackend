using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Helpers;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    /// <summary>
    /// Контроллер для операций с файлами
    /// </summary>
    /// <param name="context">Контекст БД</param>
    /// <param name="fileService">Файловый сервис</param>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class FilesController(Hoa7mlisheContext context, IFileService fileService) : ControllerBase
    {
        private readonly Hoa7mlisheContext _context = context;
        private readonly IFileService _fileService = fileService;

        /// <summary>
        /// Проверяет существование файла
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <returns></returns>
        [HttpGet("exists")]
        public IActionResult CheckFileExists(
        [FromQuery] Guid fileId)
        {
            string JpgPath = Path.Combine(FileService.filePath, fileId.ToString() + ".jpg");
            string PngPath = Path.Combine(FileService.filePath, fileId.ToString() + ".png");
            var result = new
            {
                JpgPath,
                PngPath,
                JpgExists = System.IO.File.Exists(JpgPath),
                PngExists = System.IO.File.Exists(PngPath),
            };

            return (Ok(result));
        }

        /// <summary>
        /// Получает файл
        /// </summary>
        /// <param name="id">ИД файла</param>
        /// <param name="height">Высота в пикселях</param>
        /// <returns>Байты файла</returns>
        [HttpGet("{id}")]
        public IActionResult GetFile(
            Guid id,
            [FromQuery] int height)
        {
                string extension = string.Empty;
                var bytes = _fileService.GetFileBytes(id, height, ref extension);
                return File(bytes, Dictionaries.ExtensionToDataFormat[extension]);
        }

        //[HttpPut("update/all")]
        //public IActionResult UpdateAll()
        //{
        //    List<CardInfo> newcards = new List<CardInfo>();
        //    foreach (var file in new Hoa7mlisheContext().FileInfos.ToList())
        //    {
        //        var fileHoa = new Hoa7mlisheContext().Hoa7mlisheFiles.First(x => x.PathLocator == file.PathLocator);
        //        new Hoa7mlisheContext().FileInfos.Where(x => x.Id == file.Id).ExecuteDelete();
        //        var newfile = file.Clone();
        //        newfile.Id = new Guid(Path.GetFileNameWithoutExtension(fileHoa.Name));
        //        newcards.Add(newfile);
        //    }

        //    new Hoa7mlisheContext().FileInfos.RemoveRange(new Hoa7mlisheContext().FileInfos.ToList());
        //    new Hoa7mlisheContext().FileInfos.AddRange(newcards);

        //    new Hoa7mlisheContext().SaveChanges();
        //    return Ok();
        //}
    }
}
