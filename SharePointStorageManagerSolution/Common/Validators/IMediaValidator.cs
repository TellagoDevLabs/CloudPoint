using System;
namespace SPSM.Common.Media.Validators
{
    public interface IMediaValidator
    {
        void ValidateLength(TimeSpan length);

        void ValidateFileSize(int fileSizeInBytes);
    }
}
