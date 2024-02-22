using Blurhash;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Cards;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.Helpers;
using System.Drawing;
using System.Drawing.Imaging;

namespace hoa7mlishe.Services
{
    public class FileService : IFileService
    {
        private Hoa7mlisheContext _context;
#if DEBUG
        internal static string filePath => "\\\\Desktop-b6dcgqi\\hoaserver_dev\\HoaFileContainer\\HoaFileTable";
#else
        internal static string filePath => "\\\\Hoaserver-1\\hoaserver_prod\\HoaFileContainer\\HoaFileTable";
#endif
        public FileService(Hoa7mlisheContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает файл
        /// </summary>
        /// <param name="id">ИД файла</param>
        /// <param name="height">Высота в пикселях</param>
        /// <param name="extension">Расширение файла</param>
        /// <returns>Байты файла</returns>
        public byte[] GetFileBytes(
            Guid id, int height, ref string extension)
        {
            FileInterface fileInfo = _context.FileInterfaces.First(x => x.RecordId == id)
                ?? throw new Exception("FileNotFound");

            Hoa7mlisheFile fileRecord = _context.Hoa7mlisheFiles.FirstOrDefault(x => x.PathLocator == fileInfo.PathLocator) 
                ?? throw new Exception("FileNotFound");

            string fileName = Path.Combine(filePath, fileRecord.Name);

            byte[] bytes;
            extension = Path.GetExtension(fileRecord.Name).Trim('.');

            if (extension == "gif")
            {
                return File.ReadAllBytes(fileName);
            }

            Image imgToSend;
            FileStream fs = new(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                imgToSend = FileIOHelper.ResizeImage(Image.FromStream(fs), height);
                fs.Close();
            var converter = new ImageConverter();
            bytes = converter.ConvertTo(imgToSend, typeof(byte[])) as byte[];

            return bytes;
        }

        /// <summary>
        /// Добавляет файл в базу
        /// </summary>
        /// <param name="file">Файл</param>
        /// <returns>Результат выполнения</returns>
        public Guid PostCard(CardDTO file)
        {
            string extension = Path.GetExtension(file.File.FileName);
            if (extension != ".jpg" && extension != ".png" && extension != ".gif")
            {
                throw new IOException("Only .jpg and .png (or gif)!!");
            }

            return SaveInFileTable(file, extension);
        }

        /// <summary>
        /// Сохраняет файл локально и обновляет файловую таблицу
        /// </summary>
        /// <param name="fileDto">DTO с файлом</param>
        /// <param name="extension">расширение файла</param>
        public Guid SaveInFileTable(CardDTO fileDto, string extension)
        {
            Guid guid = Guid.NewGuid();

            string newFilename = $"{guid}{extension}";
            string fullPath = Path.Combine(filePath, newFilename);
            int w, h;
            string previewHash;

            using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                fileDto.File.CopyTo(fs);

                using (var image = Image.FromStream(fs))
                {
                    w = image.Width;
                    h = image.Height;
                    previewHash = GetPreviewHash(image);
                }
                fs.Close();
            }

            var file = _context.Hoa7mlisheFiles.First(x => x.Name == newFilename);
            var fileRecord = new FileInterface()
            {
                PathLocator = file.PathLocator,
                RecordId = guid,
            };

            var activeSeason = _context.CardSeasons.Where(x => x.ActiveSeason == true).First();

            var fileInfo = new CardInfo()
            {
                Id = Guid.NewGuid(),
                Description = fileDto.Description,
                Tag = fileDto.Tag,
                Height = h,
                Width = w,
                Rarity = fileDto.Rarity,
                SeasonId = activeSeason.Id,
                PreviewHash = previewHash,
                FileId = fileRecord.RecordId
            };

            _context.FileInfos.Add(fileInfo);
            _context.FileInterfaces.Add(fileRecord);
            _context.SaveChanges();

            return fileInfo.Id;
        }

        /// <summary>
        /// Сохраняет файл и создает запись в файловой таблице
        /// </summary>
        /// <param name="file">Файл</param>
        /// <param name="imgHeight">Высота изображения</param>
        /// <param name="filename">Имя файла</param>
        /// <returns>Идентификатор записи</returns>
        public Guid SaveInFileTable(IFormFile file, int imgHeight = 0, string filename = null)
        {
            Guid guid = Guid.NewGuid();
            string extension = Path.GetExtension(file.FileName);

            filename ??= $"{guid}{extension}";
            
            string fullPath = Path.Combine(filePath, filename);
            string previewHash;

            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var image = Image.FromStream(ms);


            if (imgHeight > 0)
            {
                image = FileIOHelper.ResizeImage(image, imgHeight);
            }

            switch (extension.ToLower())
            {
                case ".gif":
                    image.Save(fullPath, ImageFormat.Gif);
                    break;
                default:
                    image.Save(fullPath);
                    break;
            }

            previewHash = GetPreviewHash(image);

            var fileInfo = _context.Hoa7mlisheFiles.First(x => x.Name == filename);
            var fileRecord = new FileInterface()
            {
                PathLocator = fileInfo.PathLocator,
                RecordId = guid,
            };

            _context.FileInterfaces.Add(fileRecord);
            _context.SaveChanges();

            return fileRecord.RecordId;
        }

        /// <summary>
        /// Удаляет файл и запись в файловой таблице
        /// </summary>
        /// <param name="fileId">Идентификатор записи</param>
        public void DeleteFile(Guid fileId)
        {
            var fileInterface = _context.FileInterfaces.FirstOrDefault(x => x.RecordId == fileId);
            if (fileInterface == null)
            {
                return;
            }

            var fileInfo = _context.Hoa7mlisheFiles.FirstOrDefault(x => x.PathLocator == fileInterface.PathLocator);
            if (fileInfo == null)
            {
                return;
            }

            string path = Path.Combine(filePath, fileInfo.Name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Вычисляет хеш для превью изображения
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static string GetPreviewHash(Image image)
        {
            image = FileIOHelper.ResizeImage(image, 40);
            var pixels = ConvertBitmap(image);
            return Blurhash.Core.Encode(pixels, 3, 4);
        }

        /// <summary>
        /// Converts the given bitmap to the library-independent representation used within the Blurhash-core
        /// </summary>
        /// <param name="sourceBitmap">The bitmap to encode</param>
        internal static unsafe Pixel[,] ConvertBitmap(Image sourceBitmap)
        {
            var width = sourceBitmap.Width;
            var height = sourceBitmap.Height;

            using var temporaryBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(temporaryBitmap))
            {
                graphics.DrawImageUnscaled(sourceBitmap, 0, 0);
            }

            // Lock the bitmap's bits.
            var bmpData = temporaryBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, temporaryBitmap.PixelFormat);

            // Get the address of the first line.
            var ptr = bmpData.Scan0;

            var result = new Pixel[width, height];

            byte* rgb = (byte*)ptr.ToPointer();
            for (var y = 0; y < height; y++)
            {
                var index = bmpData.Stride * y;

                for (var x = 0; x < width; x++)
                {
                    ref var res = ref result[x, y];
                    res.Blue = MathUtils.SRgbToLinear(rgb[index++]);
                    res.Green = MathUtils.SRgbToLinear(rgb[index++]);
                    res.Red = MathUtils.SRgbToLinear(rgb[index++]);
                }
            }

            temporaryBitmap.UnlockBits(bmpData);

            return result;
        }

        /// <summary>
        /// Создает пак карточек
        /// </summary>
        /// <param name="packDto">Модель пака</param>
        public void CreatePack(CardPackPostDTO packDto)
        {
            Guid fileId = SaveInFileTable(packDto.CoverImage);
            string previewHash;
            using (MemoryStream ms = new())
            {
                packDto.CoverImage.CopyTo(ms);

                using Image coverImg = Image.FromStream(ms);
                previewHash = GetPreviewHash(coverImg);
            }

            string[] chances = packDto.CardDistribution.Split(';');

            string cardDistrib = chances[0];
            int curChance = int.Parse(chances[0]);
            for (int i = 1; i < chances.Length; i++) 
            {
                curChance += int.Parse(chances[i]);
                cardDistrib += ";" + curChance.ToString();
            }

            var packModel = new CardPack()
            {
                Id = Guid.NewGuid(),
                Description = packDto.Description,
                CardDistribution = cardDistrib,
                CardCount = packDto.CardCount,
                CoverImageId = fileId,
                PreviewHash = previewHash,
                SeasonId = packDto.SeasonId,
                Price = packDto.Price,
                Tag = packDto.Tag,
            };

            _context.CardPacks.Add(packModel);
            _context.SaveChanges();
        }
    }
}
