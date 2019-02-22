namespace GCommon
{
    public class ResUpdaterProgressMonitor
    {
        private long m_TotalSizeInByte = 0;
        public long TotalSizeInByte
        {
            get
            {
                return m_TotalSizeInByte;
            }
        }
        private long m_TotalLoadedSizeInByte = 0;
        public long TotalLoadedSizeInByte
        {
            get
            {
                return m_TotalLoadedSizeInByte;
            }
        }
        public void Clear()
        {
            m_TotalSizeInByte = 0;
            m_TotalLoadedSizeInByte = 0;
        }
        public void AddLoaderInfo(long fileSize)
        {
            m_TotalSizeInByte += fileSize;
        }
        public void OnLoadFinished(long loadedSize)
        {
            m_TotalLoadedSizeInByte += loadedSize;
        }
    }
}
