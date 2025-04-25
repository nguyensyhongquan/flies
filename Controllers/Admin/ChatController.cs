using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using FliesProject.Models.AImodel;
using FliesProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Models.Enums;

namespace FliesProject.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "admin")]
    public class ChatController : ControllerBase
    {
        private readonly ChatRouterService _chatRouterService;
        private readonly ILogger<ChatController> _logger;
        private readonly IIntentClassificationService _intentService;

        public ChatController(ChatRouterService chatRouterService, ILogger<ChatController> logger, IIntentClassificationService intentService)
        {
            _chatRouterService = chatRouterService;
            _logger = logger;
            _intentService = intentService;
        }

        // POST: /api/chat/message
        [HttpPost("message")]
        public async Task<ActionResult<ChatRequest>> ProcessMessage([FromBody] ChatRequest req)
        {
            Console.WriteLine("hiiiiiiiiii");
 
            if (string.IsNullOrWhiteSpace(req?.Message))
                return BadRequest(new ChatResponse
                {
                    IsError = true,
                    ErrorMessage = "Message is required."
                });

            _logger.LogInformation("Start processing message: {Message}", req.Message);
       
            try
            {
  

                var reply = await _chatRouterService.RouteQuestion(req.Message);
                Console.WriteLine("The replyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy"+reply);

             
                var intent = await _intentService.ClassifyAsync(req.Message);
                if(intent == "DATABASE")
                {
                    string chartType = GetValue(reply, "ChartType");
                    List<string> strings;
                    string column = GetValue2(reply, "Column");
                    Console.WriteLine("caicolummmmmmmmmmn"+column);
                    string[] columnlist;
                   
                    string cdmm = GetValue2(reply, "Data");
                    var rowMatches = Regex.Matches(cdmm, @"\[(.*?)\]");
                    var rows = new List<string[]>();

                    foreach (Match rowMatch in rowMatches)
                    {
                        var rowData = rowMatch.Groups[1].Value
                            .Split(',')
                            .Select(d => d.Trim())
                            .ToArray();
                        rows.Add(rowData);
                    }

                    // Print out the rows
                    foreach (var row in rows)
                    {
                        Console.WriteLine("tau den day");
                        Console.WriteLine(string.Join(", ", row));
                    }
                    Console.WriteLine("cdmmmmmmmmmmmmmm"+cdmm);
                    string columcn = GetValue(reply, "Column");
                    columnlist = columcn.Split(',').Select(c => c.Trim()).ToArray();
                    foreach (var columnName in columnlist)
                    {
                        Console.WriteLine("Checcccccccccccck colmn");
                        Console.WriteLine("The column name is" + columnName);
                    }
                    Console.WriteLine("the column is"+columcn);
                    string cancreatetable = GetValue(reply, "CanCreateTable");
                    string prefer = GetValue(reply, "PreferTableorchart");

                    ChatResponse chatResponse = new ChatResponse();
                    DataTable tableData = new DataTable();
                    tableData.Columns.AddRange(columnlist.Select(c => new DataColumn(c)).ToArray());

                    // Replace the following line:  
                    // tableData.Rows.AddRange(rows.Select(r => new object[] { r }).ToArray());  

                    // With this corrected code:  
                    foreach (var row in rows)
                    {
                        var dataRow = tableData.NewRow();
                        for (int i = 0; i < row.Length; i++)
                        {
                            dataRow[i] = row[i];
                            Console.WriteLine("22222222222222" + dataRow[i]);
                        }
                        tableData.Rows.Add(dataRow);
                    }
                    //summary:In ra datatable
                    
                    var response = chatResponse.ParseReply(reply);
                    Console.WriteLine("=-=-=-="+response.ChartData);
                    ChatResponse chatResponse1 = response;
                    if(cancreatetable == "Yes")
                    {
                        
                    }
                    else
                    {
                        chatResponse1.TableData = null;
                    }
                    if (chartType == "No"||(prefer=="Table"))
                    {
                        chatResponse1.ChartType = null;
                    }
                    else
                    {
                        chatResponse1.ChartType = chartType;
                    }

                        Console.WriteLine("The message issssssssssssssss "+chatResponse1.Message);
                    if (!string.IsNullOrEmpty(response.ChartType))
                    {
                        var chartData = GenerateChartData(response.TableData);
                        response.ChartData = JsonSerializer.Serialize(chartData);
                    }
                    return Ok(new ChatResponse
                    {
                        
                        Message = chatResponse1.Message,
                        ChartType = chatResponse1.ChartType,
                        ChartData = chatResponse1.ChartData,
                        IsError = chatResponse1.IsError,
                        TableData = chatResponse1.TableData
                    });
                }
              
               
                return Ok(new ChatResponse
                {
                    Message = reply,
                    ChartType = null,
                    ChartData = null,
                    IsError = false
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message for: {Message}", req.Message);
                return StatusCode(500, new ChatResponse
                {
                    IsError = true,
                    ErrorMessage = "Có lỗi xảy ra khi xử lý câu hỏi của bạn."
                });
            }
        }
        private object GenerateChartData(TableData tableData)
        {
            // Tạo dữ liệu chart dựa trên TableData
            return new
            {
                labels = tableData.Rows.Select(r => r[0]).ToArray(), // Giả sử cột đầu là label
                datasets = new[]
                {
            new
            {
                label = tableData.ColumnNames[1], // Giả sử cột thứ 2 là dữ liệu
                data = tableData.Rows.Select(r => r[1]).ToArray()
            }
        }
            };
        }
        private string GetValue(string reply, string key)
        {
            var match = Regex.Match(reply, $@"<{key}>:\s*\[(.*?)\]</{key}");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        private string GetValue2(string reply, string key)
        {
            var match = Regex.Match(reply, $@"<{key}>:\s*(\[(.*?(\[.*?\],?)+.*?)\])\s*</{key}>");

            // If there's a match, return the value, otherwise return an empty string
           
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

    }
}
