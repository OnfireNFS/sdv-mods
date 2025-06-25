using StardewModdingAPI;

namespace CompanionFramework.Framework;

public static class Constants
{
    public static readonly ISemanticVersion MinHostVersion = new SemanticVersion(1, 0, 0);
    public const string DialogApprove = "CompanionFramework.Dialog.Approve";
    public const string DialogReject = "CompanionFramework.Dialog.Reject";
}