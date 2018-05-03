using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.DockerImageBuilder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.DockerImageBuilder.Controllers
{
    [Route("api/[controller]")]
    public class BuildController : Controller
    {
        private readonly IImageBuilderFactory _imageBuilderFactory;
        private readonly IBuildDataCleaner _buildDataCleaner;
        private readonly ILog _log;

        public BuildController(
            IImageBuilderFactory imageBuilderFactory,
            IBuildDataCleaner buildDataCleaner,
            ILog log)
        {
            _imageBuilderFactory = imageBuilderFactory;
            _buildDataCleaner = buildDataCleaner;
            _log = log;
        }

        /// <summary>
        /// Checks service is alive
        /// </summary>
        [HttpPost]
        [SwaggerOperation("DockerWinImage")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> WindowsImage(
            string gitRepoUrl,
            string commitId,
            string fullImageName,
            string dockerFilePath)
        {
            if (string.IsNullOrWhiteSpace(gitRepoUrl))
                return BadRequest(ErrorResponse.Create($"Input parameter {nameof(gitRepoUrl)} is empty"));

            if (string.IsNullOrWhiteSpace(commitId))
                return BadRequest(ErrorResponse.Create($"Input parameter {nameof(commitId)} is empty"));

            if (string.IsNullOrWhiteSpace(fullImageName))
                return BadRequest(ErrorResponse.Create($"Input parameter {nameof(fullImageName)} is empty"));

            if (string.IsNullOrWhiteSpace(dockerFilePath))
                return BadRequest(ErrorResponse.Create($"Input parameter {nameof(dockerFilePath)} is empty"));

            var winImageBuilder = _imageBuilderFactory.CreateWinImageBuilder(gitRepoUrl);

            try
            {
                winImageBuilder.FetchSources(commitId);

                winImageBuilder.BuildAndPublishApp();

                winImageBuilder.BuildDockerImage(fullImageName);

                winImageBuilder.PublishDocker(fullImageName);

                _buildDataCleaner.CleanUp(winImageBuilder.BuildDirectory);

                return Ok();
            }
            catch (Exception ex)
            {
                _buildDataCleaner.CleanUp(winImageBuilder.BuildDirectory);

                _log.WriteError(nameof(BuildController), new { gitRepoUrl, commitId, fullImageName, dockerFilePath }, ex);

                return StatusCode((int)HttpStatusCode.InternalServerError, ErrorResponse.Create(ex.Message));
            }
        }
    }
}
