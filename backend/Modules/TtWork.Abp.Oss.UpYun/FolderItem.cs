namespace TtWork.Abp.Oss.UpYun
{
    public class FolderItem
    {
        public string filename;
        public string filetype;
        public int size;
        public int number;

        public FolderItem(string filename, string filetype, int size, int number)
        {
            this.filename = filename;
            this.filetype = filetype;
            this.size = size;
            this.number = number;
        }
    }
}