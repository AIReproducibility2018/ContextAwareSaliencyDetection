namespace MyImplementation
{
    public struct DataSet
    {
        public string InputPath, OutputPath;

        public DataSet(bool is14)
        {
            var path = is14
                ? "D:/Development/Master/Context-Aware Saliency Detection/Database 14/"
                : "D:/Development/Master/Context-Aware Saliency Detection/Database 17/";
            InputPath = path + "Input/";
            OutputPath = path + "Output/";
        }
    }
}