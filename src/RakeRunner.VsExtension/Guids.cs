// Guids.cs
// MUST match guids.h
using System;

namespace Company.RakeRunner_VsExtension
{
    static class GuidList
    {
        public const string guidRakeRunnerPkgString = "250f7ad3-190d-489e-b8f9-9a0604c2ac72";
        public const string guidRakeRunnerListTasksCmdSetString = "c78d067c-81eb-42ee-b3c6-5ad5ebec72ae";

        public static readonly Guid guidRakeRunnerCmdSet = new Guid(guidRakeRunnerListTasksCmdSetString);
    };
}