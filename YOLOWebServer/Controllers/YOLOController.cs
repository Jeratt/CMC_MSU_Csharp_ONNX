using Microsoft.AspNetCore.Mvc;
using MyYOLOApi;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Net;
using System.Drawing;
using System.Text;

namespace YOLOWebServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class YOLOController : Controller
    {
        private readonly ModelManager modelManager;

        private readonly WebFileManager fileManager;

        private readonly HtmlWritter writter;

        static CancellationTokenSource cts = new CancellationTokenSource();
        public YOLOController() {
            string modelPath = "https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx";
            
            fileManager = new WebFileManager();
            writter = new HtmlWritter();

            modelManager = new ModelManager(modelPath, fileManager, writter);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("postImage")]
        public async Task<ActionResult<List<ObjectBox>>> postImage([FromBody] string data, CancellationToken ctn)
        {
            try
            {
/*                byte[] image = Convert.FromBase64String(data);
                int width, height;
                using (var ms = new MemoryStream(image))
                {
                    System.Drawing.Image img_tmp = System.Drawing.Image.FromStream(ms);
                    width = img_tmp.Width;
                    height = img_tmp.Height;
                }
*/
                Image<Rgb24> img;
                List<ObjectBox> lob;
                byte[] image = Convert.FromBase64String(data);

                using (MemoryStream ms = new MemoryStream(image))
                {
                    img = SixLabors.ImageSharp.Image.Load<Rgb24>(ms);
                }

                //img = Image.Load(image);
                //img = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(Encoding.ASCII.GetBytes(data), width, height);
                lob = await modelManager.PredictAsync(img, cts.Token); // TODO asynchrous code here ?
                return base.StatusCode((int)HttpStatusCode.OK, lob);
            }
            catch(Exception ex)
            {
                return base.StatusCode((int)HttpStatusCode.NotImplemented, ex.Message);
            }
        }
    }
}
