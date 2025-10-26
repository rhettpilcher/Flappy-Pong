// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("agiAksNQdN0iwmuK2v9E5JJocvOy9aJtq8ObiRfouJ+jJjZmznShSYCn1koGYAqAgjVYgOmSHRsL64P6Tlx/i24tFqPF1LXyeyf+t+pOqJjvaOEubOxP7CIkf9s2qNQQuaqsqnY+MfgcmwZoqmLvG7N3NcpVmvDgUuBjQFJvZGtI5CrklW9jY2NnYmHYoVsR+4p5acnFzZBMwbehzHP5dt6KD2f9OMIyrJ/j+KR2mhD1Hjso4GNtYlLgY2hg4GNjYsc6YXdbwLzjxlpHoVAhJjrhBR9WwNkq1Fddmj98H/GYdm6kjnJbTyiXgurpdddx5Gtu4xOLIhQpDSKfcdBGJZc0p9mJfE23QMBPVN29UkalgDHyD9MIvCOSIKC8CNMLq2BhY2Jj");
        private static int[] order = new int[] { 11,2,6,10,12,9,11,9,10,11,10,11,12,13,14 };
        private static int key = 98;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
