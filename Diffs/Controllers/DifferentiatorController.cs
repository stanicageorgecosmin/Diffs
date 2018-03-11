using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using BLL;
using BusinessSupport;

namespace Diffs.Controllers
{
    public class DifferentiatorController : ApiController
    {
        /// <summary>
        /// Sets the left part asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [HandleError]
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
        [System.Web.Http.HttpPost]
        [HandleError]
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
        [HandleError]
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
