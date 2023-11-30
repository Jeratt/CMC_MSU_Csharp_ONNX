using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using YOLOWebServer;
using System.Net.Http.Json;

namespace YOLOWebTest
{
    public class YOLOControllerTest: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public YOLOControllerTest(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task postImageOkTest()
        {
            var client = factory.CreateClient();
            string sampleImg = Convert.ToBase64String(File.ReadAllBytes(@"C:\pics\cat.jpg"));
            var response = await client.PostAsJsonAsync("https://localhost:7296/YOLO/postImage", sampleImg);
            var answer = await response.Content.ReadAsStringAsync();
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task postImageErrorTest()
        {
            var client = factory.CreateClient();
            string brokenImg = "TEST";
            var response = await client.PostAsJsonAsync("https://localhost:7296/YOLO/postImage", brokenImg);
            var answer = await response.Content.ReadAsStringAsync();
            Assert.Equal(501, (int)response.StatusCode);
        }

        [Fact]
        public async Task postImageResultTest()
        {
            var client = factory.CreateClient();
            string sampleImg = Convert.ToBase64String(File.ReadAllBytes(@"C:\pics\Kittens!!.jpg"));
            var response = await client.PostAsJsonAsync("https://localhost:7296/YOLO/postImage", sampleImg);
            var answer = await response.Content.ReadAsStringAsync();
            List<DetectedData> ldd = JsonConvert.DeserializeObject<List<DetectedData>>(answer);
            Assert.Equal(200, (int)response.StatusCode);
            Assert.Equal(2, ldd.Count());
            Assert.Equal(7, ldd[0].Class);
            Assert.Equal(7, ldd[1].Class);
        }
    }

    internal record DetectedData
    {
        public double XMin { get; init; }
        public double YMin { get; init; }
        public double XMax { get; init; }

        public double YMax { get; init; }
        public double Confidence { get; init; }
        public int Class {get; init;}
        public DetectedData(double XMin, double YMin, double XMax, double YMax, double Confidence, int Class)
        {
            this.XMin = XMin;
            this.YMin = YMin;
            this.XMax = XMax;
            this.YMax = YMax;
            this.Confidence = Confidence;
            this.Class = Class;
        }
    }
}