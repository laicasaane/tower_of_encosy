#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.IO;

namespace EncosyTower.UserDataVaults
{
    public delegate string MakeFilePathFunc(
          RootPath rootPath
        , string subFolderName
        , string fileName
        , string fileExtension
        , Type userDataType
    );
}

#endif
