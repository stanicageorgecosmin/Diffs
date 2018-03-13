using System.Threading.Tasks;
using System.Web.Http;
using BLL;
using BusinessSupport;

namespace DiffsApi.ApiControllers
{
    public class DifferentiatorController : ApiController
    {
        /// <summary>
        /// Sets the left part asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Feedback> SetLeftPartAsync([FromUri] int id)
        {
            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                using (var processor = new DataSetterProcessor(DiffPartEnum.Left))
                {
                    var feedback = await processor.TrySetDataForKeyIdentifierAsync(id, stream);
                    return feedback;
                }
            }
        }

        /// <summary>
        /// Sets the right part asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Feedback> SetRightPartAsync([FromUri] int id)
        {
            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                using (var processor = new DataSetterProcessor(DiffPartEnum.Right))
                {
                    var feedback = await processor.TrySetDataForKeyIdentifierAsync(id, stream);
                    return feedback;
                }
            }
        }

        /// <summary>
        /// Gets the difference results asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<CalculatedDiffsFeedback> GetDiffResultsAsync([FromUri] int id)
        {
            using (var processor = new DiffProcessor())
            {
                var feedback = await processor.TryGetDiffsForKeyIdentifierAsync(id);
                return feedback;
            }
        }
    }
}
