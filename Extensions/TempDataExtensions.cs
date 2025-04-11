using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FliesProject.Extensions
{
    public static class TempDataExtensions
    {
        private const string ErrorMessageKey = "ErrorMessage";

        public static void SetErrorMessage(this ITempDataDictionary tempData, string message)
        {
            tempData[ErrorMessageKey] = message;
        }

        public static string GetErrorMessage(this ITempDataDictionary tempData)
        {
            return tempData[ErrorMessageKey]?.ToString();
        }

        public static bool HasErrorMessage(this ITempDataDictionary tempData)
        {
            return tempData.ContainsKey(ErrorMessageKey) && tempData[ErrorMessageKey] != null;
        }
    }
}
