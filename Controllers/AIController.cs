using FliesProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Data;

namespace FliesProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("query")]
        public async Task<IActionResult> GenerateQuery([FromBody] QueryRequest request)
        {
            try
            {
                _logger.LogInformation("Received query request: {Question}", request.Question);
                var (query, explanation) = await _aiService.GenerateSqlQuery(request.Question, request.DatabaseInfo);
                return Ok(new { query, explanation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SQL query");
                return StatusCode(500, new { error = "Failed to generate SQL query" });
            }
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeResult([FromBody] AnalysisRequest request)
        {
            try
            {
                _logger.LogInformation("Received analysis request: {Question}", request.Question);
                var analysis = await _aiService.AnalyzeQueryResult(request.Question, request.Result);
                return Ok(new { analysis });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing query result");
                return StatusCode(500, new { error = "Failed to analyze query result" });
            }
        }

        [HttpPost("chart")]
        public async Task<IActionResult> SuggestChart([FromBody] ChartRequest request)
        {
            try
            {
                _logger.LogInformation("Received chart request");
                var (chartType, chartData) = await _aiService.GenerateChartSuggestion(request.Data, request.Analysis);
                return Ok(new { chartType, chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting chart");
                return StatusCode(500, new { error = "Failed to suggest chart" });
            }
        }
    }

    public class QueryRequest
    {
        public string Question { get; set; }
        public Dictionary<string, object> DatabaseInfo { get; set; }
    }

    public class AnalysisRequest
    {
        public string Question { get; set; }
        public DataTable Result { get; set; }
    }

    public class ChartRequest
    {
        public DataTable Data { get; set; }
        public string Analysis { get; set; }
    }
} 