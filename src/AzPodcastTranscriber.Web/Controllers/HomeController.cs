namespace AzPodcastTranscriber.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using AzPodcastTranscriber.Shared;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    // I am not exactly proud of these fat controllers. Have to refactor this code later. 
    // Until then, I'll add this to the list of other things I am not proud of.

    public class HomeController : Controller
    {
        private readonly DocumentDBRepository<TranscriptionResult> _cosmosRepository;

        public HomeController(IOptions<AppSettings> appsettings)
        {
            _cosmosRepository = new DocumentDBRepository<TranscriptionResult>
                                        (appsettings.Value.CosmosDBDatabaseName,
                                          appsettings.Value.CosmosDBCollectionName,
                                          appsettings.Value.CosmosDBEndpoint,
                                          appsettings.Value.CosmosDBAuthKey);

        }

        [ResponseCache(Duration = 3000, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index()
        {
            var transcriptions = await _cosmosRepository.GetItemsAsync();

            return View(transcriptions.OrderByDescending(m => m.PublishDate));


        }

        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "id" })]
        public async Task<IActionResult> Details(string id)
        {
            // I know, I know. Human Beings should not see / read GUID's even in query strings.
            var podcastTranscription = await _cosmosRepository.GetItemAsync(id);
            return View(podcastTranscription);

        }

        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "id" })]
        public async Task<IActionResult> Edit(string id)
        {
            // Something feels wrong with the view.

            var transcription = await _cosmosRepository.GetItemAsync(id);
            return View(transcription);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TranscriptionResult transcription)
        {
            //await _cosmosRepository.UpdateItemAsync(transcription.Id, transcription);
            return RedirectToAction("Details", routeValues: new { id = transcription.Id });
            //Not Today.
        }
    }
}
