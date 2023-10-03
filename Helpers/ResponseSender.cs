namespace API.Helpers
{
    public class ResponseSender
    {
        public string Status { get; set; }
        public object Response { get; set; }
        public ResponseSender(string _Status, object _Response)
        {
            this.Status = _Status;
            this.Response = _Response;
        }
    }
}
