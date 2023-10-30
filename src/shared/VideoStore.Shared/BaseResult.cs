namespace VideoStore.Shared
{
    public class BaseResult
    {
        public bool Success => Errors.Count == 0;
        public List<string> Warnings { get; private set; } = new List<string>();
        public List<string> Errors { get; private set; } = new List<string>();

        public BaseResult AddError(string errorMessage)
        {
            Errors.Add(errorMessage);
            return this;
        }

        public BaseResult AddWarning(string errorMessage)
        {
            Warnings.Add(errorMessage);
            return this;
        }
    }
}
