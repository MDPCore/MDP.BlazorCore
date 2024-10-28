namespace MDP.BlazorCore
{
    public class InteropResponse
    {
        // Properties
        public InteropStatusCode StatusCode { get; set; } = InteropStatusCode.InternalServerError;

        public object Result { get; set; } = null;

        public string ErrorMessage { get; set; } = null;

        public bool Succeeded
        {
            get
            {
                // StatusCode
                if (this.StatusCode == InteropStatusCode.OK) return true;

                // Return
                return false;
            }
        }
    }
}
