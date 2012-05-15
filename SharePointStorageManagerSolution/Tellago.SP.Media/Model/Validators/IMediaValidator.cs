using System;
namespace Tellago.SP.Media.Model.Validators
{
    interface IMediaValidator
    {
        void ValidateLength(TimeSpan length);

        void ValidateFileSize(int fileSizeInBytes);
    }
}
