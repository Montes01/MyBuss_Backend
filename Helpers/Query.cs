using System.Text;
namespace API.Helpers

{
    public static class Query
    {
        public static string Make(string Usp, string[] parameters)
        {
            StringBuilder Query = new($"EXEC {Usp} ");
            foreach (string param in parameters)
            {
                Query.Append(param + ",");
            }

            return Query.ToString()[..(Query.Length - 1)];
        }
    }
    

}
