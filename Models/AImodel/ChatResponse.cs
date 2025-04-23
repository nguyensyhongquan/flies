using Microsoft.EntityFrameworkCore.Metadata.Internal;
using FliesProject.Models.AImodel;
using System.Text.RegularExpressions;
namespace FliesProject.Models.AImodel
{
    public class ChatResponse
    {
        public string Message { get; set; }
        public string? ChartData { get; set; }
        public string? ChartType { get; set; }
        public bool IsError { get; set; }
        public string? ErrorMessage { get; set; }
        public TableData TableData { get; set; }
        public string? IsCreateTableorChat { get; set; }
        public ChatResponse()
        {

        }
        public ChatResponse ParseReply(string reply)
        {
            var response = new ChatResponse();

            // Parse message
            var messageMatch = Regex.Match(reply, @"<Message>:\s*(.*?)\s*(?=\n|$)");
            if (messageMatch.Success)
            {
                response.Message = messageMatch.Groups[1].Value;
            }

            // Parse columns
            var columnMatch = Regex.Match(reply, @"<Column>:\s*\[(.*?)\]");
            var columns = columnMatch.Success
                ? columnMatch.Groups[1].Value.Split(',').Select(c => c.Trim()).ToArray()
                : new string[0];

            // Parse data rows
            var dataMatch = Regex.Match(reply, @"<Data>:\s*(\[(.*?)\])\s*</Data>");
            var rows = new List<string[]>();
            if (dataMatch.Success)
            {
                var dataStr = dataMatch.Groups[1].Value;

                // Match each row inside the data list
                var rowMatches = Regex.Matches(dataStr, @"\[(.*?)\]");

                // Iterate through each row match
                foreach (Match rowMatch in rowMatches)
                {
                    // Get the row data inside each square bracket
                    var rowData = rowMatch.Groups[1].Value
                        .Split(',')
                        .Select(d => d.Trim()) // Trim whitespace
                        .ToArray();

                    // Add to the rows list
                    rows.Add(rowData);
                }
            }
            else
            {
                Console.WriteLine("bomeeeeeeeeeeeeeeeeygadfyuejwghvFHHHHHHHHe");
            }
            if(rows.Count==0)
            {
                Console.WriteLine("câiditremaetanjkankfja");
            }    
            
            // Print rows
            foreach (var row in rows)
            {
                Console.WriteLine("câcdjnhjkbnhjkvbhjabvhjkabfvhjvbahjfbafhFASEEEEEEEEEEEEEEEEEEEEEb");
                Console.WriteLine(string.Join(", ", row));
            }

            // Set table data
            response.TableData = new TableData
            {
                ColumnNames = columns,
                Rows = rows
            };

            // Parse chart preference
            var chartMatch = Regex.Match(reply, @"<ChartType>:\s*\[(.*?)\]");
            if (chartMatch.Success && chartMatch.Groups[1].Value.ToLower() != "no")
            {
                response.ChartType = chartMatch.Groups[1].Value;
            }

            // Parse error status
            var errorMatch = Regex.Match(reply, @"<IsError>:\s*\[(.*?)\]");
            response.IsError = errorMatch.Success &&
                              errorMatch.Groups[1].Value.ToLower() == "yes";

            return response;
        }
        public override string ToString()
        {
            return $"Message: {Message}, ChartType: {ChartType}, ChartData: {ChartData}, IsError: {IsError}, ErrorMessage: {ErrorMessage}+{TableData.ToString
                }";
        }
    }
    public class TableData
    {
        public string[] ColumnNames { get; set; }
        public List<string[]> Rows { get; set; }
        public override string ToString()
        {
            return base.ToString();
        }
    }
    
}

