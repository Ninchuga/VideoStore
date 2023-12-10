namespace VideoStore.Shared
{
    public class Result
    {
        public bool Success => Errors.Count == 0;
        public List<string> Warnings { get; private set; } = new List<string>();
        public List<string> Errors { get; private set; } = new List<string>();

        public Result AddError(string errorMessage)
        {
            Errors.Add(errorMessage);
            return this;
        }

        public Result AddWarning(string errorMessage)
        {
            Warnings.Add(errorMessage);
            return this;
        }
    }
}
